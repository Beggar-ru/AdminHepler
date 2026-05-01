using AdminHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminHelper.Logger
{
    public interface ILogger 
    {
        void Log(string message, LogLevel level = LogLevel.Info);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Success(string message);
        void Debug(string message);
        void Clear();
        string GetAllLogs();
        string SaveToFile(string fileName = null);
    }
}
