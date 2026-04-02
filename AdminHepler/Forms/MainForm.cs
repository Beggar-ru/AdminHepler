using AdminHepler.Logger;
using AdminHepler.Utils;
using AdminHepler.Models;
using System.Windows.Forms;
using AdminHepler.Forms;
using AdminHepler.Scripts;
using AdminHepler.Services;

namespace AdminHepler
{
    public partial class MainForm : Form
    {
        private readonly ILogger _logger;
        private IMonitoringService _monitoringService;

        public MainForm()
        {
            InitializeComponent();

            _logger = new LoggerService(rtbLogger);
            _monitoringService = new MonitoringService(_logger);
            _monitoringService.DataUpdated += OnMonitoringDataUpdated;
            SetupMonitoringTextBoxes();
            btnStopMonitoring.Enabled = false;


            _logger.Info("Приложение запущено");
            _logger.Info($"Папка для логов: {FileUtils.GetLogsFolderPath()}");

            
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
            tbMonitorCPU.Text = $"{info.CpuUsage:F1}%";
            tbMonitoringGPU.Text = $"{info.GpuUsage:F1}%";
            tbMonitoringRAM.Text = $"{info.RamUsage:F1} / {info.RamTotal:F1} GB";
            tbMonitoringHDD.Text = $"{info.DiskUsage:F1}% (свободно: {info.DiskFree:F0} GB)";

            // Цветовая индикация нагрузки
            ColorizeTextBox(tbMonitorCPU, info.CpuUsage);
            ColorizeTextBox(tbMonitoringGPU, info.GpuUsage);

            double ramPercent = (info.RamUsage / info.RamTotal) * 100;
            ColorizeTextBox(tbMonitoringRAM, ramPercent);

            ColorizeTextBox(tbMonitoringHDD, info.DiskUsage);
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
                /*
                lblMonitoringStatus.Text = "Мониторинг остановлен";
                lblMonitoringStatus.ForeColor = Color.Gray;
                */
                // Очистка полей
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
                /*
                // Визуальный индикатор работы
                lblMonitoringStatus.Text = "Мониторинг активен";
                lblMonitoringStatus.ForeColor = Color.Green;
                */
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка запуска мониторинга: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _monitoringService?.StopMonitoring();
            _monitoringService?.Dispose();

            base.OnFormClosing(e);
        }
    }
}
