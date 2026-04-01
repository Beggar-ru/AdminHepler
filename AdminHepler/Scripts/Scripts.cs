using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminHepler.Logger;
using AdminHepler.Utils;

namespace AdminHepler.Scripts
{
    public class SystemTools
    {
        private readonly ILogger _logger;

        public SystemTools(ILogger logger)
        {
            _logger = logger;
        }

        public void ScheduleShutdown(int minutes)
        {
            try
            {
                int seconds = minutes * 60;
                Process.Start("shutdown", $"/s /t {seconds}");
                _logger.Success($"Планирование выключения через {minutes} минут");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при планировании выключения: {ex.Message}");
            }
        }

        public void AbortShutdown()
        {
            try
            {
                Process.Start("shutdown", "/a");
                _logger.Success("Планирование выключения отменено");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при отмене планирования выключения: {ex.Message}");
            }
        }
    }
}
