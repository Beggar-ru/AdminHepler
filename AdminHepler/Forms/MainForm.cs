using AdminHepler.Forms;
using AdminHepler.Logger;
using AdminHepler.Models;
using AdminHepler.Scripts;
using AdminHepler.Services;
using AdminHepler.Utils;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.ServiceProcess;
using System.Management;

namespace AdminHepler
{
    public partial class MainForm : Form
    {
        private readonly ILogger _logger;
        private IMonitoringService _monitoringService;
        private System.Windows.Forms.Timer _updateTimer;

        public MainForm()
        {
            InitializeComponent();
            StartMonitoring();
            _logger = new LoggerService(rtbLogger);
            _monitoringService = new MonitoringService(_logger);
            _monitoringService.DataUpdated += OnMonitoringDataUpdated;
            SetupMonitoringTextBoxes();
            btnStopMonitoring.Enabled = false;

            _logger.Info("Приложение запущено");
            _logger.Info($"Папка для логов: {FileUtils.GetLogsFolderPath()}");
        }
        
        private void StartMonitoring()
        {
            /*
            _updateTimer = new System.Windows.Forms.Timer();
            _updateTimer.Interval = 2000; // 2 секунды
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
            */

            // Первоначальная загрузка
            UpdateProcesses();
            UpdateServices();
            LoadScripts();
        }
        
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateProcesses();
            UpdateServices();
        }

