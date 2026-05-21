/*
 * ================================================================
 *  MonitoringService — расширенный сбор данных о железе
 * ================================================================
 *  Используемые библиотеки / NuGet:
 *    LibreHardwareMonitorLib  — LHM (CPU/GPU/MB/Storage сенсоры)
 *    Microsoft.VisualBasic    — ComputerInfo (RAM) — встроена в .NET
 *    System.Management        — WMI (Win32_*) — встроена в .NET
 *
 *  NuGet (добавить в .csproj):
 *    <PackageReference Include="LibreHardwareMonitorLib" Version="0.9.*" />
 *
 *  Права: температуры CPU AMD, SMART дисков — требуют прав администратора.
 * ================================================================
 */

using AdminHelper.Logger;
using AdminHelper.Models;
using AdminHelper.Utils;
using LibreHardwareMonitor.Hardware;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace AdminHelper.Services
{
    public class MonitoringService : IMonitoringService
    {
        // ── события и состояние ───────────────────────────────────
        public event EventHandler<SystemInfo> DataUpdated;
        public bool IsMonitoring => _isMonitoring;
        public int PollingIntervalMs { get; set; } = 1000;

        // ── LHM ──────────────────────────────────────────────────
        private Computer _computer;
        private readonly ILogger _logger;
        private CancellationTokenSource _cts;
        private bool _isMonitoring;

        // ── CPU сенсоры ───────────────────────────────────────────
        private IHardware _cpuHardware;
        private ISensor _cpuLoadTotal;
        private ISensor _cpuTempPackage;
        private ISensor _cpuPowerPackage;
        private ISensor _cpuPowerCores;
        private ISensor _cpuPowerMemory;
        private ISensor _cpuVoltageCore;
        private List<ISensor> _cpuCoreLoads = new();
        private List<ISensor> _cpuCoreTemps = new();
        private List<ISensor> _cpuCoreClocks = new();
        private PerformanceCounter _cpuFallbackCounter;
        private string _cpuVendor = "Unknown";

        // ── GPU сенсоры (несколько GPU) ───────────────────────────
        private List<GpuSensorSet> _gpuSensors = new();

        // ── MB сенсоры ────────────────────────────────────────────
        private IHardware _mbHardware;
        private List<ISensor> _mbSensors = new();

        // ── Storage сенсоры ───────────────────────────────────────
        // ключ — hardware.Name (модель диска)
        private Dictionary<string, StorageSensorSet> _storageSensors = new();

        // ── Диски производительность (PerformanceCounter) ─────────
        private Dictionary<string, DiskPerfCounters> _diskPerfCounters = new();

        // ── Сетевые адаптеры (PerformanceCounter) ─────────────────
        private Dictionary<string, NetCounters> _netCounters = new();

        // ── Статические данные (читаются один раз) ────────────────
        private string _cpuName;
        private int _cpuPhysicalCores;
        private int _cpuLogicalCores;
        private double _cpuBaseClockMHz;
        private double _cpuMaxClockMHz;
        private List<RamModuleInfo> _ramModules = new();
        private string _ramType = "";
        private double _ramFreqMHz;
        private string _ramTimings = "";
        private string _mbManufacturer = "";
        private string _mbProduct = "";
        private string _mbBiosVersion = "";
        private string _mbBiosDate = "";

        // ── WMI кэши ─────────────────────────────────────────────
        private Dictionary<string, string> _driveLetterToModel = new();
        private Dictionary<string, string> _driveLetterToSerial = new();
        private Dictionary<string, string> _driveLetterToBus = new();
        private Dictionary<string, string> _driveLetterToFS = new();
        private Dictionary<string, string> _driveLetterToMediaType = new();
        private Dictionary<string, uint?> _driveLetterToRotationRate = new();

        // ================================================================
        #region Инициализация

        public MonitoringService(ILogger logger)
        {
            _logger = logger;
            try
            {
                _logger.Info("=== MonitoringService: Init ===");
                InitStaticCpuInfo();
                InitStaticRamInfo();
                InitStaticMbInfo();
                InitDriveLetterMaps();
                InitLhm();
                InitDiskPerfCounters();
                InitNetCounters();
                _logger.Info("=== MonitoringService: Ready ===");
            }
            catch (Exception ex)
            {
                _logger.Error($"MonitoringService init failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // ── 1. Статика CPU (WMI) ─────────────────────────────────
        private void InitStaticCpuInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    _cpuName = obj["Name"]?.ToString()?.Trim() ?? "Unknown";
                    _cpuPhysicalCores = Convert.ToInt32(obj["NumberOfCores"] ?? 0);
                    _cpuLogicalCores = Convert.ToInt32(obj["NumberOfLogicalProcessors"] ?? 0);
                    _cpuBaseClockMHz = Convert.ToDouble(obj["CurrentClockSpeed"] ?? 0);
                    _cpuMaxClockMHz = Convert.ToDouble(obj["MaxClockSpeed"] ?? 0);

                    string mfr = obj["Manufacturer"]?.ToString() ?? "";
                    if (mfr.Contains("AMD")) _cpuVendor = "AMD";
                    else if (mfr.Contains("Intel")) _cpuVendor = "Intel";

                    _logger.Info($"CPU: {_cpuName}, Cores: {_cpuPhysicalCores}P/{_cpuLogicalCores}L, " +
                                 $"Base: {_cpuBaseClockMHz:F0} MHz, Max: {_cpuMaxClockMHz:F0} MHz, Vendor: {_cpuVendor}");
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"WMI CPU info failed: {ex.Message}");
            }

            // PerformanceCounter — fallback для нагрузки если LHM не справится
            try
            {
                _cpuFallbackCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            catch { }
        }

        // ── 2. Статика RAM (WMI) ──────────────────────────────────
        private void InitStaticRamInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var module = new RamModuleInfo
                    {
                        Slot = obj["DeviceLocator"]?.ToString() ?? "",
                        Manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "",
                        PartNumber = obj["PartNumber"]?.ToString()?.Trim() ?? "",
                        CapacityGB = Convert.ToUInt64(obj["Capacity"] ?? 0UL) / 1024.0 / 1024.0 / 1024.0,
                        SpeedMHz = Convert.ToDouble(obj["ConfiguredClockSpeed"] ?? obj["Speed"] ?? 0),
                        FormFactor = DecodeRamFormFactor(Convert.ToInt32(obj["FormFactor"] ?? 0)),
                        MemoryType = DecodeRamType(Convert.ToInt32(obj["SMBIOSMemoryType"] ?? 0)),
                        VoltageV = Convert.ToDouble(obj["ConfiguredVoltage"] ?? 0) / 1000.0
                    };

                    if (string.IsNullOrWhiteSpace(_ramType) || _ramType == "Unknown")
                        _ramType = module.MemoryType;
                    if (_ramFreqMHz == 0)
                        _ramFreqMHz = module.SpeedMHz;

                    _ramModules.Add(module);
                    _logger.Info($"RAM Module: {module.Slot} | {module.Manufacturer} {module.PartNumber} " +
                                 $"| {module.CapacityGB:F0} GB | {module.SpeedMHz} MHz | {module.MemoryType}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"WMI RAM info failed: {ex.Message}");
            }
        }

        // ── 3. Статика MB (WMI) ───────────────────────────────────
        private void InitStaticMbInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    _mbManufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "";
                    _mbProduct = obj["Product"]?.ToString()?.Trim() ?? "";
                    break;
                }
            }
            catch { }

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    _mbBiosVersion = obj["SMBIOSBIOSVersion"]?.ToString() ?? "";
                    _mbBiosDate = obj["ReleaseDate"] is string d && d.Length >= 8
                        ? $"{d.Substring(0, 4)}-{d.Substring(4, 2)}-{d.Substring(6, 2)}"
                        : "";
                    break;
                }
            }
            catch { }

            _logger.Info($"MB: {_mbManufacturer} {_mbProduct}, BIOS: {_mbBiosVersion} ({_mbBiosDate})");
        }

        // ── 4. Карта буква→модель диска (WMI) ─────────────────────
        private void InitDriveLetterMaps()
        {
            try
            {
                // Win32_DiskDrive → Win32_DiskDriveToDiskPartition → Win32_LogicalDiskToPartition
                // Используем SELECT * — явное перечисление полей с SpindleSpeed/MediaType
                // вызывает "Недопустимый запрос" на ряде конфигураций Windows/драйверов.
                using var driveSrch = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject drive in driveSrch.Get())
                {
                    string model = drive["Model"]?.ToString() ?? "Unknown";
                    string serial = drive["SerialNumber"]?.ToString()?.Trim() ?? "";
                    string bus = drive["InterfaceType"]?.ToString() ?? "";

                    // Определяем тип шины точнее
                    if (model.Contains("NVMe") || bus.Contains("NVMe")) bus = "NVMe";
                    else if (bus == "IDE") bus = "SATA";

                    // MediaType и SpindleSpeed — читаем защищённо: поле может отсутствовать
                    // у виртуальных дисков, USB-адаптеров или при нехватке прав.
                    string mediaType = "";
                    uint? rotationRate = null;
                    try { mediaType = drive["MediaType"]?.ToString() ?? ""; } catch { }
                    try
                    {
                        var rr = drive["SpindleSpeed"];
                        if (rr != null) rotationRate = Convert.ToUInt32(rr);
                    }
                    catch { }

                    using var partSrch = drive.GetRelated("Win32_DiskPartition");
                    foreach (ManagementObject partition in partSrch)
                    {
                        using var logSrch = partition.GetRelated("Win32_LogicalDisk");
                        foreach (ManagementObject logical in logSrch)
                        {
                            string letter = logical["DeviceID"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(letter))
                            {
                                _driveLetterToModel[letter] = model;
                                _driveLetterToSerial[letter] = serial;
                                _driveLetterToBus[letter] = bus;
                                _driveLetterToMediaType[letter] = mediaType;
                                _driveLetterToRotationRate[letter] = rotationRate;

                                string fs = logical["FileSystem"]?.ToString() ?? "";
                                _driveLetterToFS[letter] = fs;

                                _logger.Info($"Drive map: {letter} → {model} ({bus}) MediaType={mediaType} RPM={rotationRate}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Drive letter map failed: {ex.Message}");
            }
        }

        // ── 5. LHM (LibreHardwareMonitor) ─────────────────────────
        private void InitLhm()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true,
                IsNetworkEnabled = true,
                IsControllerEnabled = false
            };

            bool admin = IsAdminUtils.IsAdmin();
            _logger.Info($"Admin rights: {admin}");

            _computer.Open();
            _logger.Info($"LHM hardware count: {_computer.Hardware.Count}");

            foreach (var hw in _computer.Hardware)
            {
                hw.Update();
                foreach (var sub in hw.SubHardware) sub.Update();

                _logger.Info($"Hardware: [{hw.HardwareType}] {hw.Name}");

                switch (hw.HardwareType)
                {
                    case HardwareType.Cpu:
                        InitCpuSensors(hw);
                        break;
                    case HardwareType.GpuNvidia:
                    case HardwareType.GpuAmd:
                    case HardwareType.GpuIntel:
                        InitGpuSensors(hw);
                        break;
                    case HardwareType.Motherboard:
                        InitMbSensors(hw);
                        break;
                    case HardwareType.Storage:
                        InitStorageSensors(hw);
                        break;
                }
            }
        }

        private void InitCpuSensors(IHardware hw)
        {
            _cpuHardware = hw;

            // Обходим SubHardware (AMD SMU, Intel MSR)
            var allSensors = hw.Sensors.ToList();
            foreach (var sub in hw.SubHardware)
                allSensors.AddRange(sub.Sensors);

            foreach (var s in allSensors)
            {
                _logger.Debug($"  CPU Sensor: {s.SensorType,-14} '{s.Name}' = {s.Value}");

                switch (s.SensorType)
                {
                    // Нагрузка
                    case SensorType.Load:
                        if (s.Name.Contains("Total") || s.Name.Contains("Package"))
                        {
                            if (_cpuLoadTotal == null) _cpuLoadTotal = s;
                        }
                        else if (s.Name.StartsWith("CPU Core #") || s.Name.StartsWith("Core #"))
                        {
                            _cpuCoreLoads.Add(s);
                        }
                        break;

                    // Температура — приоритеты по вендору
                    case SensorType.Temperature:
                        if (s.Name == "Core (Tctl/Tdie)" || s.Name == "Tdie" || s.Name == "CPU Package")
                            _cpuTempPackage = s;   // AMD: Tdie; Intel: Package
                        else if (s.Name.Contains("Package") && _cpuTempPackage == null)
                            _cpuTempPackage = s;
                        else if ((s.Name.StartsWith("CPU Core #") || s.Name.StartsWith("Core #")) &&
                                 !s.Name.Contains("Distance"))
                            _cpuCoreTemps.Add(s);
                        break;

                    // Частота ядер
                    case SensorType.Clock:
                        if (s.Name.StartsWith("CPU Core #") || s.Name.StartsWith("Core #"))
                            _cpuCoreClocks.Add(s);
                        break;

                    // Мощность
                    case SensorType.Power:
                        if (s.Name.Contains("Package") && _cpuPowerPackage == null) _cpuPowerPackage = s;
                        else if (s.Name.Contains("Cores") && _cpuPowerCores == null) _cpuPowerCores = s;
                        else if (s.Name.Contains("Memory") && _cpuPowerMemory == null) _cpuPowerMemory = s;
                        break;

                    // Напряжение
                    case SensorType.Voltage:
                        if ((s.Name.Contains("Core") || s.Name.Contains("VCore")) && _cpuVoltageCore == null)
                            _cpuVoltageCore = s;
                        break;
                }
            }

            // Fallback: если пакетный датчик не найден — берём первый температурный
            if (_cpuTempPackage == null && _cpuCoreTemps.Count > 0)
                _cpuTempPackage = _cpuCoreTemps[0];

            _logger.Info($"CPU sensors — Load: {_cpuLoadTotal?.Name ?? "fallback"}, " +
                         $"Temp: {_cpuTempPackage?.Name ?? "N/A"}, " +
                         $"Clocks: {_cpuCoreClocks.Count}, Cores temps: {_cpuCoreTemps.Count}, " +
                         $"Power: {_cpuPowerPackage?.Name ?? "N/A"}");
        }

        private void InitGpuSensors(IHardware hw)
        {
            var set = new GpuSensorSet { Hardware = hw };
            string vendor = hw.HardwareType == HardwareType.GpuNvidia ? "NVIDIA"
                          : hw.HardwareType == HardwareType.GpuAmd ? "AMD"
                          : "Intel";
            set.Vendor = vendor;

            // Тип GPU — iGPU если Intel, иначе dGPU (можно уточнить через WMI)
            set.GpuType = hw.HardwareType == HardwareType.GpuIntel ? "Integrated" : "Discrete";

            // Читаем версию драйвера через WMI однократно
            set.DriverVersion = GetGpuDriverVersion(hw.Name);

            foreach (var s in hw.Sensors)
            {
                _logger.Debug($"  GPU Sensor: {s.SensorType,-14} '{s.Name}' = {s.Value}");

                switch (s.SensorType)
                {
                    case SensorType.Load:
                        if (s.Name.Contains("Core")) set.CoreLoad = s;
                        else if (s.Name.Contains("Memory")) set.MemoryLoad = s;
                        else if (s.Name.Contains("Video")) set.VideoEngLoad = s;
                        else if (s.Name.Contains("D3D")) set.D3DLoad = s;
                        break;

                    case SensorType.Temperature:
                        if (s.Name.Contains("Core") || s.Name == "GPU Temperature")
                            set.TempCore = s;
                        else if (s.Name.Contains("Hot Spot") || s.Name.Contains("Junction"))
                            set.TempHotSpot = s;
                        else if (s.Name.Contains("Memory"))
                            set.TempMemory = s;
                        break;

                    case SensorType.Clock:
                        if (s.Name.Contains("Core")) set.ClockCore = s;
                        else if (s.Name.Contains("Memory")) set.ClockMemory = s;
                        else if (s.Name.Contains("Shader")) set.ClockShader = s;
                        break;

                    case SensorType.Fan:
                        set.Fans.Add(s);
                        break;

                    case SensorType.Control:
                        if (s.Name.Contains("Fan")) set.FanPercent = s;
                        break;

                    case SensorType.Power:
                        if (set.Power == null) set.Power = s;
                        break;

                    case SensorType.Voltage:
                        if (s.Name.Contains("Core")) set.VoltageCore = s;
                        break;

                    case SensorType.SmallData:
                        if (s.Name.Contains("Used")) set.MemoryUsed = s;
                        if (s.Name.Contains("Total")) set.MemoryTotal = s;
                        if (s.Name.Contains("Free")) set.MemoryFree = s;
                        break;
                }
            }

            // Получаем TotalMemory из реестра/WMI если LHM не отдал
            if (set.MemoryTotal == null)
            {
                set.MemoryTotalMBFallback = GetGpuMemoryFromRegistry() > 0
                    ? GetGpuMemoryFromRegistry()
                    : GetGpuMemoryFromWmi(hw.Name);
            }

            _gpuSensors.Add(set);
            _logger.Info($"GPU '{hw.Name}': Load={set.CoreLoad?.Name ?? "N/A"}, " +
                         $"Temp={set.TempCore?.Name ?? "N/A"}, " +
                         $"Mem={set.MemoryUsed?.Name ?? "N/A"}/{set.MemoryTotal?.Name ?? "fallback"}, " +
                         $"Fans={set.Fans.Count}, Power={set.Power?.Name ?? "N/A"}");
        }

        private void InitMbSensors(IHardware hw)
        {
            _mbHardware = hw;
            // SuperIO находится в SubHardware
            foreach (var sub in hw.SubHardware)
            {
                sub.Update();
                foreach (var s in sub.Sensors)
                {
                    _mbSensors.Add(s);
                    _logger.Debug($"  MB Sensor: {s.SensorType,-14} '{s.Name}' = {s.Value}");
                }
            }
            _logger.Info($"MB sensors found: {_mbSensors.Count}");
        }

        private void InitStorageSensors(IHardware hw)
        {
            var set = new StorageSensorSet { Hardware = hw, ModelName = hw.Name };

            foreach (var s in hw.Sensors)
            {
                _logger.Debug($"  Storage Sensor: {s.SensorType,-14} '{s.Name}' = {s.Value}");

                switch (s.SensorType)
                {
                    case SensorType.Temperature:
                        if (set.Temperature == null) set.Temperature = s;
                        break;
                    case SensorType.Load:
                        if (s.Name.Contains("Used Space") || s.Name.Contains("Usage"))
                            set.UsageLoad = s;
                        break;
                    case SensorType.Data:
                        if (s.Name.Contains("Read")) set.TotalRead = s;
                        if (s.Name.Contains("Write")) set.TotalWrite = s;
                        break;
                    case SensorType.SmallData:
                        if (s.Name.Contains("Read Rate")) set.ReadRate = s;
                        if (s.Name.Contains("Write Rate")) set.WriteRate = s;
                        break;
                    case SensorType.Level:
                        if (s.Name.Contains("Remaining Life") || s.Name.Contains("Wear"))
                            set.HealthLevel = s;
                        break;
                        /*
                    case SensorType.RawValue:
                        if (s.Name.Contains("Power On Hours")) set.PowerOnHours = s;
                        if (s.Name.Contains("Power Cycle")) set.PowerCycles = s;
                        if (s.Name.Contains("Read Error")) set.ReadErrors = s;
                        break;
                        */
                }
            }

            _storageSensors[hw.Name] = set;
            _logger.Info($"Storage '{hw.Name}': Temp={set.Temperature?.Name ?? "N/A"}, " +
                         $"Health={set.HealthLevel?.Name ?? "N/A"}, " +
                         $"ReadRate={set.ReadRate?.Name ?? "N/A"}");
        }

        // ── 6. PerformanceCounter для дисков ──────────────────────
        private void InitDiskPerfCounters()
        {
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed);
                foreach (var d in drives)
                {
                    // d.Name = "C:\", TrimEnd даёт "C:" — именно этот формат нужен PerformanceCounter.
                    // Ранее было letter + ":" что давало "C::" — отсюда ошибка "Instance does not exist".
                    string letter = d.Name.TrimEnd('\\', '/'); // "C:"
                    string perfLetter = letter.TrimEnd(':');   // "C" — для ключа словаря без двоеточия
                    try
                    {
                        var pc = new DiskPerfCounters
                        {
                            ReadCounter = new PerformanceCounter("LogicalDisk", "Disk Read Bytes/sec", letter),
                            WriteCounter = new PerformanceCounter("LogicalDisk", "Disk Write Bytes/sec", letter),
                            ActiveCounter = new PerformanceCounter("LogicalDisk", "% Disk Time", letter)
                        };
                        pc.ReadCounter.NextValue();
                        pc.WriteCounter.NextValue();
                        pc.ActiveCounter.NextValue();
                        // Ключ словаря — без двоеточия, т.к. _driveLetterToModel тоже хранит "C:" из DeviceID
                        _diskPerfCounters[letter] = pc;
                        _logger.Debug($"DiskPerfCounter {letter} OK");
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"DiskPerfCounter {letter} init failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"DiskPerfCounters init: {ex.Message}");
            }
        }

        // ── 7. Сетевые счётчики ───────────────────────────────────
        private void InitNetCounters()
        {
            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                n.OperationalStatus == OperationalStatus.Up);

                foreach (var nic in nics)
                {
                    string name = nic.Name;
                    try
                    {
                        var nc = new NetCounters
                        {
                            NicName = name,
                            Description = nic.Description,
                            SpeedMbps = nic.Speed / 1_000_000.0,
                            Mac = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes(), 0),
                            UpCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name),
                            DownCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", name)
                        };
                        nc.UpCounter.NextValue();
                        nc.DownCounter.NextValue();
                        _netCounters[name] = nc;
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"NetCounter '{name}' init: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"NetCounters init: {ex.Message}");
            }
        }

        #endregion

        // ================================================================
        #region Мониторинг (цикл)

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
                    var info = CollectSystemInfo();
                    DataUpdated?.Invoke(this, info);
                    await Task.Delay(PollingIntervalMs, token);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.Error($"MonitorLoop error: {ex.Message}");
                    await Task.Delay(PollingIntervalMs);
                }
            }
        }

        #endregion

        // ================================================================
        #region Сборка SystemInfo

        private SystemInfo CollectSystemInfo()
        {
            var info = new SystemInfo
            {
                Cpu = CollectCpuInfo(),
                Ram = CollectRamInfo(),
                Motherboard = CollectMotherboardInfo(),
                Disks = CollectDiskInfo(),
                LastUpdate = DateTime.Now
            };

            foreach (var gpu in CollectGpuInfos())
                info.Gpus.Add(gpu);

            foreach (var net in CollectNetworkInfo())
                info.NetworkAdapters.Add(net);

            return info;
        }

        // ── CPU ────────────────────────────────────────────────────
        private CpuInfo CollectCpuInfo()
        {
            var cpu = new CpuInfo
            {
                Name = _cpuName ?? "Unknown",
                Vendor = _cpuVendor,
                PhysicalCores = _cpuPhysicalCores,
                LogicalCores = _cpuLogicalCores,
                BaseClockMHz = _cpuBaseClockMHz,
                MaxClockMHz = _cpuMaxClockMHz
            };

            // Обновляем LHM
            _cpuHardware?.Update();
            foreach (var sub in _cpuHardware?.SubHardware ?? Enumerable.Empty<IHardware>())
                sub.Update();

            // Нагрузка суммарная
            if (_cpuLoadTotal != null)
                cpu.TotalLoad = _cpuLoadTotal.Value ?? 0;
            else if (_cpuFallbackCounter != null)
                cpu.TotalLoad = _cpuFallbackCounter.NextValue();

            // Нагрузка по ядрам
            foreach (var s in _cpuCoreLoads)
                cpu.CoreLoads.Add(s.Value ?? 0);

            // Температура пакета
            if (_cpuTempPackage != null)
                cpu.Temperature = _cpuTempPackage.Value ?? 0;

            // Температуры ядер
            foreach (var s in _cpuCoreTemps)
                cpu.CoreTemperatures.Add(s.Value ?? 0);

            // Частоты ядер
            double freqSum = 0;
            foreach (var s in _cpuCoreClocks)
            {
                double mhz = s.Value ?? 0;
                cpu.CoreClocks.Add(new CoreClockInfo
                {
                    CoreName = s.Name,
                    FrequencyMHz = mhz
                });
                freqSum += mhz;
            }

            if (_cpuCoreClocks.Count > 0)
            {
                cpu.AverageFrequencyGHz = freqSum / _cpuCoreClocks.Count / 1000.0;
            }
            else
            {
                // WMI fallback
                cpu.AverageFrequencyGHz = _cpuBaseClockMHz / 1000.0;
            }

            // Мощность
            if (_cpuPowerPackage != null) cpu.PackagePower = _cpuPowerPackage.Value ?? 0;
            if (_cpuPowerCores != null) cpu.CoresPower = _cpuPowerCores.Value ?? 0;
            if (_cpuPowerMemory != null) cpu.MemoryPower = _cpuPowerMemory.Value ?? 0;

            // Напряжение
            if (_cpuVoltageCore != null) cpu.VCore = _cpuVoltageCore.Value ?? 0;

            return cpu;
        }

        // ── GPU ────────────────────────────────────────────────────
        private List<GpuInfo> CollectGpuInfos()
        {
            var result = new List<GpuInfo>();

            foreach (var set in _gpuSensors)
            {
                set.Hardware.Update();

                double memTotal = set.MemoryTotal?.Value ?? set.MemoryTotalMBFallback;
                double memUsed = set.MemoryUsed?.Value ?? 0;

                // Если сенсор — Load (%), пересчитываем
                if (set.MemoryUsed?.SensorType == SensorType.Load)
                    memUsed = memTotal * (set.MemoryUsed.Value ?? 0) / 100.0;

                var gpu = new GpuInfo
                {
                    Name = set.Hardware.Name,
                    Vendor = set.Vendor,
                    GpuType = set.GpuType,
                    DriverVersion = set.DriverVersion,

                    CoreLoad = set.CoreLoad?.Value ?? 0,
                    MemoryLoad = set.MemoryLoad?.Value ?? 0,
                    VideoEngineLoad = set.VideoEngLoad?.Value ?? 0,
                    D3DLoad = set.D3DLoad?.Value ?? 0,

                    MemoryTotalMB = memTotal,
                    MemoryUsedMB = memUsed,

                    CoreFrequencyMHz = set.ClockCore?.Value ?? 0,
                    MemoryFrequencyMHz = set.ClockMemory?.Value ?? 0,
                    ShaderFrequencyMHz = set.ClockShader?.Value ?? 0,

                    Temperature = set.TempCore?.Value ?? 0,
                    HotSpotTemperature = set.TempHotSpot?.Value ?? 0,
                    MemoryTemperature = set.TempMemory?.Value ?? 0,

                    FanSpeedRpm = set.Fans.FirstOrDefault()?.Value ?? 0,
                    FanSpeedPercent = set.FanPercent?.Value ?? 0,

                    PowerWatts = set.Power?.Value ?? 0,
                    CoreVoltage = set.VoltageCore?.Value ?? 0
                };

                foreach (var fan in set.Fans)
                    gpu.FanSpeeds.Add(fan.Value ?? 0);

                result.Add(gpu);
            }

            return result;
        }

        // ── RAM ────────────────────────────────────────────────────
        private RamInfo CollectRamInfo()
        {
            var ram = new RamInfo
            {
                Modules = _ramModules,
                FrequencyMHz = _ramFreqMHz,
                MemoryType = _ramType,
                TimingsString = _ramTimings
            };

            try
            {
                var compInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                double total = (double)compInfo.TotalPhysicalMemory / 1024.0 / 1024.0 / 1024.0;
                double avail = (double)compInfo.AvailablePhysicalMemory / 1024.0 / 1024.0 / 1024.0;
                ram.TotalGB = total;
                ram.UsedGB = total - avail;
                ram.LoadPercent = total > 0 ? (ram.UsedGB / total) * 100.0 : 0;
            }
            catch (Exception ex)
            {
                _logger.Debug($"RAM info error: {ex.Message}");
            }

            return ram;
        }

        // ── MB ─────────────────────────────────────────────────────
        private MotherboardInfo CollectMotherboardInfo()
        {
            var mb = new MotherboardInfo
            {
                Manufacturer = _mbManufacturer,
                Product = _mbProduct,
                BiosVersion = _mbBiosVersion,
                BiosDate = _mbBiosDate
            };

            _mbHardware?.Update();
            foreach (var sub in _mbHardware?.SubHardware ?? Enumerable.Empty<IHardware>())
                sub.Update();

            foreach (var s in _mbSensors)
            {
                string unit = s.SensorType switch
                {
                    SensorType.Temperature => "°C",
                    SensorType.Fan => "RPM",
                    SensorType.Voltage => "V",
                    SensorType.Current => "A",
                    SensorType.Power => "W",
                    _ => ""
                };

                mb.Sensors.Add(new MotherboardSensorInfo
                {
                    Name = s.Name,
                    Type = s.SensorType.ToString(),
                    Value = s.Value ?? 0,
                    Unit = unit
                });
            }

            return mb;
        }

        // ── Диски ──────────────────────────────────────────────────
        private List<DiskInfo> CollectDiskInfo()
        {
            var result = new List<DiskInfo>();

            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Fixed && d.IsReady);

                foreach (var drive in drives)
                {
                    string letter = drive.Name.TrimEnd('\\');

                    double total = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                    double free = drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0;
                    double used = total - free;

                    _driveLetterToModel.TryGetValue(letter, out string model);
                    _driveLetterToSerial.TryGetValue(letter, out string serial);
                    _driveLetterToBus.TryGetValue(letter, out string bus);
                    _driveLetterToFS.TryGetValue(letter, out string fs);

                    _driveLetterToMediaType.TryGetValue(letter, out string mediaType);
                    _driveLetterToRotationRate.TryGetValue(letter, out uint? rotationRate);


                    var di = new DiskInfo
                    {
                        Name = letter,
                        Model = model ?? "Unknown",
                        SerialNumber = serial ?? "",
                        BusType = bus ?? "Unknown",
                        FileSystem = fs ?? "",
                        Type = DetermineType(model, bus, mediaType, rotationRate),
                        TotalSizeGB = total,
                        FreeSpaceGB = free,
                        UsedSpaceGB = used,
                        UsagePercent = total > 0 ? (used / total) * 100.0 : 0
                    };

                    // SMART / LHM данные
                    var storageSet = FindStorageSetByModel(model);
                    if (storageSet != null)
                    {
                        storageSet.Hardware.Update();
                        di.Temperature = storageSet.Temperature?.Value ?? 0;
                        di.HealthPercent = storageSet.HealthLevel != null
                            ? (int)(storageSet.HealthLevel.Value ?? -1)
                            : -1;
                        di.TotalReadsGB = storageSet.TotalRead != null
                            ? (ulong)(storageSet.TotalRead.Value ?? 0)
                            : 0;
                        di.TotalWritesGB = storageSet.TotalWrite != null
                            ? (ulong)(storageSet.TotalWrite.Value ?? 0)
                            : 0;
                        di.PowerOnHours = storageSet.PowerOnHours != null
                            ? (int)(storageSet.PowerOnHours.Value ?? 0)
                            : 0;
                        di.PowerCycles = storageSet.PowerCycles != null
                            ? (int)(storageSet.PowerCycles.Value ?? 0)
                            : 0;

                        if (storageSet.ReadRate != null) di.ReadSpeedMBs = (storageSet.ReadRate.Value ?? 0) / 1024.0;
                        if (storageSet.WriteRate != null) di.WriteSpeedMBs = (storageSet.WriteRate.Value ?? 0) / 1024.0;
                    }

                    // PerformanceCounter скорости / активность
                    if (_diskPerfCounters.TryGetValue(letter, out var pc))
                    {
                        // ReadRate/WriteRate из LHM предпочтительнее — не перезаписываем если есть
                        double readBps = pc.ReadCounter.NextValue();
                        double writeBps = pc.WriteCounter.NextValue();
                        if (di.ReadSpeedMBs == 0) di.ReadSpeedMBs = readBps / 1024.0 / 1024.0;
                        if (di.WriteSpeedMBs == 0) di.WriteSpeedMBs = writeBps / 1024.0 / 1024.0;

                        double active = pc.ActiveCounter.NextValue();
                        di.ActiveTimePercent = Math.Min(active, 100.0);
                    }

                    result.Add(di);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"CollectDiskInfo error: {ex.Message}");
            }

            return result;
        }

        // ── Сеть ───────────────────────────────────────────────────
        private List<NetworkAdapterInfo> CollectNetworkInfo()
        {
            var result = new List<NetworkAdapterInfo>();

            foreach (var kvp in _netCounters)
            {
                var nc = kvp.Value;
                try
                {
                    result.Add(new NetworkAdapterInfo
                    {
                        Name = nc.NicName,
                        Description = nc.Description,
                        MacAddress = nc.Mac,
                        SpeedMbps = nc.SpeedMbps,
                        IsConnected = true,
                        UploadKBs = nc.UpCounter.NextValue() / 1024.0,
                        DownloadKBs = nc.DownCounter.NextValue() / 1024.0
                    });
                }
                catch { }
            }

            return result;
        }

        #endregion

        // ================================================================
        #region Вспомогательные методы

        private StorageSensorSet FindStorageSetByModel(string model)
        {
            if (model == null) return null;

            foreach (var kvp in _storageSensors)
            {
                string lhmName = kvp.Key;
                if (lhmName.Contains(model, StringComparison.OrdinalIgnoreCase) ||
                    model.Contains(lhmName, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }
            return _storageSensors.Count > 0 ? _storageSensors.Values.First() : null;
        }

        private string DetermineType(string model, string bus, string mediaType, uint? rotationRate)
        {
            if (bus == "NVMe") return "NVMe";
            if (bus == "USB") return "USB";

            // MSFT_Disk — точный MediaType (3=HDD, 4=SSD), работает без прав администратора.
            // Win32_DiskDrive.MediaType возвращает "Fixed hard disk media" для любого типа — бесполезно.
            string msftType = GetDiskTypeFromMsftDisk(model ?? "");
            if (!string.IsNullOrEmpty(msftType))
                return msftType;

            // SpindleSpeed: 0 или 1 — нет вращения (SSD), >1 — RPM (HDD). Требует прав администратора.
            if (rotationRate.HasValue)
            {
                if (rotationRate.Value == 0 || rotationRate.Value == 1) return "SSD";
                if (rotationRate.Value > 1) return "HDD";
            }

            // Название модели — финальный fallback
            var m = model ?? "";

            if (m.Contains("NVMe", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("PCIe", StringComparison.OrdinalIgnoreCase)) return "NVMe";

            if (m.Contains("SSD", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("Solid", StringComparison.OrdinalIgnoreCase) ||
                // Crucial BX/MX серии
                m.Contains("BX500", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("BX300", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("BX200", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("MX500", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("MX300", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("MX200", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("P3 Plus", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("P5 Plus", StringComparison.OrdinalIgnoreCase) ||
                // Samsung EVO/QVO/PRO
                m.Contains("870 EVO", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("860 EVO", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("850 EVO", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("870 QVO", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("860 QVO", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("970 EVO", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("980 PRO", StringComparison.OrdinalIgnoreCase) ||
                // WD Blue/Green — SSD линейка (в отличие от WD Blue HDD)
                m.Contains("WD Blue SSD", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("WD Green SSD", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("WD_GREEN", StringComparison.OrdinalIgnoreCase) ||
                // Apacer AS серия
                m.Contains("AS350", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("AS340", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("AS330", StringComparison.OrdinalIgnoreCase) ||
                // Kingston A/UV серии
                m.Contains("SA400", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("UV500", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("UV400", StringComparison.OrdinalIgnoreCase) ||
                // Patriot
                m.Contains("Burst Elite", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("P210", StringComparison.OrdinalIgnoreCase) ||
                // ADATA
                m.Contains("SU800", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("SU650", StringComparison.OrdinalIgnoreCase) ||
                // Transcend
                m.Contains("TS480", StringComparison.OrdinalIgnoreCase)) return "SSD";

            if (m.Contains("HDD", StringComparison.OrdinalIgnoreCase) ||
                // Seagate
                m.Contains("Barracuda", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("IronWolf", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("Exos", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("Skyhawk", StringComparison.OrdinalIgnoreCase) ||
                // WD цветные серии HDD (без слова SSD)
                m.Contains("WD Black", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("WD Red", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("WD Purple", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("WD Gold", StringComparison.OrdinalIgnoreCase) ||
                // Toshiba HDD серии
                m.Contains("DT01", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("MQ01", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("MQ04", StringComparison.OrdinalIgnoreCase) ||
                // HGST
                m.Contains("HUS", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("HTS", StringComparison.OrdinalIgnoreCase)) return "HDD";

            return "Unknown";
        }

        /// <summary>
        /// Определяет тип диска через WMI MSFT_Disk (Windows Storage namespace).
        /// Возвращает корректный MediaType (3=HDD, 4=SSD) без прав администратора,
        /// в отличие от Win32_DiskDrive.MediaType который всегда даёт "Fixed hard disk media".
        /// </summary>
        private string GetDiskTypeFromMsftDisk(string model)
        {
            try
            {
                var scope = new ManagementScope(@"\\.\ROOT\Microsoft\Windows\Storage");
                scope.Connect();
                using var searcher = new ManagementObjectSearcher(scope,
                    new ObjectQuery("SELECT FriendlyName, MediaType FROM MSFT_Disk"));

                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["FriendlyName"]?.ToString() ?? "";
                    if (string.IsNullOrEmpty(name)) continue;

                    // Сопоставляем по подстроке в обе стороны
                    if (!name.Contains(model, StringComparison.OrdinalIgnoreCase) &&
                        !model.Contains(name, StringComparison.OrdinalIgnoreCase)) continue;

                    int mt = Convert.ToInt32(obj["MediaType"] ?? 0);
                    string result = mt switch
                    {
                        3 => "HDD",
                        4 => "SSD",
                        5 => "SCM",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(result))
                    {
                        _logger.Debug($"MSFT_Disk '{name}' → MediaType={mt} ({result})");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"MSFT_Disk lookup failed: {ex.Message}");
            }
            return "";
        }

        private (string type, string model, string bus, string mediaType, uint? rotationRate) GetDiskWmiInfo(string deviceId)
        {
            try
            {
                // Win32_DiskDrive даёт MediaType и SpindleSpeed
                var query = $"SELECT Model, MediaType, SpindleSpeed, InterfaceType FROM Win32_DiskDrive WHERE DeviceID='{deviceId.Replace("\\", "\\\\")}'";
                using var searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject obj in searcher.Get())
                {
                    var model = obj["Model"]?.ToString();
                    var mediaType = obj["MediaType"]?.ToString();
                    var bus = obj["InterfaceType"]?.ToString(); // "IDE", "SCSI", "NVMe", "USB"
                    uint? rpm = obj["SpindleSpeed"] is uint u ? u : null;

                    return (DetermineType(model, bus, mediaType, rpm), model, bus, mediaType, rpm);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"WMI disk info failed for {deviceId}: {ex.Message}");
            }
            return ("Unknown", null, null, null, null);
        }

        private string GetGpuDriverVersion(string gpuName)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    if (name.Contains(gpuName, StringComparison.OrdinalIgnoreCase) ||
                        gpuName.Contains(name, StringComparison.OrdinalIgnoreCase))
                        return obj["DriverVersion"]?.ToString() ?? "";
                }
            }
            catch { }
            return "";
        }

        private double GetGpuMemoryFromRegistry()
        {
            try
            {
                using var baseKey = Registry.LocalMachine
                    .OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (baseKey == null) return 0;

                foreach (var name in baseKey.GetSubKeyNames())
                {
                    if (name == "Properties") continue;
                    using var sub = baseKey.OpenSubKey(name);
                    var val = sub?.GetValue("HardwareInformation.qwMemorySize");
                    if (val == null) continue;

                    ulong bytes = val switch
                    {
                        // Большинство драйверов (NVIDIA, AMD новые)
                        long l => (ulong)l,
                        ulong u => u,
                        int i => (ulong)i,
                        uint ui => (ulong)ui,
                        // Некоторые драйверы пишут как byte[8] (little-endian)
                        byte[] b when b.Length >= 8 => BitConverter.ToUInt64(b, 0),
                        byte[] b when b.Length == 4 => BitConverter.ToUInt32(b, 0),
                        _ => Convert.ToUInt64(val)
                    };

                    if (bytes > 0)
                    {
                        double mb = bytes / 1024.0 / 1024.0;
                        _logger.Info($"GPU Memory from registry: {bytes} bytes = {mb} MB");
                        return mb; // Возвращаем MB
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Registry GPU memory failed: {ex.Message}");
            }
            return 0;
        }

        private double GetGpuMemoryFromWmi(string gpuName)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    if (!name.Contains(gpuName, StringComparison.OrdinalIgnoreCase)) continue;
                    var ram = obj["AdapterRAM"];
                    if (ram != null) return Convert.ToDouble(ram) / 1024.0 / 1024.0;
                }
            }
            catch { }
            return 0;
        }

        private static string DecodeRamType(int code) => code switch
        {
            0x12 => "DDR3",   // 18
            0x13 => "DDR3",   // 19
            0x18 => "DDR4",   // 24
            0x1A => "DDR4",   // 26
            0x1B => "LPDDR3", // 27
            0x1C => "LPDDR4", // 28
            0x1E => "DDR5",   // 30
            0x22 => "DDR5",   // 34
            0x24 => "LPDDR4", // 36
            0x26 => "LPDDR5", // 38
            _ => $"Unknown (0x{code:X2})"
        };

        private static string DecodeRamFormFactor(int code) => code switch
        {
            8 => "DIMM",
            12 => "SODIMM",
            13 => "RIMM",
            _ => "Unknown"
        };

        #endregion

        // ================================================================
        #region Dispose

        public void Dispose()
        {
            StopMonitoring();
            _cpuFallbackCounter?.Dispose();
            _cts?.Dispose();

            foreach (var kvp in _diskPerfCounters)
            {
                kvp.Value.ReadCounter?.Dispose();
                kvp.Value.WriteCounter?.Dispose();
                kvp.Value.ActiveCounter?.Dispose();
            }

            foreach (var kvp in _netCounters)
            {
                kvp.Value.UpCounter?.Dispose();
                kvp.Value.DownCounter?.Dispose();
            }

            _computer?.Close();
        }

        #endregion

        // ================================================================
        #region Вспомогательные классы (private)

        private class GpuSensorSet
        {
            public IHardware Hardware;
            public string Vendor;
            public string GpuType;
            public string DriverVersion;
            public ISensor CoreLoad, MemoryLoad, VideoEngLoad, D3DLoad;
            public ISensor TempCore, TempHotSpot, TempMemory;
            public ISensor ClockCore, ClockMemory, ClockShader;
            public List<ISensor> Fans = new();
            public ISensor FanPercent;
            public ISensor Power;
            public ISensor VoltageCore;
            public ISensor MemoryUsed, MemoryTotal, MemoryFree;
            public double MemoryTotalMBFallback;
        }

        private class StorageSensorSet
        {
            public IHardware Hardware;
            public string ModelName;
            public ISensor Temperature;
            public ISensor UsageLoad;
            public ISensor TotalRead, TotalWrite;
            public ISensor ReadRate, WriteRate;
            public ISensor HealthLevel;
            public ISensor PowerOnHours, PowerCycles;
            public ISensor ReadErrors;
        }

        private class DiskPerfCounters
        {
            public PerformanceCounter ReadCounter;
            public PerformanceCounter WriteCounter;
            public PerformanceCounter ActiveCounter;
        }

        private class NetCounters
        {
            public string NicName;
            public string Description;
            public string Mac;
            public double SpeedMbps;
            public PerformanceCounter UpCounter;
            public PerformanceCounter DownCounter;
        }

        #endregion
    }
}