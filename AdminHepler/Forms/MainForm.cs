using AdminHepler.Logger;
using AdminHepler.Utils;
using AdminHepler.Models;
using System.Windows.Forms;
using AdminHepler.Forms;
using AdminHepler.Scripts;

namespace AdminHepler
{
    public partial class MainForm : Form
    {
        private readonly ILogger _logger;

        public MainForm()
        {
            InitializeComponent();

            _logger = new LoggerService(rtbLogger);

            _logger.Info("Приложение запущено");
            _logger.Info($"Папка для логов: {FileUtils.GetLogsFolderPath()}");
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
    }
}
