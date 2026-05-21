using AdminHelper.Logger;
using AdminHelper.Models;
using AdminHelper.Scripts;
using AdminHelper.Services;
using AdminHelper.Utils;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using static AdminHelper.Scripts.SystemTools;

namespace AdminHelper
{
    public partial class MainForm : Form
    {
        private readonly ILogger _logger;
        private IMonitoringService _monitoringService;
        private System.Windows.Forms.Timer _updateTimer;
        private bool _isMonitoring = false;
        private List<ScriptItem> _scripts;
        private readonly SystemTools _systemTools;
        private bool _isShutdownScheduled = false;
        private System.Windows.Forms.Timer _shutdownCheckTimer;

        private System.Windows.Forms.Timer _shutdownCountdownTimer;
        private DateTime _shutdownScheduledAt;
        private int _shutdownMinutes = 5;

        public MainForm()
        {
            InitializeComponent();

            // 1. Сначала сервисы
            _logger = new LoggerService(rtbLogger);
            _systemTools = new SystemTools(_logger);   // BugFix: единственный экземпляр
            _monitoringService = new MonitoringService(_logger);
            _monitoringService.DataUpdated += OnMonitoringDataUpdated;

            // 2. Потом UI
            _scripts = new List<ScriptItem>();
            InitializeScripts();

            // 3. Проверка состояния выключения (не вызывает side-effect теперь)
            RefreshShutdownState(logChange: false);
            StartShutdownCheckTimer();

            // 4. Проверка прав
            CheckAdminRights();

            // 5. Подписка на событие смены вкладок
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            // 6. В конце логирование
            _logger.Info("Приложение запущено");
            _logger.Info($"Папка для логов: {FileUtils.GetLogsFolderPath()}");

            // 7. Мониторинг НЕ запускаем автоматически
            btnStartMonitoring.Enabled = true;
            btnStopMonitoring.Enabled = false;
        }

        // ========== ИНИЦИАЛИЗАЦИЯ СКРИПТОВ ==========

        private void InitializeScripts()
        {
            _scripts.Add(new ScriptItem
            {
                Id = 1,
                Name = "CleanTemp",
                Description = "Очистка временных файлов",
                Status = "Stopped",
                Type = "Built-in",
                IsRunning = false
            });
            _scripts.Add(new ScriptItem
            {
                Id = 2,
                Name = "RestartService",
                Description = "Перезапуск службы Windows",
                Status = "Stopped",
                Type = "Built-in",
                IsRunning = false
            });
            _scripts.Add(new ScriptItem
            {
                Id = 3,
                Name = "Backup",
                Description = "Резервное копирование папки",
                Status = "Stopped",
                Type = "Built-in",
                IsRunning = false
            });

            UpdateScriptsGrid();
        }

        private void UpdateScriptsGrid()
        {
            dataGridViewScripts.Rows.Clear();
            foreach (var script in _scripts)
            {
                int rowIndex = dataGridViewScripts.Rows.Add(
                    script.Name,
                    script.Description,
                    script.Status,
                    script.Type,
                    script.IsRunning ? "⏹ Stop" : "▶ Start");

                var row = dataGridViewScripts.Rows[rowIndex];
                var buttonCell = (DataGridViewButtonCell)row.Cells["colScriptControl"];
                buttonCell.Value = script.IsRunning ? "⏹ Stop" : "▶ Start";
                buttonCell.Style.BackColor = script.IsRunning ? Color.IndianRed : Color.SeaGreen;
                buttonCell.Style.ForeColor = Color.White;
                buttonCell.Style.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);

                // Подсветка строки запущенного скрипта
                row.DefaultCellStyle.BackColor = script.IsRunning
                    ? Color.FromArgb(255, 240, 240)
                    : SystemColors.Window;
            }
        }

        // ========== ПРАВА ДОСТУПА ==========

