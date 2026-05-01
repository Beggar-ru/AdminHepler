using AdminHelper.Models;
using System;

namespace AdminHelper.Services
{
    public interface IMonitoringService : IDisposable
    {
        /// <summary>Событие — новый снимок данных готов</summary>
        event EventHandler<SystemInfo> DataUpdated;

        void StartMonitoring();
        void StopMonitoring();

        bool IsMonitoring { get; }

        /// <summary>Интервал опроса в миллисекундах (по умолчанию 1000)</summary>
        int PollingIntervalMs { get; set; }
    }
}