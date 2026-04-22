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
    // === БЛОК 1: ПОЛЯ КЛАССА ===
    // Источник: MonitoringServices.txt (строки 20-35)
    // Изменения: Добавлено cpuVendor для определения AMD/Intel

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

        // НОВОЕ: Определение вендора процессора
        private string _cpuVendor = "Unknown";
        private bool _needsAdminForTemp = false;

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
                _logger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

        // === БЛОК 3: ВЫБОР РАБОЧЕГО ДАТЧИКА ТЕМПЕРАТУРЫ ===
        // Источник: Новый метод (не было в исходном коде)
        // Изменения: Добавлен для кроссплатформенной поддержки AMD/Intel

        /// <summary>
        /// Выбирает ОДИН рабочий датчик температуры из списка
        /// Приоритет зависит от вендора процессора
        /// </summary>
        private ISensor SelectWorkingTemperatureSensor(List<ISensor> sensors)
        {
            if (sensors.Count == 0)
            {
                _logger.Warning("No CPU temperature sensors found");
                return null;
            }

            // Приоритет 1: Ищем датчик со значением > 0 (рабочий)
            foreach (var sensor in sensors)
            {
                if (sensor.Value.HasValue && sensor.Value.Value > 0)
                {
                    _logger.Info($"✓ Selected working temperature sensor: '{sensor.Name}' = {sensor.Value.Value}°C");
                    return sensor;
                }
            }

            // Приоритет 2: Если все возвращают 0, выбираем по вендору
            if (_cpuVendor == "AMD")
            {
                // AMD: Tdie > Tctl > Package
                var amdPriority = new[] { "Tdie", "Tctl", "Package", "Core" };
                foreach (var priority in amdPriority)
                {
                    var sensor = sensors.FirstOrDefault(s => s.Name.Contains(priority));
                    if (sensor != null)
                    {
                        _needsAdminForTemp = true;
                        _logger.Warning($"⚠ AMD sensor '{sensor.Name}' returns 0 - requires admin rights or AMD Chipset Drivers");
                        return sensor;
                    }
                }
            }
            else if (_cpuVendor == "Intel")
            {
                // Intel: Package > Core > IA
                var intelPriority = new[] { "Package", "Core", "IA", "CPU" };
                foreach (var priority in intelPriority)
                {
                    var sensor = sensors.FirstOrDefault(s => s.Name.Contains(priority));
                    if (sensor != null)
                    {
                        _logger.Info($"✓ Selected Intel temperature sensor: '{sensor.Name}'");
                        return sensor;
                    }
                }
            }

            // Приоритет 3: Возвращаем первый попавшийся
            _logger.Warning($"⚠ Using fallback temperature sensor: '{sensors[0].Name}'");
            return sensors[0];
        }


        // === БЛОК 2: ПОИСК СЕНСОРОВ CPU ===
        // Источник: MonitoringServices.txt (метод FindCpuSensors, строки 105-145)
        // Изменения: Добавлена логика для AMD и Intel, выбор рабочего датчика температуры

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

                        // Обходим SubHardware (AMD SMU, Intel MSR и др.)
                        foreach (var sub in hardware.SubHardware)
                        {
                            sub.Update();
                            _logger.Info($"  CPU SubHardware: {sub.Name}");
                            CollectCpuSensors(sub.Sensors, isSubHardware: true);
                        }

                        CollectCpuSensors(hardware.Sensors, isSubHardware: false);

                        if (_cpuLoadSensor == null)
                            _logger.Warning("CPU Load sensor not found, using PerformanceCounter");

                        _logger.Info($"CPU Sensors - Load: {_cpuLoadSensor?.Name ?? "PerformanceCounter"}, " +
                                     $"Temp: {_cpuTempSensor?.Name ?? "N/A"}, " +
                                     $"Clock sensors: {_cpuClockSensors.Count}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"CPU sensor search failed: {ex.Message}");
            }
        }

        private void CollectCpuSensors(IEnumerable<ISensor> sensors, bool isSubHardware)
        {
            foreach (var sensor in sensors)
            {
                _logger.Debug($"  {(isSubHardware ? "[Sub]" : "")} CPU Sensor: '{sensor.Name}' | Type: {sensor.SensorType} | Value: {sensor.Value}");

                switch (sensor.SensorType)
                {
                    case SensorType.Load:
                        if (sensor.Name.Contains("Total") || sensor.Name.Contains("CPU Total") || sensor.Name.Contains("Package"))
                            _cpuLoadSensor = sensor;
                        break;

                    case SensorType.Temperature:
                        // AMD Ryzen: "Core (Tctl/Tdie)" или "Tdie" — основной сенсор
                        // Intel:     "CPU Package" или "Package"
                        // Приоритеты: Package > Tctl/Tdie > Core Average > Core #N
                        if (sensor.Name == "Core (Tctl/Tdie)" || sensor.Name == "Tdie" || sensor.Name == "Tctl/Tdie")
                        {
                            _cpuTempSensor = sensor; // AMD — высший приоритет
                        }
                        else if ((sensor.Name.Contains("Package") || sensor.Name == "CPU Package") && _cpuTempSensor == null)
                        {
                            _cpuTempSensor = sensor; // Intel Package
                        }
                        else if (sensor.Name.Contains("Core") && _cpuTempSensor == null)
                        {
                            _cpuTempSensor = sensor; // Fallback — любое ядро
                        }
                        break;

                    case SensorType.Clock:
                        if (sensor.Name.Contains("Core"))
                            _cpuClockSensors.Add(sensor);
                        break;

                    case SensorType.Power:
                        if (_cpuPowerSensor == null || sensor.Name.Contains("Package"))
                            _cpuPowerSensor = sensor;
                        break;
                }
            }
        }


        /// <summary>
        /// ИСПРАВЛЕНИЕ #1: Выбирает ОДИН рабочий датчик температуры из списка
        /// </summary>
        /*
        private ISensor SelectWorkingTemperatureSensor(List<ISensor> sensors)
        {
            if (sensors.Count == 0)
            {
                _logger.Warning("No CPU temperature sensors found");
                return null;
            }

            // Приоритет 1: Ищем датчик со значением > 0 (рабочий)
            foreach (var sensor in sensors)
            {
                if (sensor.Value.HasValue && sensor.Value.Value > 0)
                {
                    _logger.Info($"✓ Selected working temperature sensor: '{sensor.Name}' = {sensor.Value.Value}°C");
                    return sensor;
                }
            }

            // Приоритет 2: Если все возвращают 0, выбираем по имени (Tdie > Tctl > Package)
            var priorityNames = new[] { "Tdie", "Tctl", "Package", "Core" };

            foreach (var priority in priorityNames)
            {
                var sensor = sensors.FirstOrDefault(s => s.Name.Contains(priority));
                if (sensor != null)
                {
                    _logger.Warning($"⚠ Temperature sensor '{sensor.Name}' returns 0 (may require admin rights or driver update)");
                    return sensor;
                }
            }
         

            // Приоритет 3: Возвращаем первый попавшийся
            _logger.Warning($"⚠ Using fallback temperature sensor: '{sensors[0].Name}'");
            return sensors[0];
        }
        */
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
                                    if (sensor.Name.Contains("GPU") && sensor.Name.Contains("Core"))
                                        _gpuLoadSensor = sensor;
                                    if (sensor.Name.Contains("Memory") && sensor.Name.Contains("Load"))
                                        _gpuMemorySensor = sensor;
                                    break;
                                case SensorType.Temperature:
                                    // Выбираем GPU Core, не Memory Junction
                                    if (sensor.Name.Contains("GPU") && sensor.Name.Contains("Core"))
                                        _gpuTempSensor = sensor;
                                    else if (sensor.Name.Contains("GPU") && _gpuTempSensor == null)
                                        _gpuTempSensor = sensor;
                                    break;
                                case SensorType.Clock:
                                    if (sensor.Name.Contains("GPU") && sensor.Name.Contains("Core"))
                                        _gpuClockSensor = sensor;
                                    break;
                                case SensorType.Fan:
                                    _gpuFanSensor = sensor;
                                    break;
                                case SensorType.SmallData:
                                    if (sensor.Name.Contains("Memory") && sensor.Name.Contains("Used"))
                                        _gpuMemorySensor = sensor;
                                    if (sensor.Name.Contains("Memory") && sensor.Name.Contains("Total"))
                                    {
                                        lhmMemoryTotal = sensor.Value ?? 0;
                                        _logger.Info($"✓ GPU Memory Total (LHM): {lhmMemoryTotal} MB");
                                    }
                                    break;
                            }
                        }

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
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warning($"Could not get GPU memory from WMI: {ex.Message}");
                                }
                            }
                        }

                        _logger.Info($"GPU Sensors - Load: {_gpuLoadSensor?.Name ?? "N/A"}, " +
                            $"Temp: {_gpuTempSensor?.Name ?? "N/A"}, " +
                            $"Memory Total: {_gpuMemoryTotal} MB");
                        break;
                    }
                }

                if (_gpuLoadSensor == null)
                {
                    _logger.Warning("GPU Load sensor not found");
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

        // === БЛОК 4: ПОЛУЧЕНИЕ СИСТЕМНОЙ ИНФОРМАЦИИ (CPU часть) ===
        // Источник: MonitoringServices.txt (метод GetSystemInfo, строки 245-280)
        // Изменения: Добавлен WMI fallback для частоты CPU, проверка температуры

        private SystemInfo GetSystemInfo()
        {
            var info = new SystemInfo();

            try
            {
                // === CPU ===
                // Загрузка CPU
                if (_cpuLoadSensor != null)
                {
                    _cpuLoadSensor.Hardware.Update();
                    info.CpuLoad = _cpuLoadSensor.Value ?? 0;
                }
                else if (_cpuCounter != null)
                {
                    info.CpuLoad = _cpuCounter.NextValue();
                }

                // Температура CPU
                // В GetSystemInfo, блок температуры CPU:
                if (_cpuTempSensor != null)
                {
                    // Важно: обновляем и SubHardware если сенсор оттуда
                    _cpuTempSensor.Hardware.Update();
                    foreach (var sub in _cpuTempSensor.Hardware.SubHardware)
                        sub.Update();
                    info.CpuTemperature = _cpuTempSensor.Value ?? 0;
                }

                // Частота CPU: LHM сенсоры → WMI fallback
                if (_cpuClockSensors.Count > 0)
                {
                    foreach (var sensor in _cpuClockSensors)
                    {
                        sensor.Hardware.Update();
                    }

                    double avgMhz = _cpuClockSensors.Average(s => s.Value ?? 0);
                    if (avgMhz > 0)
                    {
                        info.CpuFrequency = avgMhz / 1000.0; // MHz → GHz
                        _logger.Debug($"CPU Frequency (LHM): {info.CpuFrequency:F2} GHz from {_cpuClockSensors.Count} cores");
                    }
                }

                // WMI fallback если LHM не дал частоту
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
                                _logger.Debug($"CPU Frequency (WMI): {info.CpuFrequency:F2} GHz");
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

                    if (_gpuMemorySensor.SensorType == SensorType.Load)
                    {
                        info.GpuMemoryUsed = (_gpuMemoryTotal * memValue) / 100.0;
                    }
                    else
                    {
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
                    diskInfo.Type = GetDiskType(drive.Name.TrimEnd('\\'));
                    diskInfo.Temperature = GetDiskTemperature(drive.Name.TrimEnd('\\'));
                    diskInfo.Model = GetDiskModel(drive.Name.TrimEnd('\\'));

                    disks.Add(diskInfo);
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

                    if (model.Contains("NVMe") || model.Contains("M.2"))
                        return "NVMe";
                    if (model.Contains("SSD") || model.Contains("Solid State"))
                        return "SSD";
                    if (mediaType.Contains("SSD") || mediaType.Contains("Solid State"))
                        return "SSD";

                    return "HDD";
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"GetDiskType error: {ex.Message}");
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
                foreach (var kvp in _diskTempSensors)
                {
                    if (kvp.Key.Contains(driveLetter) || driveLetter.Contains(kvp.Key.Substring(0, 2)))
                    {
                        kvp.Value.Hardware.Update();
                        return kvp.Value.Value ?? 0;
                    }
                }

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