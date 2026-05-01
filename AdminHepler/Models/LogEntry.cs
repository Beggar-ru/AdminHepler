using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminHelper.Models
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success,
        Debug
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogLevel Level { get; set; }

        public LogEntry(string message, LogLevel level)
        {
            Timestamp = DateTime.Now;
            Message = message;
            Level = level;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] [{Level}] {Message}";
        }
    }
}