        private void UpdateProcesses()
        {
            dataGridViewProcesses.Rows.Clear();

            try
            {
                var processes = Process.GetProcesses();
                foreach (var proc in processes)
                {
                    try
                    {
                        string description = "";
                        try
                        {
                            description = proc.MainModule?.FileVersionInfo?.FileDescription ?? "";
                        }
                        catch { }

                        dataGridViewProcesses.Rows.Add(
                            proc.ProcessName,
                            (proc.WorkingSet64 / 1024 / 1024).ToString(), // MB
                            description,
                            proc.Responding ? "Работает" : "Нет ответа",
                            "Process"
                        );
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка обновления процессов: {ex.Message}");
            }
        }

        private void UpdateServices()
        {
            dataGridViewServices.Rows.Clear();

            try
            {
                var services = ServiceController.GetServices();
                foreach (var service in services)
                {
                    string startType = GetServiceStartType(service.ServiceName);

                    dataGridViewServices.Rows.Add(
                        service.ServiceName,
                        "N/A", // Память для служб сложнее получить
                        service.DisplayName,
                        service.Status.ToString(),
                        startType
                    );
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка обновления служб: {ex.Message}");
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

        private void LoadScripts()
        {
            dataGridViewScripts.Rows.Clear();

            // Пример встроенных скриптов
            dataGridViewScripts.Rows.Add("CleanTemp", "0", "Очистка временных файлов", "Включен", "PowerShell");
            dataGridViewScripts.Rows.Add("RestartService", "0", "Перезапуск службы", "Включен", "Встроенный");
            dataGridViewScripts.Rows.Add("Backup", "0", "Резервное копирование", "Выключен", "bat");
        }

        private void LogMessage(string message)
        {
            if (rtbLogger.InvokeRequired)
            {
                rtbLogger.Invoke(new Action(() => LogMessage(message)));
                return;
            }

            rtbLogger.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }


        private void SetupMonitoringTextBoxes()
        {
            tbMonitorCPU.ReadOnly = true;
            tbMonitoringRAM.ReadOnly = true;
            tbMonitoringGPU.ReadOnly = true;
            tbMonitoringHDD.ReadOnly = true;
        }

        private void OnMonitoringDataUpdated(object sender, SystemInfo info)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateMonitoringDisplay(info)));
            }
            else
            {
                UpdateMonitoringDisplay(info);
            }
        }

        private void UpdateMonitoringDisplay(SystemInfo info)
        {
            // === CPU ===
            var cpuText = new StringBuilder();
            cpuText.AppendLine($"Загрузка: {info.CpuLoad:F1}%");
            cpuText.AppendLine($"Температура: {info.CpuTemperature:F1}°C");
            cpuText.AppendLine($"Частота: {info.CpuFrequency:F2} GHz");
            if (info.CpuPower > 0)
                cpuText.AppendLine($"Потребление: {info.CpuPower:F1}W");
            tbMonitorCPU.Text = cpuText.ToString();
            ColorizeTextBox(tbMonitorCPU, info.CpuLoad);

            // === GPU ===
            var gpuText = new StringBuilder();
            gpuText.AppendLine($"Загрузка: {info.GpuLoad:F1}%");
            gpuText.AppendLine($"Температура: {info.GpuTemperature:F1}°C");
            if (info.GpuMemoryTotal > 0)
                gpuText.AppendLine($"Память: {info.GpuMemoryUsed:F0} / {info.GpuMemoryTotal:F0} MB");
            if (info.GpuFrequency > 0)
                gpuText.AppendLine($"Частота: {info.GpuFrequency:F0} MHz");
            if (info.GpuFanSpeed > 0)
                gpuText.AppendLine($"Вентилятор: {info.GpuFanSpeed:F0} RPM");
            tbMonitoringGPU.Text = gpuText.ToString();
            ColorizeTextBox(tbMonitoringGPU, info.GpuLoad);

            // === RAM ===
            var ramText = new StringBuilder();
            ramText.AppendLine($"Использовано: {info.RamUsed:F1} / {info.RamTotal:F1} GB");
            ramText.AppendLine($"Загрузка: {info.RamLoad:F1}%");
            tbMonitoringRAM.Text = ramText.ToString();
            ColorizeTextBox(tbMonitoringRAM, info.RamLoad);

            // === DISKS ===
            var diskText = new StringBuilder();
            if (info.Disks.Count > 0)
            {
                foreach (var disk in info.Disks)
                {
                    diskText.AppendLine($"{disk.Name} ({disk.Model})");
                    diskText.AppendLine($"Тип: {disk.Type}");
                    diskText.AppendLine($"Всего: {disk.TotalSize:F0} GB");
                    diskText.AppendLine($"Свободно: {disk.FreeSpace:F0} GB");
                    diskText.AppendLine($"Загрузка: {disk.UsagePercent:F1}%");
                    if (disk.Temperature > 0)
                        diskText.AppendLine($"Температура: {disk.Temperature:F1}°C");
                }
            }
            else
            {
                diskText.AppendLine("Диски не обнаружены");
            }
            tbMonitoringHDD.Text = diskText.ToString();

            if (info.Disks.Count > 0)
            {
                var maxDiskUsage = info.Disks.Max(d => d.UsagePercent);
                ColorizeTextBox(tbMonitoringHDD, maxDiskUsage);
            }
        }

        private void ColorizeTextBox(TextBox textBox, double percentage)
        {
            if (percentage < 70)
            {
                textBox.ForeColor = Color.Green;
            }
            else if (percentage < 85)
            {
                textBox.ForeColor = Color.Orange;
            }
            else
            {
                textBox.ForeColor = Color.Red;
            }
        }

        private void rtbLogger_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnTestLogger_Click(object sender, EventArgs e)
        {
            _logger.Info("Это информационное сообщение");
            _logger.Warning("Это предупреждение");
            _logger.Error("Это ошибка");
            _logger.Success("Это успешное действие");
        }


        private void btnCopyLog_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rtbLogger.Text))
            {
                Clipboard.SetText(rtbLogger.Text);
                _logger.Info("Лог скопирован в буфер обмена");
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            _logger.Clear();
            _logger.Info("Логи очищены");
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = _logger.SaveToFile();
                _logger.Success($"Лог сохранен: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при сохранении лога: {ex.Message}");
            }
        }

        private void btnOffPcTimer_Click(object sender, EventArgs e)
        {
            using (var frm = new frmShutdownTimer(_logger))
            {
                frm.ShowDialog();
            }
        }

        private void btnCancelOffpc_Click(object sender, EventArgs e)
        {
            var tools = new Scripts.SystemTools(_logger);
            tools.AbortShutdown();
        }

        private void btnStopMonitoring_Click(object sender, EventArgs e)
        {
            try
            {
                _monitoringService.StopMonitoring();
                _logger.Info("Мониторинг ресурсов остановлен");

                btnStartMonitoring.Enabled = true;
                btnStopMonitoring.Enabled = false;

                tbMonitorCPU.Text = "0.0%";
                tbMonitoringGPU.Text = "0.0%";
                tbMonitoringRAM.Text = "0.0 / 0.0 GB";
                tbMonitoringHDD.Text = "0.0%";

                tbMonitorCPU.ForeColor = Color.Black;
                tbMonitoringGPU.ForeColor = Color.Black;
                tbMonitoringRAM.ForeColor = Color.Black;
                tbMonitoringHDD.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка остановки мониторинга: {ex.Message}");
            }
        }

        private void btnStartMonitoring_Click(object sender, EventArgs e)
        {
            try
            {
                _monitoringService.StartMonitoring();
                _logger.Success("Мониторинг ресурсов запущен");

                btnStartMonitoring.Enabled = false;
                btnStopMonitoring.Enabled = true;

            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка запуска мониторинга: {ex.Message}");
            }
        }

        private void RestartAsAdmin()
        {
            try
            {
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                var startInfo = new ProcessStartInfo(exeName)
                {
                    Verb = "runas",
                    UseShellExecute = true
                };
                _logger.Info("Request admin rights...");
                Process.Start(startInfo);
                _logger.Success("Admin rights granted, restarting...");
                Application.Exit();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.Warning("Admin rights denied by user.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to restart as admin: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _monitoringService?.StopMonitoring();
            _monitoringService?.Dispose();

            base.OnFormClosing(e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Utils.IsAdminUtils.IsAdmin())
            {
                btnAdminRights.Enabled = false;
                _logger.Info("Запущено с правами администратора");
                lblRights.Text = "Admin";
                lblRights.ForeColor = Color.Red;
            }
            else
            {
                btnAdminRights.Enabled = true;
                _logger.Warning("Запущено без прав администратора. Некоторые функции могут быть недоступны.");
                lblRights.Text = "User";
                lblRights.ForeColor = Color.Blue;
            }
            _monitoringService = new MonitoringService(_logger);
            _monitoringService.DataUpdated += OnMonitoringDataUpdated;

            bool isAdmin = new System.Security.Principal.WindowsPrincipal(
                System.Security.Principal.WindowsIdentity.GetCurrent())
                .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

            lblRights.Text = isAdmin ? "Administrator" : "User";
            lblRights.ForeColor = isAdmin ? Color.Green : Color.Red;
        }

        private void tbMonitoringRAM_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnAdminRights_Click(object sender, EventArgs e)
        {
            RestartAsAdmin();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
    }
}
