using AdminHepler.Logger;
using AdminHepler.Models;
using AdminHepler.Utils;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace AdminHepler.Services
{
    public class MonitoringService : IMonitoringService
    {
        public event EventHandler<SystemInfo> DataUpdated;

        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private PerformanceCounter _cpuCounter;
        private Computer _computer;
        private readonly ILogger _logger;

        // Сенсоры CPU
        private ISensor _cpuLoadSensor;
        private ISensor _cpuTempSensor;
        private ISensor _cpuClockSensor;
        private ISensor _cpuPowerSensor;

        // Сенсоры GPU
        private ISensor _gpuLoadSensor;
        private ISensor _gpuTempSensor;
        private ISensor _gpuMemorySensor;
        private ISensor _gpuClockSensor;
        private ISensor _gpuFanSensor;
        private double _gpuMemoryTotal;

        public bool IsMonitoring => _isMonitoring;

        public MonitoringService(ILogger logger)
        {
            _logger = logger;
            InitializeCounters();
            InitializeHardwareMonitoring();
        }

        private void InitializeCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error initializing CPU counter: {ex.Message}");
            }
        }

        private void InitializeHardwareMonitoring()
        {
            try
            {
                _logger.Info("=== Initializing Hardware Monitoring ===");

                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsMotherboardEnabled = true,
                    IsControllerEnabled = false,
                    IsNetworkEnabled = false,
                    IsStorageEnabled = true
                };

                bool hasAdminRights = Utils.IsAdminUtils.IsAdmin();
                _logger.Info($"Admin rights: {hasAdminRights}");

                _computer.Open();
                _logger.Info($"Hardware count: {_computer.Hardware.Count}");

                // Поиск CPU сенсоров
                FindCpuSensors();

                // Поиск GPU сенсоров
                FindGpuSensors();

                _logger.Info("=== Hardware Monitoring Initialized ===");
            }
            catch (Exception ex)
            {
                _logger.Error($"Hardware monitoring initialization failed: {ex.Message}");
                _logger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

        private void FindCpuSensors()
        {
            try
            {
                foreach (var hardware in _computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.Cpu)
                    {
                        _logger.Info($"CPU found: {hardware.Name}");
                        hardware.Update();

                        foreach (var sensor in hardware.Sensors)
                        {
                            _logger.Debug($"  CPU Sensor: '{sensor.Name}' | Type: {sensor.SensorType}");

                            switch (sensor.SensorType)
                            {
                                case SensorType.Load:
                                    if (sensor.Name.Contains("Total") || sensor.Name.Contains("CPU"))
                                        _cpuLoadSensor = sensor;
                                    break;
                                case SensorType.Temperature:
                                    if (sensor.Name.Contains("Core") || sensor.Name.Contains("Package") || sensor.Name.Contains("CPU"))
                                        _cpuTempSensor = sensor;
                                    break;
                                case SensorType.Clock:
                                    if (sensor.Name.Contains("Core") || sensor.Name.Contains("CPU"))
                                        _cpuClockSensor = sensor;
                                    break;
                                case SensorType.Power:
                                    _cpuPowerSensor = sensor;
                                    break;
                            }
                        }

                        _logger.Info($"CPU Sensors - Load: {_cpuLoadSensor?.Name}, Temp: {_cpuTempSensor?.Name}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"CPU sensor search failed: {ex.Message}");
            }
        }

        private void FindGpuSensors()
        {
            try
            {
                foreach (var hardware in _computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.GpuNvidia ||
                        hardware.HardwareType == HardwareType.GpuAmd ||
                        hardware.HardwareType == HardwareType.GpuIntel)
                    {
                        _logger.Info($"GPU found: {hardware.Name}");
                        hardware.Update();

                        foreach (var sensor in hardware.Sensors)
                        {
                            _logger.Debug($"  GPU Sensor: '{sensor.Name}' | Type: {sensor.SensorType}");

                            switch (sensor.SensorType)
                            {
                                case SensorType.Load:
                                    if (sensor.Name.Contains("GPU") || sensor.Name.Contains("Core") || sensor.Name.Contains("3D"))
                                        _gpuLoadSensor = sensor;
                                    if (sensor.Name.Contains("Memory"))
                                        _gpuMemorySensor = sensor;
                                    break;
                                case SensorType.Temperature:
                                    if (sensor.Name.Contains("GPU") || sensor.Name.Contains("Core"))
                                        _gpuTempSensor = sensor;
                                    break;
                                case SensorType.Clock:
                                    if (sensor.Name.Contains("Core") || sensor.Name.Contains("GPU"))
                                        _gpuClockSensor = sensor;
                                    break;
                                case SensorType.Fan:
                                    _gpuFanSensor = sensor;
                                    break;
                            }
                        }

                        // Получаем общий объем памяти GPU
                        try
                        {
                            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                            foreach (var obj in searcher.Get())
                            {
                                var adapterRam = obj["AdapterRAM"];
                                if (adapterRam != null)
                                {
                                    _gpuMemoryTotal = Convert.ToUInt64(adapterRam) / 1024 / 1024; // MB
                                }
                            }
                        }
                        catch { }

                        _logger.Info($"GPU Sensors - Load: {_gpuLoadSensor?.Name}, Temp: {_gpuTempSensor?.Name}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"GPU sensor search failed: {ex.Message}");
            }
        }

        public void StartMonitoring()
        {
            if (_isMonitoring) return;

            _cts = new CancellationTokenSource();
            _isMonitoring = true;
            Task.Run(() => MonitorLoop(_cts.Token));
            _logger.Info("Monitoring started");
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring) return;

            _cts?.Cancel();
            _isMonitoring = false;
            _logger.Info("Monitoring stopped");
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
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Monitoring error: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        }

        private SystemInfo GetSystemInfo()
        {
            var info = new SystemInfo();

            try
            {
                // === CPU ===
                if (_cpuLoadSensor != null)
                {
                    _cpuLoadSensor.Hardware.Update();
                    info.CpuLoad = _cpuLoadSensor.Value ?? 0;
                }
                else if (_cpuCounter != null)
                {
                    info.CpuLoad = _cpuCounter.NextValue();
                }

                if (_cpuTempSensor != null)
                {
                    info.CpuTemperature = _cpuTempSensor.Value ?? 0;
                }

                if (_cpuClockSensor != null)
                {
                    info.CpuFrequency = (_cpuClockSensor.Value ?? 0) / 1000; // MHz → GHz
                }

                if (_cpuPowerSensor != null)
                {
                    info.CpuPower = _cpuPowerSensor.Value ?? 0;
                }

                // === GPU ===
                if (_gpuLoadSensor != null)
                {
                    _gpuLoadSensor.Hardware.Update();
                    info.GpuLoad = _gpuLoadSensor.Value ?? 0;
                }

                if (_gpuTempSensor != null)
                {
                    info.GpuTemperature = _gpuTempSensor.Value ?? 0;
                }

                if (_gpuMemorySensor != null)
                {
                    info.GpuMemoryUsed = _gpuMemorySensor.Value ?? 0;
                }
                info.GpuMemoryTotal = _gpuMemoryTotal;

                if (_gpuClockSensor != null)
                {
                    info.GpuFrequency = (_gpuClockSensor.Value ?? 0) / 1000; // MHz
                }

                if (_gpuFanSensor != null)
                {
                    info.GpuFanSpeed = _gpuFanSensor.Value ?? 0;
                }

                // === RAM ===
                var ramInfo = GetRamInfo();
                info.RamUsed = ramInfo.used;
                info.RamTotal = ramInfo.total;
                info.RamLoad = (info.RamUsed / info.RamTotal) * 100;

                // === DISKS ===
                info.Disks = GetDiskInfo();

                info.LastUpdate = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting system info: {ex.Message}");
            }

            return info;
        }

        private (double used, double total) GetRamInfo()
        {
            try
            {
                var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
                var total = (double)info.TotalPhysicalMemory / 1024 / 1024 / 1024;
                var available = (double)info.AvailablePhysicalMemory / 1024 / 1024 / 1024;
                var used = total - available;
                return (used, total);
            }
            catch
            {
                return (0, 0);
            }
        }

        private List<DiskInfo> GetDiskInfo()
        {
            var disks = new List<DiskInfo>();

            try
            {
                // Получаем все логические диски
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Fixed)
                    .ToList();

                foreach (var drive in drives)
                {
                    var diskInfo = new DiskInfo
                    {
                        Name = drive.Name.TrimEnd('\\'),
                        TotalSize = drive.TotalSize / 1024.0 / 1024.0 / 1024.0,
                        FreeSpace = drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0,
                    };

                    diskInfo.UsedSpace = diskInfo.TotalSize - diskInfo.FreeSpace;
                    diskInfo.UsagePercent = (diskInfo.UsedSpace / diskInfo.TotalSize) * 100;

                    // Попытка определить тип диска (SSD/HDD)
                    diskInfo.Type = GetDiskType(drive.Name.TrimEnd('\\'));

                    // Попытка получить температуру диска
                    diskInfo.Temperature = GetDiskTemperature(drive.Name.TrimEnd('\\'));

                    disks.Add(diskInfo);
                }

                // Также пробуем получить данные из LibreHardwareMonitor (для температуры)
                foreach (var hardware in _computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.Storage)
                    {
                        hardware.Update();
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                var existingDisk = disks.FirstOrDefault(d =>
                                    hardware.Name.Contains(d.Name) || d.Name.Contains(hardware.Name.Substring(0, 2)));

                                if (existingDisk != null)
                                {
                                    existingDisk.Temperature = sensor.Value ?? 0;
                                    existingDisk.Model = hardware.Name;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Disk info error: {ex.Message}");
            }

            return disks;
        }

        private string GetDiskType(string driveLetter)
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_DiskDrive WHERE DeviceID='\\\\.\\PHYSICALDRIVE{driveLetter[0]}'");

                foreach (var obj in searcher.Get())
                {
                    var model = obj["Model"]?.ToString() ?? "";
                    if (model.Contains("SSD") || model.Contains("NVMe") || model.Contains("Solid State"))
                        return "SSD";
                    if (model.Contains("NVMe"))
                        return "NVMe";
                    return "HDD";
                }
            }
            catch { }

            return "Unknown";
        }

        private double GetDiskTemperature(string driveLetter)
        {
            // Температура диска доступна только через SMART
            // LibreHardwareMonitor может предоставить эти данные
            // Возвращаем 0 если недоступно
            return 0;
        }

        public void Dispose()
        {
            StopMonitoring();
            _cpuCounter?.Dispose();
            _cts?.Dispose();
            _computer?.Close();
        }
    }
}
