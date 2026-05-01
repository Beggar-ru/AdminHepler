using AdminHepler.Forms;
using AdminHepler.Logger;
using AdminHepler.Models;
using AdminHepler.Scripts;
using AdminHepler.Services;
using AdminHepler.Utils;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace AdminHelper
{
    public partial class MainForm : Form
    {
        private readonly ILogger _logger;
        private IMonitoringService _monitoringService;
        private System.Windows.Forms.Timer _updateTimer;
        private bool _isMonitoring = false;
        private List<ScriptItem> _scripts;

        public MainForm()
        {
            InitializeComponent();

            _logger = new LoggerService(rtbLogger);
            _monitoringService = new MonitoringService(_logger);
            _monitoringService.DataUpdated += OnMonitoringDataUpdated;

            _scripts = new List<ScriptItem>();
            InitializeScripts();
            CheckAdminRights();

            _logger.Info("Приложение запущено");
            _logger.Info($"Папка для логов: {FileUtils.GetLogsFolderPath()}");
        }

        private void InitializeScripts()
        {
            _scripts.Add(new ScriptItem
            {
                Id = 1,
                Name = "CleanTemp",
                Description = "Очистка временных файлов",
                Status = "Stopped",
                Type = "PowerShell",
                IsRunning = false
            });
            _scripts.Add(new ScriptItem
            {
                Id = 2,
                Name = "RestartService",
                Description = "Перезапуск службы",
                Status = "Stopped",
                Type = "Built-in",
                IsRunning = false
            });
            _scripts.Add(new ScriptItem
            {
                Id = 3,
                Name = "Backup",
                Description = "Резервное копирование",
                Status = "Stopped",
                Type = "Batch",
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
                    script.IsRunning ? "⏹ Stop" : "▶ Start"
                );

                // Цвет кнопки в зависимости от статуса
                var buttonCell = (DataGridViewButtonCell)dataGridViewScripts.Rows[rowIndex].Cells["colScriptControl"];
                buttonCell.Style.BackColor = script.IsRunning ? Color.Red : Color.Green;
                buttonCell.Style.ForeColor = Color.White;
            }
        }

        private void CheckAdminRights()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            lblRights.Text = isAdmin ? "Administrator" : "User";
            lblRights.ForeColor = isAdmin ? Color.Green : Color.Red;
            btnAdminRights.Enabled = !isAdmin;
        }

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
            btnStopMonitoring.Enabled = true;

            _logger.Success("Мониторинг ресурсов запущен");
        }

        private void StopMonitoring()
        {
            if (!_isMonitoring) return;

            _monitoringService.StopMonitoring();
            _updateTimer?.Stop();
            _updateTimer?.Dispose();

            _isMonitoring = false;
            btnStartMonitoring.Enabled = true;
            btnStopMonitoring.Enabled = false;

            _logger.Info("Мониторинг ресурсов остановлен");
        }

        private async void UpdateTimer_Tick(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                UpdateProcesses();
                UpdateServices();
            });
        }

        private void UpdateProcesses()
        {
            try
            {
                var processes = Process.GetProcesses()
                    .Take(100) // Ограничим для производительности
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
                        dataGridViewProcesses.Rows.Add(
                            proc.ProcessName,
                            proc.MemoryMB,
                            proc.Description,
                            proc.Status,
                            proc.Type
                        );
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка обновления процессов: {ex.Message}");
            }
        }

        private void UpdateServices()
        {
            try
            {
                var services = ServiceController.GetServices()
                    .Take(100)
                    .Select(s => new
                    {
                        s.ServiceName,
                        MemoryMB = 0,
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
                        dataGridViewServices.Rows.Add(
                            svc.ServiceName,
                            svc.MemoryMB,
                            svc.DisplayName,
                            svc.Status,
                            svc.StartType
                        );
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка обновления служб: {ex.Message}");
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
            var cpuText = new StringBuilder();
            cpuText.AppendLine($"Load: {info.CpuLoad:F1}%");
            cpuText.AppendLine($"Temp: {info.CpuTemperature:F1}°C");
            cpuText.AppendLine($"Freq: {info.CpuFrequency:F2} GHz");
            if (info.CpuPower > 0)
                cpuText.AppendLine($"Power: {info.CpuPower:F1}W");
            tbMonitorCPU.Text = cpuText.ToString();
            ColorizeTextBox(tbMonitorCPU, info.CpuLoad);

            var gpuText = new StringBuilder();
            gpuText.AppendLine($"Load: {info.GpuLoad:F1}%");
            gpuText.AppendLine($"Temp: {info.GpuTemperature:F1}°C");
            if (info.GpuMemoryTotal > 0)
                gpuText.AppendLine($"VRAM: {info.GpuMemoryUsed:F0} / {info.GpuMemoryTotal:F0} MB");
            if (info.GpuFrequency > 0)
                gpuText.AppendLine($"Clock: {info.GpuFrequency:F0} MHz");
            if (info.GpuFanSpeed > 0)
                gpuText.AppendLine($"Fan: {info.GpuFanSpeed:F0} RPM");
            tbMonitoringGPU.Text = gpuText.ToString();
            ColorizeTextBox(tbMonitoringGPU, info.GpuLoad);

            var ramText = new StringBuilder();
            ramText.AppendLine($"Used: {info.RamUsed:F1} / {info.RamTotal:F1} GB");
            ramText.AppendLine($"Load: {info.RamLoad:F1}%");
            tbMonitoringRAM.Text = ramText.ToString();
            ColorizeTextBox(tbMonitoringRAM, info.RamLoad);

            var diskText = new StringBuilder();
            if (info.Disks.Count > 0)
            {
                foreach (var disk in info.Disks)
                {
                    diskText.AppendLine($"{disk.Name} ({disk.Model})");
                    diskText.AppendLine($"Type: {disk.Type}");
                    diskText.AppendLine($"Total: {disk.TotalSize:F0} GB");
                    diskText.AppendLine($"Free: {disk.FreeSpace:F0} GB");
                    diskText.AppendLine($"Load: {disk.UsagePercent:F1}%");
                    if (disk.Temperature > 0)
                        diskText.AppendLine($"Temp: {disk.Temperature:F1}°C");
                    diskText.AppendLine("-------------------");
                }
            }
            else
            {
                diskText.AppendLine("No disks detected");
            }
            tbMonitoringHDD.Text = diskText.ToString();
        }

        private void ColorizeTextBox(TextBox textBox, double percentage)
        {
            if (percentage < 70)
                textBox.ForeColor = Color.Green;
            else if (percentage < 85)
                textBox.ForeColor = Color.Orange;
            else
                textBox.ForeColor = Color.Red;
        }

        // === ОБРАБОТЧИКИ СОБЫТИЙ ===

        private void btnStartMonitoring_Click(object sender, EventArgs e)
        {
            StartMonitoring();
        }

        private void btnStopMonitoring_Click(object sender, EventArgs e)
        {
            StopMonitoring();
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
            var tools = new SystemTools(_logger);
            tools.AbortShutdown();
        }

        private void btnAdminRights_Click(object sender, EventArgs e)
        {
            RestartAsAdmin();
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
                Process.Start(startInfo);
                Application.Exit();
            }
            catch
            {
                _logger.Warning("Права администратора отклонены");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckAdminRights();
        }

        // === НОВЫЕ МЕТОДЫ ДЛЯ СКРИПТОВ ===

        private void DataGridViewScripts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dataGridViewScripts.Columns["colScriptControl"].Index)
                return;

            var script = _scripts[e.RowIndex];

            if (script.IsRunning)
            {
                // Остановить скрипт
                script.IsRunning = false;
                script.Status = "Stopped";
                _logger.Info($"Скрипт '{script.Name}' остановлен");
            }
            else
            {
                // Запустить скрипт
                script.IsRunning = true;
                script.Status = "Running";
                _logger.Success($"Скрипт '{script.Name}' запущен");

                // Здесь можно добавить реальное выполнение скрипта
                ExecuteScript(script);
            }

            UpdateScriptsGrid();
        }

        private void ExecuteScript(ScriptItem script)
        {
            Task.Run(() =>
            {
                try
                {
                    switch (script.Name)
                    {
                        case "CleanTemp":
                            var tempPath = Path.GetTempPath();
                            Invoke(new Action(() =>
                                _logger.Info($"Очистка {tempPath}...")));

                            var files = Directory.GetFiles(tempPath, "*.*");
                            int deleted = 0;
                            foreach (var file in files)
                            {
                                try
                                {
                                    File.Delete(file);
                                    deleted++;
                                }
                                catch { }
                            }

                            Invoke(new Action(() =>
                                _logger.Success($"Удалено файлов: {deleted}")));
                            break;

                        case "RestartService":
                            Invoke(new Action(() =>
                                _logger.Info("Перезапуск службы...")));
                            // TODO: Открыть диалог выбора службы
                            break;

                        case "Backup":
                            Invoke(new Action(() =>
                                _logger.Info("Запуск резервного копирования...")));
                            // TODO: Вызвать ShowBackupDialog()
                            break;
                    }

                    Invoke(new Action(() =>
                    {
                        script.IsRunning = false;
                        script.Status = "Stopped";
                        UpdateScriptsGrid();
                    }));
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() =>
                        _logger.Error($"Ошибка выполнения скрипта: {ex.Message}")));
                }
            });
        }

        private void BtnAddScript_Click(object sender, EventArgs e)
        {
            // TODO: Открыть диалог добавления нового скрипта
            _logger.Info("Открытие диалога добавления скрипта...");
        }

        private void BtnBackupScript_Click(object sender, EventArgs e)
        {
            ShowBackupDialog();
        }

        private void ShowBackupDialog()
        {
            using (var folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.Description = "Выберите папку для резервного копирования";
                folderBrowser.ShowNewFolderButton = true;

                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    using (var sourceBrowser = new FolderBrowserDialog())
                    {
                        sourceBrowser.Description = "Выберите папку для бекапа";

                        if (sourceBrowser.ShowDialog() == DialogResult.OK)
                        {
                            _logger.Success($"Бекап: {sourceBrowser.SelectedPath} → {folderBrowser.SelectedPath}");
                            // TODO: Реализовать логику бекапа
                            PerformBackup(sourceBrowser.SelectedPath, folderBrowser.SelectedPath);
                        }
                    }
                }
            }
        }

        private void PerformBackup(string sourcePath, string destinationPath)
        {
            Task.Run(() =>
            {
                try
                {
                    string backupName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    string backupPath = Path.Combine(destinationPath, backupName);

                    _logger.Info($"Начало копирования...");
                    Directory.CreateDirectory(backupPath);

                    // Копирование файлов (упрощенно)
                    var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    int total = files.Length;
                    int current = 0;

                    foreach (var file in files)
                    {
                        string relativePath = file.Substring(sourcePath.Length).TrimStart('\\');
                        string destFile = Path.Combine(backupPath, relativePath);

                        Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                        File.Copy(file, destFile, true);

                        current++;
                        if (current % 10 == 0)
                        {
                            Invoke(new Action(() =>
                                _logger.Info($"Прогресс: {current}/{total} файлов")));
                        }
                    }

                    Invoke(new Action(() =>
                        _logger.Success($"Бекап завершен: {backupPath}")));
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() =>
                        _logger.Error($"Ошибка бекапа: {ex.Message}")));
                }
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopMonitoring();
            _monitoringService?.Dispose();
            base.OnFormClosing(e);
        }
    }

    // Модель скрипта
    public class ScriptItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public bool IsRunning { get; set; }
    }
}