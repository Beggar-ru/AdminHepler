using AdminHepler.Logger;
using AdminHepler.Models;
using AdminHepler.Utils;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;

namespace AdminHepler.Services
{
    public class MonitoringService : IMonitoringService
    {
        public event EventHandler<SystemInfo> DataUpdated;

        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;

        private Computer _computer;
        private ISensor _gpuSensor;

        private readonly ILogger _logger;

        public bool IsMonitoring => _isMonitoring;

        public MonitoringService(ILogger logger)
        {
            InitializeCounters();
            InitializeGpuMonitoring();
            _logger = logger;
            
        }

        private void InitializeCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error initializing counters: {ex.Message}");
            }
        }

        public void StartMonitoring()
        {
            if (_isMonitoring) return;

            _cts = new CancellationTokenSource();
            _isMonitoring = true;

            Task.Run(() => MonitorLoop(_cts.Token));
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring) return;

            _cts?.Cancel();
            _isMonitoring = false;
            _logger.Info("Monitoring stopped.");
        }

        private async Task MonitorLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var info = GetSystemInfo();
                    DataUpdated?.Invoke(this, info);

                    await Task.Delay(1000, token);
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("Monitoring stopped.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Monitoring error: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        }

        private (double used, double total) GetRamInfo()
        {
            var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
            var total = (double)info.TotalPhysicalMemory / 1024 / 1024 / 1024;
            var available = (double)info.AvailablePhysicalMemory / 1024 / 1024 / 1024;
            var used = total - available;

            return (used, total);
        }
        private void InitializeGpuMonitoring()
        {
            _computer = new Computer
            {
                IsGpuEnabled = true,
                IsCpuEnabled = false,
                IsMemoryEnabled = false,
                IsMotherboardEnabled = false,
                IsControllerEnabled = false,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };

            bool hasAdminRights = Utils.IsAdminUtils.IsAdmin();
            _computer.Open(hasAdminRights ? false : true);

            //_computer.Open(false);

            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia ||
                    hardware.HardwareType == HardwareType.GpuAmd ||
                    hardware.HardwareType == HardwareType.GpuIntel)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load &&
                            sensor.Name.Contains("GPU Core"))
                        {
                            _gpuSensor = sensor;
                            break;
                        }
                    }
                }
            }
        }

        private double GetGpuUsage()
        {
            if (_gpuSensor != null)
            {
                _gpuSensor.Hardware.Update();
                return _gpuSensor.Value ?? 0;
            }
            return 0;
        }

        private (double usage, double free) GetDiskInfo()
        {
            try
            {
                var drive = new System.IO.DriveInfo("C");
                var total = drive.TotalSize;
                var free = drive.TotalFreeSpace;
                var used = total - free;
                var usage = (used * 100.0) / total;
                var freeGb = free / 1024.0 / 1024.0 / 1024.0;

                return (usage, freeGb);
            }
            catch
            {
                return (0, 0);
            }
        }
        private SystemInfo GetSystemInfo()
        {
            var info = new SystemInfo();

            try
            {
                // CPU
                if (_cpuCounter != null)
                    info.CpuUsage = _cpuCounter.NextValue();

                // RAM
                var ramInfo = GetRamInfo();
                info.RamUsage = ramInfo.used;
                info.RamTotal = ramInfo.total;

                // GPU
                info.GpuUsage = GetGpuUsage();

                // Disk
                var diskInfo = GetDiskInfo();
                info.DiskUsage = diskInfo.usage;
                info.DiskFree = diskInfo.free;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting system info: {ex.Message}");
            }

            return info;
        }

        public void Dispose()
        {
            StopMonitoring();
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
            _cts?.Dispose();
            _computer?.Close();
        }
    }
}
