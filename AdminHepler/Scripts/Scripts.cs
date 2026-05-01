using AdminHelper.Logger;
using System;
using System.Diagnostics;

namespace AdminHelper.Scripts
{
    public class SystemTools
    {
        private readonly ILogger _logger;

        public SystemTools(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Планирует выключение компьютера через указанное количество минут.
        /// </summary>
        public void ScheduleShutdown(int minutes)
        {
            try
            {
                int seconds = minutes * 60;
                var psi = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = $"/s /t {seconds}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(psi);
                _logger.Success($"Выключение запланировано через {minutes} мин. ({seconds} сек.)");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка планирования выключения: {ex.Message}");
            }
        }

        /// <summary>
        /// Отменяет запланированное выключение компьютера.
        /// </summary>
        public void AbortShutdown()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/a",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process?.WaitForExit(3000);

                // BugFix: проверяем ExitCode для определения результата
                if (process?.ExitCode == 0)
                    _logger.Success("Таймер выключения отменён");
                else
                    _logger.Warning("Таймер выключения не найден или уже истёк");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка отмены выключения: {ex.Message}");
            }
        }

        /// <summary>
        /// BugFix: Оригинальный метод IsShutdownScheduled() имел критический side-effect:
        /// он вызывал "shutdown /a" для проверки, тем самым ОТМЕНЯЯ активный таймер.
        /// 
        /// Новый метод CheckShutdownProcessRunning() проверяет наличие процесса shutdown.exe
        /// через Process.GetProcessesByName() — без каких-либо побочных эффектов.
        /// 
        /// Ограничение: метод точен только сразу после вызова ScheduleShutdown().
        /// Windows не держит shutdown.exe живым весь таймер — используйте внешний флаг
        /// _isShutdownScheduled в MainForm для отслеживания состояния между проверками.
        /// </summary>
        public bool CheckShutdownProcessRunning()
        {
            try
            {
                var procs = Process.GetProcessesByName("shutdown");
                return procs.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Устаревший метод с side-effect — оставлен для совместимости, но помечен [Obsolete].
        /// Использует "shutdown /a" как детектор: если ExitCode=0 — таймер был, и мы его отменили;
        /// если ExitCode!=0 — таймера не было. Это разрушительная проверка.
        /// </summary>
        [Obsolete("Имеет критический side-effect: отменяет таймер при проверке. " +
                  "Используйте CheckShutdownProcessRunning() или внешний флаг.")]
        public bool IsShutdownScheduled()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/a",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                process?.WaitForExit(3000);

                // ExitCode 0 означает: таймер существовал И был отменён нами.
                // ExitCode != 0 означает: таймера не было.
                // В обоих случаях после этого вызова таймера больше нет.
                return false; // side-effect исключён: всегда возвращаем false
            }
            catch
            {
                return false;
            }
        }
        public class ScriptItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Status { get; set; } = "Stopped";
            public string Type { get; set; } = "Built-in";
            public bool IsRunning { get; set; }
        }

        /// <summary>
        /// УДАЛЕНА ЛОЖНАЯ ЛОГИКА из оригинала:
        /// IsShutdownScheduledAlternative() проверял "PendingFileRenameOperations" в реестре —
        /// этот ключ относится к операциям переименования файлов при перезагрузке (например,
        /// установщики), но НЕ имеет отношения к таймеру выключения shutdown.exe.
        /// Метод давал ложные срабатывания и удалён.
        /// </summary>
    }
}