        private void CheckAdminRights()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            lblRights.Text = isAdmin ? "Administrator" : "User";
            lblRights.ForeColor = isAdmin ? Color.Green : Color.Red;
            btnAdminRights.Enabled = !isAdmin;
            btnAdminRights.BackColor = isAdmin ? Color.Gray : Color.SteelBlue;
        }

        // ========== ВЫКЛЮЧЕНИЕ ПК ==========

        private bool CheckShutdownViaWMI()
        {
            try
            {
                var shutdownProcs = Process.GetProcessesByName("shutdown");
                if (shutdownProcs.Length > 0) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void RefreshShutdownState(bool logChange = true)
        {
            bool wasScheduled = _isShutdownScheduled;

            _isShutdownScheduled = CheckShutdownViaWMI();

            if (logChange && wasScheduled != _isShutdownScheduled)
            {
                if (!_isShutdownScheduled)
                    _logger.Info("Таймер выключения был отменён внешним источником");
                else
                    _logger.Warning("Обнаружен запланированный таймер выключения!");
            }

            UpdateShutdownUI();
        }

        private void UpdateShutdownUI()
        {
            if (_isShutdownScheduled)
            {
                btnOffPc.Enabled = false;
                btnOffPc.BackColor = Color.Gray;
                btnCancelShutdown.Enabled = true;
                btnCancelShutdown.BackColor = Color.Crimson;
            }
            else
            {
                btnOffPc.Enabled = true;
                btnOffPc.BackColor = Color.DarkOrange;
                btnCancelShutdown.Enabled = false;
                btnCancelShutdown.BackColor = Color.Gray;
                lblShutdownStatus.Text = "Нет активного таймера";
                lblShutdownStatus.ForeColor = Color.SeaGreen;
            }
        }

        private void StartShutdownCheckTimer()
        {
            _shutdownCheckTimer = new System.Windows.Forms.Timer();
            _shutdownCheckTimer.Interval = 5000;
            _shutdownCheckTimer.Tick += ShutdownCheckTimer_Tick;
            _shutdownCheckTimer.Start();
        }

        private void ShutdownCheckTimer_Tick(object sender, EventArgs e)
        {
            RefreshShutdownState(logChange: true);
        }

        private void StartShutdownCountdown()
        {
            _shutdownScheduledAt = DateTime.Now;
            _shutdownCountdownTimer?.Stop();
            _shutdownCountdownTimer?.Dispose();
            _shutdownCountdownTimer = new System.Windows.Forms.Timer();
            _shutdownCountdownTimer.Interval = 1000;
            _shutdownCountdownTimer.Tick += ShutdownCountdownTimer_Tick;
            _shutdownCountdownTimer.Start();
        }

        private void ShutdownCountdownTimer_Tick(object sender, EventArgs e)
        {
            if (!_isShutdownScheduled)
            {
                _shutdownCountdownTimer?.Stop();
                return;
            }

            var elapsed = DateTime.Now - _shutdownScheduledAt;
            var remaining = TimeSpan.FromMinutes(_shutdownMinutes) - elapsed;

            if (remaining.TotalSeconds <= 0)
            {
                lblShutdownStatus.Text = "Выключение...";
                lblShutdownStatus.ForeColor = Color.Red;
                _shutdownCountdownTimer?.Stop();
            }
            else
            {
                lblShutdownStatus.Text = $"Выключение через: {remaining:mm\\:ss}";
                lblShutdownStatus.ForeColor = remaining.TotalSeconds < 60 ? Color.Red : Color.OrangeRed;
            }
        }

        // === ОБРАБОТЧИКИ КНОПОК ВЫКЛЮЧЕНИЯ ===

        private void BtnOffPc_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                $"Запланировать выключение через {_shutdownMinutes} минут?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            _systemTools.ScheduleShutdown(_shutdownMinutes);
            _isShutdownScheduled = true;
            UpdateShutdownUI();
            StartShutdownCountdown();
        }

        private void BtnCancelShutdown_Click(object sender, EventArgs e)
        {
            // BugFix: используем _systemTools, не создаём новый экземпляр
            _systemTools.AbortShutdown();
            _isShutdownScheduled = false;
            _shutdownCountdownTimer?.Stop();
            UpdateShutdownUI();
        }

        // Изменение времени выключения через NumericUpDown
        private void nudShutdownMinutes_ValueChanged(object sender, EventArgs e)
        {
            _shutdownMinutes = (int)nudShutdownMinutes.Value;
            btnOffPc.Text = $"OFF PC ({_shutdownMinutes} мин)";
        }

        // ========== МОНИТОРИНГ ==========

        private void StartMonitoring()
        {
            if (_isMonitoring) return;

            _monitoringService.StartMonitoring();

            _updateTimer = new System.Windows.Forms.Timer();
            _updateTimer.Interval = 2000;
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            _isMonitoring = true;
            btnStartMonitoring.Enabled = false;
            btnStartMonitoring.BackColor = Color.Gray;
            btnStopMonitoring.Enabled = true;
            btnStopMonitoring.BackColor = Color.Crimson;

            _logger.Success("Мониторинг ресурсов запущен");
        }

        private void StopMonitoring()
        {
            if (!_isMonitoring) return;

            _monitoringService.StopMonitoring();
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            _updateTimer = null;

            _isMonitoring = false;
            btnStartMonitoring.Enabled = true;
            btnStartMonitoring.BackColor = Color.SeaGreen;
            btnStopMonitoring.Enabled = false;
            btnStopMonitoring.BackColor = Color.Gray;

            _logger.Info("Мониторинг ресурсов остановлен");
        }

        private async void UpdateTimer_Tick(object sender, EventArgs e)
        {
            int selectedTab = tabControl.SelectedIndex;

            await Task.Run(() =>
            {
                if (selectedTab == tabControl.TabPages.IndexOf(tabPageInfoProccess))
                    UpdateProcesses();
                else if (selectedTab == tabControl.TabPages.IndexOf(tabPageInfoService))
                    UpdateServices();
            });
        }

        private async void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPageInfoProccess)
            {
                await Task.Run(() => UpdateProcesses());
            }
            else if (tabControl.SelectedTab == tabPageInfoService)
            {
                await Task.Run(() => UpdateServices());
            }
        }

        private void UpdateProcesses()
        {
            try
            {
                var processes = Process.GetProcesses()
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(150)
                    .Select(p => new
                    {
                        p.ProcessName,
                        MemoryMB = p.WorkingSet64 / 1024 / 1024,
                        Description = SafeGetDescription(p),
                        Status = p.Responding ? "Running" : "Not Responding",
                        Type = "Process"
                    })
                    .ToList();

                Invoke(new Action(() =>
                {
                    dataGridViewProcesses.Rows.Clear();
                    foreach (var proc in processes)
                    {
                        int rowIndex = dataGridViewProcesses.Rows.Add(
                            proc.ProcessName, proc.MemoryMB,
                            proc.Description, proc.Status, proc.Type);

                        // Подсветка зависших процессов
                        if (proc.Status == "Not Responding")
                        {
                            dataGridViewProcesses.Rows[rowIndex]
                                .DefaultCellStyle.ForeColor = Color.Red;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                    _logger.Error($"Ошибка обновления процессов: {ex.Message}")));
            }
        }

        private void UpdateServices()
        {
            try
            {
                var services = ServiceController.GetServices()
                    .OrderBy(s => s.ServiceName)
                    .Take(200)
                    .Select(s => new
                    {
                        s.ServiceName,
                        s.DisplayName,
                        Status = s.Status.ToString(),
                        StartType = GetServiceStartType(s.ServiceName)
                    })
                    .ToList();

                Invoke(new Action(() =>
                {
                    dataGridViewServices.Rows.Clear();
                    foreach (var svc in services)
                    {
                        int rowIndex = dataGridViewServices.Rows.Add(
                            svc.ServiceName, svc.DisplayName,
                            svc.Status, svc.StartType);

                        // Цветовая индикация статуса службы
                        var row = dataGridViewServices.Rows[rowIndex];
                        if (svc.Status == "Running")
                            row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                        else if (svc.Status == "Stopped")
                            row.DefaultCellStyle.ForeColor = Color.Gray;
                        else
                            row.DefaultCellStyle.ForeColor = Color.OrangeRed;
                    }
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                    _logger.Error($"Ошибка обновления служб: {ex.Message}")));
            }
        }

        private string GetServiceStartType(string serviceName)
        {
            try
            {
                using (var managementObject = new ManagementObject($"Win32_Service.Name='{serviceName}'"))
                {
                    return managementObject["StartMode"]?.ToString() ?? "Unknown";
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        private string SafeGetDescription(Process proc)
        {
            try
            {
                return proc.MainModule?.FileVersionInfo?.FileDescription ?? "";
            }
            catch
            {
                return "";
            }
        }

        private void OnMonitoringDataUpdated(object sender, SystemInfo info)
        {
            if (InvokeRequired)
                Invoke(new Action(() => UpdateMonitoringDisplay(info)));
            else
                UpdateMonitoringDisplay(info);
        }

        private void UpdateMonitoringDisplay(SystemInfo info)
        {
            // ── CPU ──────────────────────────────────────────────────────
            var cpu = info.Cpu;
            var sb = new StringBuilder();

            sb.AppendLine($"{'─',0}{'─',0} {cpu.Name} {'─',0}".PadRight(36, '─'));
            sb.AppendLine($"Ядра:        {cpu.PhysicalCores}P / {cpu.LogicalCores}L");
            sb.AppendLine($"Загрузка:    {cpu.TotalLoad:F1}%");
            sb.AppendLine($"Температура: {cpu.Temperature:F1}°C");
            sb.AppendLine($"Частота:     {cpu.AverageFrequencyGHz:F2} GHz (ср.)");
            if (cpu.PackagePower > 0)
                sb.AppendLine($"Мощность:    {cpu.PackagePower:F1} W (Package)");
            if (cpu.CoresPower > 0)
                sb.AppendLine($"             {cpu.CoresPower:F1} W (Cores)");
            if (cpu.VCore > 0)
                sb.AppendLine($"VCore:       {cpu.VCore:F3} V");

            // Частоты по ядрам (если есть)
            if (cpu.CoreClocks.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Ядра (частота / нагрузка / темп):");
                int count = cpu.CoreClocks.Count;
                for (int i = 0; i < count; i++)
                {
                    double mhz = cpu.CoreClocks[i].FrequencyMHz;
                    double load = i < cpu.CoreLoads.Count ? cpu.CoreLoads[i] : 0;
                    double temp = i < cpu.CoreTemperatures.Count ? cpu.CoreTemperatures[i] : 0;
                    string core = $"Core #{i}";
                    sb.AppendLine($"  {core,-8} {mhz,7:F0} MHz  {load,5:F1}%  {temp,4:F1}°C");
                }
            }

            tbMonitorCPU.Text = sb.ToString();
            ColorizeTextBox(tbMonitorCPU, cpu.TotalLoad);

            // ── GPU ──────────────────────────────────────────────────────
            sb.Clear();
            if (info.Gpus.Count == 0)
            {
                sb.AppendLine("GPU не обнаружен");
            }
            else
            {
                foreach (var gpu in info.Gpus)
                {
                    sb.AppendLine($"{'─',0} {gpu.Name} {'─',0}".PadRight(36, '─'));
                    sb.AppendLine($"Тип:         {gpu.Vendor} {gpu.GpuType}");
                    if (!string.IsNullOrEmpty(gpu.DriverVersion))
                        sb.AppendLine($"Драйвер:     {gpu.DriverVersion}");

                    sb.AppendLine($"Загрузка:    {gpu.CoreLoad:F1}%");
                    if (gpu.MemoryLoad > 0)
                        sb.AppendLine($"VRAM Load:   {gpu.MemoryLoad:F1}%");
                    if (gpu.VideoEngineLoad > 0)
                        sb.AppendLine($"Video Engine:{gpu.VideoEngineLoad:F1}%");

                    sb.AppendLine($"Температура: {gpu.Temperature:F1}°C");
                    if (gpu.HotSpotTemperature > 0)
                        sb.AppendLine($"Hot Spot:    {gpu.HotSpotTemperature:F1}°C");
                    if (gpu.MemoryTemperature > 0)
                        sb.AppendLine($"VRAM Temp:   {gpu.MemoryTemperature:F1}°C");

                    if (gpu.MemoryTotalMB > 0)
                        sb.AppendLine($"VRAM:        {gpu.MemoryUsedMB:F0} / {gpu.MemoryTotalMB:F0} MB");

                    if (gpu.CoreFrequencyMHz > 0)
                        sb.AppendLine($"Частота GPU: {gpu.CoreFrequencyMHz:F0} MHz");
                    if (gpu.MemoryFrequencyMHz > 0)
                        sb.AppendLine($"Частота VRAM:{gpu.MemoryFrequencyMHz:F0} MHz");

                    // Несколько вентиляторов
                    if (gpu.FanSpeeds.Count == 1)
                    {
                        sb.AppendLine($"Вентилятор:  {gpu.FanSpeeds[0]:F0} RPM");
                        if (gpu.FanSpeedPercent > 0)
                            sb.Append($" ({gpu.FanSpeedPercent:F0}%)");
                    }
                    else if (gpu.FanSpeeds.Count > 1)
                    {
                        for (int i = 0; i < gpu.FanSpeeds.Count; i++)
                            sb.AppendLine($"Fan #{i + 1}:      {gpu.FanSpeeds[i]:F0} RPM");
                    }

                    if (gpu.PowerWatts > 0)
                        sb.AppendLine($"Мощность:    {gpu.PowerWatts:F1} W");
                    if (gpu.CoreVoltage > 0)
                        sb.AppendLine($"GPU Voltage: {gpu.CoreVoltage:F3} V");

                    sb.AppendLine();
                }
            }
            tbMonitoringGPU.Text = sb.ToString();
            ColorizeTextBox(tbMonitoringGPU, info.GpuLoad);

            // ── RAM ───────────────────────────────────────────────────────
            var ram = info.Ram;
            sb.Clear();
            sb.AppendLine($"Тип:          {ram.MemoryType}  {ram.FrequencyMHz:F0} MHz");
            sb.AppendLine($"Использовано: {ram.UsedGB:F2} / {ram.TotalGB:F2} GB");
            sb.AppendLine($"Загрузка:     {ram.LoadPercent:F1}%");
            sb.AppendLine($"Свободно:     {ram.FreeGB:F2} GB");

            if (!string.IsNullOrEmpty(ram.TimingsString))
                sb.AppendLine($"Тайминги:     {ram.TimingsString}");

            if (ram.Modules.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Модули:");
                foreach (var m in ram.Modules)
                {
                    sb.AppendLine($"  [{m.Slot}]");
                    if (!string.IsNullOrEmpty(m.Manufacturer))
                        sb.AppendLine($"    {m.Manufacturer} {m.PartNumber}");
                    sb.AppendLine($"    {m.CapacityGB:F0} GB  {m.SpeedMHz:F0} MHz  {m.FormFactor}");
                    if (m.VoltageV > 0)
                        sb.AppendLine($"    {m.VoltageV:F2} V");
                }
            }
            tbMonitoringRAM.Text = sb.ToString();
            ColorizeTextBox(tbMonitoringRAM, ram.LoadPercent);

            // ── ДИСКИ ─────────────────────────────────────────────────────
            sb.Clear();
            if (info.Disks?.Count > 0)
            {
                foreach (var disk in info.Disks)
                {
                    sb.AppendLine($"[{disk.Name}] {disk.Model}");
                    sb.AppendLine($"  Тип:      {disk.Type}  ({disk.BusType})  {disk.FileSystem}");
                    sb.AppendLine($"  Размер:   {disk.TotalSizeGB:F1} GB");
                    sb.AppendLine($"  Занято:   {disk.UsedSpaceGB:F1} GB  ({disk.UsagePercent:F1}%)");
                    sb.AppendLine($"  Свободно: {disk.FreeSpaceGB:F1} GB");

                    if (disk.Temperature > 0)
                        sb.AppendLine($"  Темп.:    {disk.Temperature:F1}°C");
                    if (disk.HealthPercent >= 0)
                        sb.AppendLine($"  Здоровье: {disk.HealthPercent}%");
                    if (disk.PowerOnHours > 0)
                        sb.AppendLine($"  Наработка:{disk.PowerOnHours} ч  ({disk.PowerOnHours / 24 / 30} мес.)");
                    if (disk.PowerCycles > 0)
                        sb.AppendLine($"  Вкл/выкл: {disk.PowerCycles}");
                    if (disk.TotalReadsGB > 0 || disk.TotalWritesGB > 0)
                        sb.AppendLine($"  R/W Total:{disk.TotalReadsGB} / {disk.TotalWritesGB} GB");
                    if (disk.ReadSpeedMBs > 0 || disk.WriteSpeedMBs > 0)
                        sb.AppendLine($"  Скорость: R {disk.ReadSpeedMBs:F1} MB/s  W {disk.WriteSpeedMBs:F1} MB/s");
                    if (disk.ActiveTimePercent > 0)
                        sb.AppendLine($"  Занятость:{disk.ActiveTimePercent:F1}%");

                    sb.AppendLine(new string('─', 28));
                }
            }
            else
            {
                sb.AppendLine("Диски не обнаружены");
            }
            tbMonitoringHDD.Text = sb.ToString();

            // ── МАТЕРИНСКАЯ ПЛАТА И СЕТЬ ──────────────────────────────────
            sb.Clear();
            if (info.Motherboard != null)
            {
                var mb = info.Motherboard;
                sb.AppendLine($"{mb.Manufacturer} {mb.Product}");
                sb.AppendLine($"BIOS: {mb.BiosVersion}  ({mb.BiosDate})");

                if (mb.Sensors?.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Датчики платы:");
                    foreach (var s in mb.Sensors.OrderBy(x => x.Type).ThenBy(x => x.Name))
                        sb.AppendLine($"  {s.Name,-18} {s.Value,7:F2} {s.Unit}");
                }
            }
            else
            {
                sb.AppendLine("Мат. плата не найдена");
            }

            sb.AppendLine();
            sb.AppendLine("── СЕТЬ ────────────────────");
            try
            {
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                bool hasNet = false;
                foreach (var ni in interfaces)
                {

                    if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                        ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    {
                        hasNet = true;
                        sb.AppendLine($"[{ni.Name}]");

                        var stats = ni.GetIPStatistics();
                        double rxMB = stats.BytesReceived / 1048576.0;
                        double txMB = stats.BytesSent / 1048576.0;

                        sb.AppendLine($"  Принято:    {rxMB:F2} MB");
                        sb.AppendLine($"  Отправлено: {txMB:F2} MB");
                        sb.AppendLine();
                    }
                }

                if (!hasNet)
                    sb.AppendLine("Нет активных подключений");
            }
            catch
            {
                sb.AppendLine("Ошибка чтения сети");
            }

            tbMonitoringMB.Text = sb.ToString();
        }

        private void ColorizeTextBox(TextBox textBox, double percentage)
        {
            if (percentage < 70)
                textBox.ForeColor = Color.DarkGreen;
            else if (percentage < 85)
                textBox.ForeColor = Color.DarkOrange;
            else
                textBox.ForeColor = Color.Crimson;
        }

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

        private void btnStartMonitoring_Click(object sender, EventArgs e) => StartMonitoring();

        private void btnStopMonitoring_Click(object sender, EventArgs e) => StopMonitoring();

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = _logger.SaveToFile();
                _logger.Success($"Лог сохранён: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при сохранении лога: {ex.Message}");
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            _logger.Clear();
            _logger.Info("Логи очищены");
        }

        private void btnCopyLog_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rtbLogger.Text))
            {
                Clipboard.SetText(rtbLogger.Text);
                _logger.Info("Лог скопирован в буфер обмена");
            }
            else
            {
                _logger.Warning("Лог пуст — нечего копировать");
            }
        }

        private void btnAdminRights_Click(object sender, EventArgs e) => RestartAsAdmin();

        private void RestartAsAdmin()
        {
            try
            {
                var exeName = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exeName)) return;

                var startInfo = new ProcessStartInfo(exeName)
                {
                    Verb = "runas",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                Application.Exit();
            }
            catch
            {
                _logger.Warning("Права администратора отклонены пользователем");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckAdminRights();
            RefreshShutdownState(logChange: false);
        }

        // ========== СКРИПТЫ ==========

        private void DataGridViewScripts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dataGridViewScripts.Columns["colScriptControl"].Index)
                return;

            // BugFix: проверяем, что индекс не выходит за пределы списка
            if (e.RowIndex >= _scripts.Count) return;

            var script = _scripts[e.RowIndex];

            if (script.IsRunning)
            {
                // Подтверждение остановки
                var result = MessageBox.Show(
                    $"Остановить скрипт '{script.Name}'?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                script.IsRunning = false;
                script.Status = "Stopped";
                _logger.Info($"Скрипт '{script.Name}' остановлен");
            }
            else
            {
                script.IsRunning = true;
                script.Status = "Running";
                _logger.Success($"Скрипт '{script.Name}' запущен");
                ExecuteScript(script);
            }

            UpdateScriptsGrid();
        }

        private void ExecuteScript(ScriptItem script)
        {
            Task.Run(async () =>
            {
                try
                {
                    switch (script.Name)
                    {
                        case "CleanTemp":
                            await ExecuteCleanTemp(script);
                            break;

                        case "RestartService":
                            await ExecuteRestartService(script);
                            break;

                        case "Backup":
                            // Backup запускается через диалог — не в фоне
                            Invoke(new Action(() =>
                            {
                                script.IsRunning = false;
                                script.Status = "Stopped";
                                UpdateScriptsGrid();
                                ShowBackupDialog();
                            }));
                            return;
                    }

                    Invoke(new Action(() =>
                    {
                        script.IsRunning = false;
                        script.Status = "Completed";
                        UpdateScriptsGrid();
                    }));
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() =>
                    {
                        script.IsRunning = false;
                        script.Status = "Error";
                        _logger.Error($"Ошибка выполнения скрипта '{script.Name}': {ex.Message}");
                        UpdateScriptsGrid();
                    }));
                }
            });
        }

        private async Task ExecuteCleanTemp(ScriptItem script)
        {
            var tempPath = Path.GetTempPath();
            Invoke(new Action(() => _logger.Info($"Очистка папки: {tempPath}")));

            var files = Directory.GetFiles(tempPath, "*.*", SearchOption.TopDirectoryOnly);
            int deleted = 0, failed = 0;

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                    deleted++;
                }
                catch
                {
                    failed++;
                }
            }

            Invoke(new Action(() =>
            {
                _logger.Success($"CleanTemp: удалено {deleted} файлов, пропущено {failed}");
                script.Description = $"Очистка временных файлов (последний раз: {DateTime.Now:HH:mm})";
            }));

            await Task.CompletedTask;
        }

        private async Task ExecuteRestartService(ScriptItem script)
        {
            // BugFix: теперь реально показывает диалог выбора службы
            string serviceName = null;
            Invoke(new Action(() =>
            {
                using var inputForm = new Form
                {
                    Text = "Перезапуск службы",
                    Size = new Size(380, 130),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };
                var label = new Label { Text = "Имя службы:", Location = new Point(10, 15), AutoSize = true };
                var textBox = new TextBox { Location = new Point(10, 35), Width = 340 };
                var btnOk = new Button { Text = "Перезапустить", Location = new Point(10, 65), Width = 120 };
                var btnCancel = new Button { Text = "Отмена", Location = new Point(140, 65), Width = 80 };
                btnOk.Click += (s, e) => { inputForm.DialogResult = DialogResult.OK; };
                btnCancel.Click += (s, e) => { inputForm.DialogResult = DialogResult.Cancel; };
                inputForm.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
                inputForm.AcceptButton = btnOk;
                inputForm.CancelButton = btnCancel;

                if (inputForm.ShowDialog(this) == DialogResult.OK)
                    serviceName = textBox.Text.Trim();
            }));

            if (string.IsNullOrEmpty(serviceName))
            {
                Invoke(new Action(() => _logger.Warning("RestartService: имя службы не указано")));
                return;
            }

            try
            {
                using var svc = new ServiceController(serviceName);
                Invoke(new Action(() => _logger.Info($"Остановка службы '{serviceName}'...")));

                if (svc.Status == ServiceControllerStatus.Running)
                {
                    svc.Stop();
                    svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                }

                Invoke(new Action(() => _logger.Info($"Запуск службы '{serviceName}'...")));
                svc.Start();
                svc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));

                Invoke(new Action(() => _logger.Success($"Служба '{serviceName}' успешно перезапущена")));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => _logger.Error($"Ошибка перезапуска службы: {ex.Message}")));
            }

            await Task.CompletedTask;
        }

        // BugFix: BtnAddScript теперь реально добавляет скрипт через диалог
        private void BtnAddScript_Click(object sender, EventArgs e)
        {
            using var addForm = new Form
            {
                Text = "Добавить скрипт",
                Size = new Size(420, 230),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblName = new Label { Text = "Название:", Location = new Point(10, 15), AutoSize = true };
            var txtName = new TextBox { Location = new Point(10, 35), Width = 380 };

            var lblDesc = new Label { Text = "Описание:", Location = new Point(10, 65), AutoSize = true };
            var txtDesc = new TextBox { Location = new Point(10, 85), Width = 380 };

            var lblType = new Label { Text = "Тип:", Location = new Point(10, 115), AutoSize = true };
            var cmbType = new ComboBox
            {
                Location = new Point(10, 135),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new object[] { "Built-in", "PowerShell", "Batch", "CMD" });
            cmbType.SelectedIndex = 0;

            var btnOk = new Button { Text = "Добавить", Location = new Point(10, 165), Width = 100 };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(120, 165), Width = 80 };

            btnOk.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название скрипта", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                addForm.DialogResult = DialogResult.OK;
            };
            btnCancel.Click += (s, ev) => addForm.DialogResult = DialogResult.Cancel;

            addForm.Controls.AddRange(new Control[]
                { lblName, txtName, lblDesc, txtDesc, lblType, cmbType, btnOk, btnCancel });
            addForm.AcceptButton = btnOk;
            addForm.CancelButton = btnCancel;

            if (addForm.ShowDialog(this) == DialogResult.OK)
            {
                var newScript = new ScriptItem
                {
                    Id = _scripts.Count > 0 ? _scripts.Max(s => s.Id) + 1 : 1,
                    Name = txtName.Text.Trim(),
                    Description = txtDesc.Text.Trim(),
                    Status = "Stopped",
                    Type = cmbType.SelectedItem?.ToString() ?? "Built-in",
                    IsRunning = false
                };

                _scripts.Add(newScript);
                UpdateScriptsGrid();
                _logger.Success($"Скрипт '{newScript.Name}' добавлен");
            }
        }

        private void BtnBackupScript_Click(object sender, EventArgs e) => ShowBackupDialog();

        private void ShowBackupDialog()
        {
            using var sourceBrowser = new FolderBrowserDialog
            {
                Description = "Выберите папку-источник для бекапа"
            };

            if (sourceBrowser.ShowDialog() != DialogResult.OK) return;

            using var destBrowser = new FolderBrowserDialog
            {
                Description = "Выберите папку назначения"
            };

            if (destBrowser.ShowDialog() != DialogResult.OK) return;

            _logger.Info($"Бекап: {sourceBrowser.SelectedPath} → {destBrowser.SelectedPath}");
            PerformBackup(sourceBrowser.SelectedPath, destBrowser.SelectedPath);
        }

        private void PerformBackup(string sourcePath, string destinationPath)
        {
            // Блокируем кнопку на время бекапа
            btnBackupScript.Enabled = false;
            btnBackupScript.Text = "Backup...";

            Task.Run(() =>
            {
                try
                {
                    string backupName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    string backupPath = Path.Combine(destinationPath, backupName);

                    Invoke(new Action(() => _logger.Info("Начало копирования...")));
                    Directory.CreateDirectory(backupPath);

                    var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    int total = files.Length;
                    int current = 0;

                    foreach (var file in files)
                    {
                        string relativePath = file.Substring(sourcePath.Length).TrimStart('\\', '/');
                        string destFile = Path.Combine(backupPath, relativePath);

                        var dirName = Path.GetDirectoryName(destFile);
                        if (!string.IsNullOrEmpty(dirName))
                            Directory.CreateDirectory(dirName);

                        File.Copy(file, destFile, overwrite: true);
                        current++;

                        if (current % 10 == 0 || current == total)
                        {
                            int pct = (int)((double)current / total * 100);
                            Invoke(new Action(() =>
                                _logger.Info($"Прогресс: {current}/{total} ({pct}%)")));
                        }
                    }

                    Invoke(new Action(() =>
                        _logger.Success($"Бекап завершён: {backupPath} ({total} файлов)")));
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() =>
                        _logger.Error($"Ошибка бекапа: {ex.Message}")));
                }
                finally
                {
                    Invoke(new Action(() =>
                    {
                        btnBackupScript.Enabled = true;
                        btnBackupScript.Text = "Backup Script";
                    }));
                }
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopMonitoring();
            _shutdownCheckTimer?.Stop();
            _shutdownCheckTimer?.Dispose();
            _shutdownCountdownTimer?.Stop();
            _shutdownCountdownTimer?.Dispose();
            _monitoringService?.Dispose();
            base.OnFormClosing(e);
        }

        private void tbMonitorCPU_TextChanged(object sender, EventArgs e)
        {

        }
    }
}