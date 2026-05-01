using AdminHelper.Models;
using AdminHelper.Utils;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminHelper.Logger
{
    public class LoggerService : ILogger
    {
        private readonly RichTextBox _richTextBox;
        private readonly List<LogEntry> _logEntries;
        private readonly object _lockObject = new object();

        public LoggerService(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
            _logEntries = new List<LogEntry>();
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            var entry = new LogEntry(message, level);

            lock (_lockObject)
            {
                _logEntries.Add(entry);
            }

            if (_richTextBox.InvokeRequired)
            {
                _richTextBox.Invoke(new Action(() => AppendToUI(entry)));
            }
            else
            {
                AppendToUI(entry);
            }
        }

        private void AppendToUI(LogEntry entry)
        {
            string text = entry.ToString() + Environment.NewLine;

            int start = _richTextBox.TextLength;
            _richTextBox.AppendText(text);

            _richTextBox.Select(start, text.Length);
            _richTextBox.SelectionColor = GetColorByLevel(entry.Level);
            _richTextBox.SelectionLength = 0;

            _richTextBox.ScrollToCaret();
        }

        private Color GetColorByLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Error => Color.Red,
                LogLevel.Warning => Color.Orange,
                LogLevel.Success => Color.DarkGreen,
                LogLevel.Debug => Color.Gray,
                _ => Color.White
            };
        }

        public void Info(string message) => Log(message, LogLevel.Info);
        public void Warning(string message) => Log(message, LogLevel.Warning);
        public void Error(string message) => Log(message, LogLevel.Error);
        public void Success(string message) => Log(message, LogLevel.Success);
        public void Debug(string message) => Log(message, LogLevel.Debug);

        public void Clear()
        {
            if (_richTextBox.InvokeRequired)
            {
                _richTextBox.Invoke(new Action(_richTextBox.Clear));
            }
            else
            {
                _richTextBox.Clear();
            }

            lock (_lockObject)
            {
                _logEntries.Clear();
            }
        }

        public string GetAllLogs()
        {
            lock (_lockObject)
            {
                return string.Join(Environment.NewLine, _logEntries);
            }
        }

        public string SaveToFile(string fileName = null)
        {
            string logsContent = GetAllLogs();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = FileUtils.GenerateLogFileName("admin_helper_log");
            }

            string logsFolder = FileUtils.GetLogsFolderPath();
            string fullPath = FileUtils.SaveTextToFile(logsContent, logsFolder, fileName);

            return fullPath;
        }
    }
}
