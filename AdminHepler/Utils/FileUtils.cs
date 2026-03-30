using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminHepler.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Создает папку, если она не существует
        /// </summary>
        public static string EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// Получает путь к папке Logs (рядом с exe)
        /// </summary>
        public static string GetLogsFolderPath()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string logsPath = Path.Combine(exePath, "Logs");
            return EnsureDirectoryExists(logsPath);
        }

        /// <summary>
        /// Генерирует уникальное имя файла с датой и временем
        /// </summary>
        public static string GenerateLogFileName(string prefix = "log")
        {
            return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        }

        /// <summary>
        /// Сохраняет текст в файл
        /// </summary>
        public static string SaveTextToFile(string content, string folderPath, string fileName)
        {
            string fullPath = Path.Combine(folderPath, fileName);
            File.WriteAllText(fullPath, content, System.Text.Encoding.UTF8);
            return fullPath;
        }
    }
}
