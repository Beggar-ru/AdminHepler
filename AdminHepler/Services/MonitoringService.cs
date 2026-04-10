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
            _logger = logger;
            InitializeCounters();
            InitializeGpuMonitoring();
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
            try
            {
                _logger.Info("Starting GPU monitoring initialization...");

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
                _logger.Info($"Admin rights: {hasAdminRights}. Opening computer in {(hasAdminRights ? "full" : "portable")} mode.");

                _computer.Open(hasAdminRights ? false : true);
                _logger.Info($"Computer opened. Hardware count: {_computer.Hardware.Count}");

                // Логирование всех найденных устройств
                foreach (var hardware in _computer.Hardware)
                {
                    _logger.Info($"Found hardware: {hardware.Name} (Type: {hardware.HardwareType})");

                    if (hardware.HardwareType == HardwareType.GpuNvidia ||
                        hardware.HardwareType == HardwareType.GpuAmd ||
                        hardware.HardwareType == HardwareType.GpuIntel)
                    {
                        _logger.Info($"GPU detected: {hardware.Name}");
                        hardware.Update();

                        _logger.Info($"Sensors count: {hardware.Sensors.Length}");

                        foreach (var sensor in hardware.Sensors)
                        {
                            _logger.Info($"  Sensor: '{sensor.Name}' Type: {sensor.SensorType} Value: {sensor.Value}");

                            // Ищем любой сенсор загрузки GPU (не только "GPU Core")
                            if (sensor.SensorType == SensorType.Load)
                            {
                                _logger.Info($"  → Selected as GPU sensor: {sensor.Name}");
                                _gpuSensor = sensor;
                                break;
                            }
                        }

                        if (_gpuSensor == null)
                        {
                            _logger.Warning("GPU found but no Load sensor available. Trying GPU usage sensor...");
                            // Альтернативный поиск
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.Name.Contains("GPU") && sensor.SensorType == SensorType.Load)
                                {
                                    _gpuSensor = sensor;
                                    _logger.Info($"  → Selected alternative sensor: {sensor.Name}");
                                    break;
                                }
                            }
                        }

                        if (_gpuSensor != null) break;
                    }
                }

                if (_gpuSensor != null)
                {
                    _logger.Success("GPU monitoring initialized successfully!");
                }
                else
                {
                    _logger.Warning("GPU monitoring: No suitable sensor found. GPU usage will show 0%.");
                    _logger.Warning("Possible reasons:");
                    _logger.Warning("  - Integrated GPU (Intel HD/UHD) may not be supported");
                    _logger.Warning("  - GPU drivers don't expose sensors");
                    _logger.Warning("  - Need administrator rights");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"GPU monitoring initialization failed: {ex.Message}");
                _logger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

        private double GetGpuUsage()
        {
            if (_gpuSensor != null)
            {
                try
                {
                    _gpuSensor.Hardware.Update();
                    var value = _gpuSensor.Value;

                    if (value.HasValue)
                    {
                        return value.Value;
                    }
                    else
                    {
                        _logger.Debug("GPU sensor value is null");
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error reading GPU sensor: {ex.Message}");
                    return 0;
                }
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
