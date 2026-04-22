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
        private ISensor _cpuPowerSensor;
        private List<ISensor> _cpuClockSensors = new List<ISensor>();

        // Сенсоры GPU
        private ISensor _gpuLoadSensor;
        private ISensor _gpuTempSensor;
        private ISensor _gpuMemorySensor;
        private ISensor _gpuClockSensor;
        private ISensor _gpuFanSensor;
        private double _gpuMemoryTotal;

        // Сенсоры дисков
        private Dictionary<string, ISensor> _diskTempSensors = new Dictionary<string, ISensor>();

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

                if (!hasAdminRights)
                {
                    _logger.Warning("Running without admin rights - some sensors may return 0 (especially AMD CPU temperature)");
                }

                _computer.Open();
                _logger.Info($"Hardware count: {_computer.Hardware.Count}");

                FindCpuSensors();
                FindGpuSensors();
                FindDiskSensors();

                _logger.Info("=== Hardware Monitoring Initialized ===");
            }
            catch (Exception ex)
            {
                _logger.Error($"Hardware monitoring initialization failed: {ex.Message}");
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
                            _logger.Debug($"  CPU Sensor: '{sensor.Name}' | Type: {sensor.SensorType} | Value: {sensor.Value}");

                            switch (sensor.SensorType)
                            {
                                case SensorType.Load:
                                    // Приоритет: CPU Total > CPU > Package
                                    if (sensor.Name.Contains("Total"))
                                        _cpuLoadSensor = sensor;
                                    else if (sensor.Name.Contains("CPU") && _cpuLoadSensor == null)
                                        _cpuLoadSensor = sensor;
                                    break;
                                case SensorType.Temperature:
                                    // Для AMD Ryzen: Tctl/Tdie - основной сенсор температуры
                                    // Для Intel: Package или Core
                                    if (sensor.Name.Contains("Tctl") || sensor.Name.Contains("Tdie"))
                                    {
                                        _cpuTempSensor = sensor;
                                        _logger.Info($"✓ Selected AMD temperature sensor: {sensor.Name}");
                                    }
                                    else if (sensor.Name.Contains("Package") && _cpuTempSensor == null)
                                        _cpuTempSensor = sensor;
                                    else if (sensor.Name.Contains("Core") && _cpuTempSensor == null)
                                        _cpuTempSensor = sensor;
                                    break;
                                case SensorType.Clock:
                                    // Собираем все сенсоры частоты ядер
                                    if (sensor.Name.Contains("Core") || sensor.Name.Contains("CPU"))
                                    {
                                        // Проверяем что значение не null и > 0
                                        if (sensor.Value.HasValue && sensor.Value.Value > 0)
                                            _cpuClockSensors.Add(sensor);
                                    }
                                    break;
                                case SensorType.Power:
                                    _cpuPowerSensor = sensor;
                                    break;
                            }
                        }

                        if (_cpuLoadSensor == null)
                        {
                            _logger.Warning("CPU Load sensor not found, using PerformanceCounter");
                        }

                        if (_cpuClockSensors.Count == 0)
                        {
                            _logger.Warning("No valid CPU Clock sensors found, will use WMI fallback (base frequency only)");
                        }

                        _logger.Info($"CPU Sensors - Load: {_cpuLoadSensor?.Name ?? "PerformanceCounter"}, " +
                            $"Temp: {_cpuTempSensor?.Name ?? "N/A"}, " +
                            $"Clock sensors (valid): {_cpuClockSensors.Count}");
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

                        double lhmMemoryTotal = 0;

                        foreach (var sensor in hardware.Sensors)
                        {
                            _logger.Debug($"  GPU Sensor: '{sensor.Name}' | Type: {sensor.SensorType} | Value: {sensor.Value}");

                            switch (sensor.SensorType)
                            {
                                case SensorType.Load:
                                    // Загрузка GPU Core (не Memory!)
                                    if (sensor.Name.Contains("GPU") && sensor.Name.Contains("Core"))
                                        _gpuLoadSensor = sensor;
                                    else if (sensor.Name.Contains("3D") && _gpuLoadSensor == null)
                                        _gpuLoadSensor = sensor;
                                    // Память GPU через Load%
                                    if (sensor.Name.Contains("Memory") && sensor.Name.Contains("Load"))
                                        _gpuMemorySensor = sensor;
                                    break;
                                case SensorType.Temperature:
                                    // Приоритет: GPU Core > GPU > Hot Spot > Memory Junction
                                    if (sensor.Name.Contains("GPU") && sensor.Name.Contains("Core"))
                                        _gpuTempSensor = sensor;
                                    else if (sensor.Name.Contains("GPU") && _gpuTempSensor == null)
                                        _gpuTempSensor = sensor;
                                    break;
                                case SensorType.Clock:
                                    // Только GPU Core Clock (не Memory Clock!)
                                    if (sensor.Name.Contains("GPU") && sensor.Name.Contains("Core"))
                                        _gpuClockSensor = sensor;
                                    else if (sensor.Name.Contains("Core") &&
                                             !sensor.Name.Contains("Memory") &&
                                             !sensor.Name.Contains("Shader") &&
                                             _gpuClockSensor == null)
                                        _gpuClockSensor = sensor;
                                    break;
                                case SensorType.Fan:
                                    _gpuFanSensor = sensor;
                                    break;
                                case SensorType.SmallData:
                                    // Память GPU используемая (в MB)
                                    if (sensor.Name.Contains("Memory") && sensor.Name.Contains("Used"))
                                        _gpuMemorySensor = sensor;
                                    // Память GPU общая
                                    if (sensor.Name.Contains("Memory") && sensor.Name.Contains("Total"))
                                    {
                                        lhmMemoryTotal = sensor.Value ?? 0;
                                        _logger.Info($"✓ GPU Memory Total (LHM): {lhmMemoryTotal} MB");
                                    }
                                    break;
                            }
                        }

                        // Приоритет: LHM > Registry > WMI
                        if (lhmMemoryTotal > 0)
                        {
                            _gpuMemoryTotal = lhmMemoryTotal;
                        }
                        else
                        {
                            bool gotFromRegistry = TryGetGpuMemoryFromRegistry(out double registryMemory);
                            if (gotFromRegistry && registryMemory > 0)
                            {
                                _gpuMemoryTotal = registryMemory;
                                _logger.Info($"✓ GPU Memory Total (Registry): {_gpuMemoryTotal} MB");
                            }
                            else
                            {
                                try
                                {
                                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                                    foreach (var obj in searcher.Get())
                                    {
                                        var adapterRam = obj["AdapterRAM"];
                                        if (adapterRam != null)
                                        {
                                            try
                                            {
                                                _gpuMemoryTotal = Convert.ToUInt64(adapterRam) / 1024.0 / 1024.0;
                                            }
                                            catch
                                            {
                                                _gpuMemoryTotal = Convert.ToUInt32(adapterRam) / 1024.0 / 1024.0;
                                            }
                                            _logger.Warning($"⚠ GPU Memory Total (WMI fallback): {_gpuMemoryTotal} MB");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warning($"✗ Could not get GPU memory from WMI: {ex.Message}");
                                }
                            }
                        }

                        _logger.Info($"GPU Sensors - Load: {_gpuLoadSensor?.Name ?? "N/A"}, " +
                            $"Temp: {_gpuTempSensor?.Name ?? "N/A"}, " +
                            $"Memory Total: {_gpuMemoryTotal} MB, " +
                            $"Clock: {_gpuClockSensor?.Name ?? "N/A"}");
                        break;
                    }
                }

                if (_gpuLoadSensor == null)
                {
                    _logger.Warning("GPU Load sensor not found - integrated GPU may not support monitoring");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"GPU sensor search failed: {ex.Message}");
            }
        }

        private bool TryGetGpuMemoryFromRegistry(out double memoryMb)
        {
            memoryMb = 0;
            try
            {
                using (var baseKey = Microsoft.Win32.Registry.LocalMachine
                    .OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"))
                {
                    if (baseKey == null) return false;

                    foreach (var subKeyName in baseKey.GetSubKeyNames())
                    {
                        if (subKeyName == "Properties") continue;

                        using (var subKey = baseKey.OpenSubKey(subKeyName))
                        {
                            var qwMemorySize = subKey?.GetValue("HardwareInformation.qwMemorySize");
                            if (qwMemorySize != null)
                            {
                                ulong bytes = Convert.ToUInt64(qwMemorySize);
                                if (bytes > 0)
                                {
                                    memoryMb = bytes / 1024.0 / 1024.0;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"Registry GPU memory read failed: {ex.Message}");
            }
            return false;
        }

        private void FindDiskSensors()
        {
            try
            {
                foreach (var hardware in _computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.Storage)
                    {
                        _logger.Info($"Storage found: {hardware.Name}");
                        hardware.Update();

                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                var diskName = hardware.Name;
                                _logger.Debug($"  Disk Sensor: '{sensor.Name}' | Value: {sensor.Value}");

                                if (!_diskTempSensors.ContainsKey(diskName))
                                {
                                    _diskTempSensors[diskName] = sensor;
                                }
                            }
                        }
                    }
                }

                _logger.Info($"Disk temperature sensors found: {_diskTempSensors.Count}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Disk sensor search failed: {ex.Message}");
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
                    _cpuTempSensor.Hardware.Update();
                    var tempValue = _cpuTempSensor.Value;

                    // Для AMD Ryzen без прав админа температура может быть 0
                    if (tempValue.HasValue && tempValue.Value > 0)
                    {
                        info.CpuTemperature = tempValue.Value;
                    }
                    else if (!Utils.IsAdminUtils.IsAdmin())
                    {
                        // Логгируем только один раз (визуально в UI будет 0)
                        _logger.Debug("CPU temperature = 0 (AMD Ryzen requires admin rights for accurate reading)");
                    }
                }

                // CPU частота: LHM сенсоры ИЛИ WMI fallback
                if (_cpuClockSensors.Count > 0)
                {
                    foreach (var sensor in _cpuClockSensors)
                    {
                        sensor.Hardware.Update();
                    }

                    double avgMhz = _cpuClockSensors.Average(s => s.Value ?? 0);
                    if (avgMhz > 0)
                    {
                        info.CpuFrequency = avgMhz / 1000.0;
                    }
                }

                // Если LHM не дал частоту - используем WMI
                if (info.CpuFrequency == 0)
                {
                    try
                    {
                        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                        foreach (var obj in searcher.Get())
                        {
                            var currentClockSpeed = obj["CurrentClockSpeed"];
                            if (currentClockSpeed != null)
                            {
                                info.CpuFrequency = Convert.ToDouble(currentClockSpeed) / 1000.0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"WMI CPU frequency failed: {ex.Message}");
                    }
                }

                if (_cpuPowerSensor != null)
                {
                    _cpuPowerSensor.Hardware.Update();
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
                    _gpuTempSensor.Hardware.Update();
                    info.GpuTemperature = _gpuTempSensor.Value ?? 0;
                }

                if (_gpuMemorySensor != null)
                {
                    _gpuMemorySensor.Hardware.Update();
                    var memValue = _gpuMemorySensor.Value ?? 0;

                    // Если сенсор возвращает проценты (Load)
                    if (_gpuMemorySensor.SensorType == SensorType.Load)
                    {
                        info.GpuMemoryUsed = (_gpuMemoryTotal * memValue) / 100.0;
                    }
                    else
                    {
                        // Сенсор возвращает MB (SmallData)
                        info.GpuMemoryUsed = memValue;
                    }
                }

                info.GpuMemoryTotal = _gpuMemoryTotal;

                if (_gpuClockSensor != null)
                {
                    _gpuClockSensor.Hardware.Update();
                    info.GpuFrequency = _gpuClockSensor.Value ?? 0;
                }

                if (_gpuFanSensor != null)
                {
                    _gpuFanSensor.Hardware.Update();
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
                // Получаем ВСЕ диски (не только Fixed)
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Network)
                    .ToList();

                _logger.Debug($"Found {drives.Count} drives to monitor");

                foreach (var drive in drives)
                {
                    try
                    {
                        if (!drive.IsReady)
                        {
                            _logger.Debug($"Drive {drive.Name} not ready, skipping");
                            continue;
                        }

                        var diskInfo = new DiskInfo
                        {
                            Name = drive.Name.TrimEnd('\\'),
                            TotalSize = drive.TotalSize / 1024.0 / 1024.0 / 1024.0,
                            FreeSpace = drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0,
                        };

                        diskInfo.UsedSpace = diskInfo.TotalSize - diskInfo.FreeSpace;
                        diskInfo.UsagePercent = (diskInfo.UsedSpace / diskInfo.TotalSize) * 100;
                        diskInfo.Type = GetDiskType(drive.Name.TrimEnd('\\'));
                        diskInfo.Temperature = GetDiskTemperature(drive.Name.TrimEnd('\\'));
                        diskInfo.Model = GetDiskModel(drive.Name.TrimEnd('\\'));

                        _logger.Debug($"Disk {diskInfo.Name}: Type={diskInfo.Type}, Model={diskInfo.Model}");

                        disks.Add(diskInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"Error reading drive {drive.Name}: {ex.Message}");
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
                char letter = driveLetter[0];
                int driveIndex = letter - 'A';

                var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_DiskDrive WHERE Index={driveIndex}");

                foreach (var obj in searcher.Get())
                {
                    var model = obj["Model"]?.ToString() ?? "";
                    var mediaType = obj["MediaType"]?.ToString() ?? "";

                    _logger.Debug($"Disk {driveLetter}: Model={model}, MediaType={mediaType}");

                    // Проверяем модель на SSD/NVMe маркеры
                    if (model.Contains("NVMe") || model.Contains("M.2") || model.Contains("PCIe"))
                        return "NVMe";
                    if (model.Contains("SSD") || model.Contains("Solid State") || model.Contains("SATADOM"))
                        return "SSD";
                    if (model.Contains("HDD") || model.Contains("Hard Disk"))
                        return "HDD";

                    // Проверяем MediaType
                    if (mediaType.Contains("SSD") || mediaType.Contains("Solid State"))
                        return "SSD";
                    if (mediaType.Contains("NVMe"))
                        return "NVMe";

                    // Для Microsoft Storage Space Device пробуем альтернативный метод
                    if (model.Contains("Storage Space"))
                    {
                        // Пул хранилищ может содержать SSD или HDD - определяем по первому физическому диску
                        return "Unknown"; // Оставляем Unknown для пулов
                    }

                    return "HDD"; // По умолчанию
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"GetDiskType error for {driveLetter}: {ex.Message}");
            }

            return "Unknown";
        }

        private string GetDiskModel(string driveLetter)
        {
            try
            {
                char letter = driveLetter[0];
                int driveIndex = letter - 'A';

                var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_DiskDrive WHERE Index={driveIndex}");

                foreach (var obj in searcher.Get())
                {
                    return obj["Model"]?.ToString() ?? "Unknown";
                }
            }
            catch { }

            return "Unknown";
        }

        private double GetDiskTemperature(string driveLetter)
        {
            try
            {
                // Ищем в сенсорах LHM
                foreach (var kvp in _diskTempSensors)
                {
                    if (kvp.Key.Contains(driveLetter) || driveLetter.Contains(kvp.Key.Substring(0, 2)))
                    {
                        kvp.Value.Hardware.Update();
                        return kvp.Value.Value ?? 0;
                    }
                }

                // Альтернативный поиск по всем дискам
                foreach (var hardware in _computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.Storage)
                    {
                        hardware.Update();
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                return sensor.Value ?? 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"GetDiskTemperature error: {ex.Message}");
            }

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