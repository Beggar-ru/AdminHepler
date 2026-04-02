using AdminHepler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminHepler.Services
{
    public interface IMonitoringService : IDisposable
    {
        event EventHandler<SystemInfo> DataUpdated;
        void StartMonitoring();
        void StopMonitoring();
        bool IsMonitoring { get; }
    }
}
