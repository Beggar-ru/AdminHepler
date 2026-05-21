using AdminHelper.Models;
using System;

namespace AdminHelper.Services
{
    public interface IMonitoringService : IDisposable
    {
        // Добавляем асинхронный метод инициализации в интерфейс
        Task InitializeAsync();

        event EventHandler<SystemInfo> DataUpdated;
        void StartMonitoring();
        void StopMonitoring();
        bool IsMonitoring { get; }
        int PollingIntervalMs { get; set; }
    }
}