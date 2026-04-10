using AdminHepler.Logger;
using AdminHepler.Models;
using AdminHepler.Utils;
using LibreHardwareMonitor.Hardware;
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
                _logger.Info("=== Starting GPU monitoring initialization ===");

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
                _logger.Info($"Admin rights: {hasAdminRights}");

                _computer.Open();
                _logger.Info($"Computer opened. Hardware count: {_computer.Hardware.Count}");

                // Логирование ВСЕХ найденных устройств
                int gpuIndex = 0;
                foreach (var hardware in _computer.Hardware)
                {
                    _logger.Info($"Hardware [{gpuIndex++}]: {hardware.Name} (Type: {hardware.HardwareType})");

                    // Проверяем ЛЮБОЕ GPU устройство
                    if (hardware.HardwareType == HardwareType.GpuNvidia ||
                        hardware.HardwareType == HardwareType.GpuAmd ||
                        hardware.HardwareType == HardwareType.GpuIntel)
                    {
                        _logger.Info($"→ GPU detected: {hardware.Name}");
                        hardware.Update();

                        _logger.Info($"Sensors available: {hardware.Sensors.Count()}");

                        // Логирование ВСЕХ сенсоров
                        int sensorIndex = 0;
                        foreach (var sensor in hardware.Sensors)
                        {
                            _logger.Info($"    Sensor [{sensorIndex++}]: '{sensor.Name}' | Type: {sensor.SensorType} | Value: {sensor.Value}");
                        }

                        // Поиск ЛЮБОГО сенсора загрузки (не только "GPU Core")
                        foreach (var sensor in hardware.Sensors)
                        {
                            // Ищем сенсоры загрузки GPU
                            if (sensor.SensorType == SensorType.Load)
                            {
                                string sensorName = sensor.Name.ToLower();

                                // Проверяем различные варианты имен
                                if (sensorName.Contains("gpu") ||
                                    sensorName.Contains("engine") ||
                                    sensorName.Contains("3d") ||
                                    sensorName.Contains("core") ||
                                    sensorName.Contains("usage"))
                                {
                                    _logger.Info($"  ✓ Selected GPU sensor: '{sensor.Name}' (Type: {sensor.SensorType})");
                                    _gpuSensor = sensor;
                                    break;
                                }
                            }
                        }

                        // Если не нашли через Load, пробуем альтернативы
                        if (_gpuSensor == null)
                        {
                            _logger.Warning("  No Load sensor found. Trying alternative sensors...");

                            foreach (var sensor in hardware.Sensors)
                            {
                                // Пробуем любой сенсор с "GPU" в имени
                                if (sensor.Name.ToLower().Contains("gpu"))
                                {
                                    _logger.Info($"  → Using alternative sensor: '{sensor.Name}' (Type: {sensor.SensorType})");
                                    _gpuSensor = sensor;
                                    break;
                                }
                            }
                        }

                        if (_gpuSensor != null)
                        {
                            _logger.Success("✓ GPU monitoring initialized successfully!");
                            _logger.Info($"  Sensor: {_gpuSensor.Name}");
                            _logger.Info($"  Type: {_gpuSensor.SensorType}");
                            break; // Нашли GPU - выходим
                        }
                        else
                        {
                            _logger.Error("  ✗ GPU found but NO suitable sensor detected!");
                        }
                    }
                }

                if (_gpuSensor == null)
                {
                    _logger.Warning("=== GPU MONITORING FAILED ===");
                    _logger.Warning("Possible reasons:");
                    _logger.Warning("  1. Integrated GPU (Intel HD/UHD/Iris) - limited support");
                    _logger.Warning("  2. GPU drivers don't expose sensors via OHM");
                    _logger.Warning("  3. Need to update OpenHardwareMonitor to latest version");
                    _logger.Warning("  4. Some GPUs require additional drivers (NVAPI for NVIDIA)");
                    _logger.Info("GPU usage will display 0%");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"GPU monitoring initialization FAILED: {ex.Message}");
                _logger.Error($"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    _logger.Error($"Inner exception: {ex.InnerException.Message}");
                }
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
                        return Math.Round(value.Value, 1);
                    }
                    else
                    {
                        // Сенсор есть, но значение null
                        _logger.Debug("GPU sensor exists but value is null");
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
