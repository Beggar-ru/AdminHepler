using System;
using System.Collections.Generic;

namespace AdminHelper.Models
{
    public class SystemInfo
    {
        // ── CPU
        public CpuInfo Cpu { get; set; } = new CpuInfo();

        // ── GPU (список: iGPU + dGPU) ────────────────────────────
        public List<GpuInfo> Gpus { get; set; } = new List<GpuInfo>();

        // ── RAM ───────────────────────────────────────────────────
        public RamInfo Ram { get; set; } = new RamInfo();

        // ── Диски ─────────────────────────────────────────────────
        public List<DiskInfo> Disks { get; set; } = new List<DiskInfo>();

        // ── Материнская плата ─────────────────────────────────────
        public MotherboardInfo Motherboard { get; set; } = new MotherboardInfo();

        // ── Сеть ──────────────────────────────────────────────────
        public List<NetworkAdapterInfo> NetworkAdapters { get; set; } = new List<NetworkAdapterInfo>();

        // ── Метаданные ────────────────────────────────────────────
        public DateTime LastUpdate { get; set; } = DateTime.Now;

        // Обратная совместимость — плоские свойства для UI
        // (делегируют к вложенным объектам)
        public double CpuLoad => Cpu.TotalLoad;
        public double CpuTemperature => Cpu.Temperature;
        public double CpuFrequency => Cpu.AverageFrequencyGHz;
        public double CpuPower => Cpu.PackagePower;

        public double GpuLoad => Gpus.Count > 0 ? Gpus[0].CoreLoad : 0;
        public double GpuTemperature => Gpus.Count > 0 ? Gpus[0].Temperature : 0;
        public double GpuMemoryUsed => Gpus.Count > 0 ? Gpus[0].MemoryUsedMB : 0;
        public double GpuMemoryTotal => Gpus.Count > 0 ? Gpus[0].MemoryTotalMB : 0;
        public double GpuFrequency => Gpus.Count > 0 ? Gpus[0].CoreFrequencyMHz : 0;
        public double GpuFanSpeed => Gpus.Count > 0 ? Gpus[0].FanSpeedRpm : 0;

        public double RamUsed => Ram.UsedGB;
        public double RamTotal => Ram.TotalGB;
        public double RamLoad => Ram.LoadPercent;
    }

    // ============================================================
    //  CPU
    // ============================================================
    public class CpuInfo
    {
        // Идентификация
        public string Name { get; set; } = "Unknown";
        public string Vendor { get; set; } = "Unknown";   // "AMD" / "Intel"
        public int PhysicalCores { get; set; }
        public int LogicalCores { get; set; }
        public double BaseClockMHz { get; set; }   // из WMI/CPUID
        public double MaxClockMHz { get; set; }   // из WMI MaxClockSpeed

        // Нагрузка
        public double TotalLoad { get; set; }    // %
        public List<double> CoreLoads { get; set; } = new List<double>();  // % на ядро

        // Температуры
        public double Temperature { get; set; }    // пакет / Tdie / Tctl
        public List<double> CoreTemperatures { get; set; } = new List<double>();

        // Частоты (текущие)
        public double AverageFrequencyGHz { get; set; }
        public List<CoreClockInfo> CoreClocks { get; set; } = new List<CoreClockInfo>();

        // Мощность
        public double PackagePower { get; set; }   // Вт
        public double CoresPower { get; set; }   // Вт
        public double MemoryPower { get; set; }   // Вт (если доступно)

        // Voltage (если LHM отдаёт)
        public double VCore { get; set; }          // В
    }

    public class CoreClockInfo
    {
        public int CoreIndex { get; set; }
        public string CoreName { get; set; } = "";
        public double FrequencyMHz { get; set; }
    }

    // ============================================================
    //  GPU
    // ============================================================
    public class GpuInfo
    {
        // Идентификация
        public string Name { get; set; } = "Unknown";
        public string Vendor { get; set; } = "Unknown";   // "NVIDIA" / "AMD" / "Intel"
        public string GpuType { get; set; } = "Unknown";   // "Discrete" / "Integrated"
        public string DriverVersion { get; set; } = "";

        // Нагрузка
        public double CoreLoad { get; set; }   // %
        public double MemoryLoad { get; set; }   // %
        public double VideoEngineLoad { get; set; }   // % (NVIDIA — Video Engine)
        public double D3DLoad { get; set; }   // % (Windows DXGI)

        // Память
        public double MemoryUsedMB { get; set; }
        public double MemoryTotalMB { get; set; }
        public double MemoryFreeMB => MemoryTotalMB - MemoryUsedMB;

        // Частоты
        public double CoreFrequencyMHz { get; set; }
        public double MemoryFrequencyMHz { get; set; }
        public double ShaderFrequencyMHz { get; set; }

        // Температуры
        public double Temperature { get; set; }   // GPU Core
        public double HotSpotTemperature { get; set; }   // Hot Spot / Junction (NVIDIA Turing+)
        public double MemoryTemperature { get; set; }   // GDDR6X Junction (если есть)

        // Охлаждение
        public double FanSpeedRpm { get; set; }
        public double FanSpeedPercent { get; set; }
        public List<double> FanSpeeds { get; set; } = new List<double>();  // несколько вентиляторов

        // Мощность
        public double PowerWatts { get; set; }
        public double PowerLimit { get; set; }   // Вт (TDP / Power Limit)

        // Voltage
        public double CoreVoltage { get; set; }  // В
    }

    // ============================================================
    //  RAM
    // ============================================================
    public class RamInfo
    {
        // Общее
        public double TotalGB { get; set; }
        public double UsedGB { get; set; }
        public double FreeGB => TotalGB - UsedGB;
        public double LoadPercent { get; set; }

        // Идентификация (из WMI Win32_PhysicalMemory)
        public List<RamModuleInfo> Modules { get; set; } = new List<RamModuleInfo>();

        // Частота (из LHM или WMI)
        public double FrequencyMHz { get; set; }    // реальная рабочая частота
        public double TimingCL { get; set; }    // CAS Latency
        public string TimingsString { get; set; } = "";   // "CL16-18-18-38" и т.п.

        // Тип памяти
        public string MemoryType { get; set; } = "";  // "DDR4" / "DDR5" / "LPDDR5"
    }

    public class RamModuleInfo
    {
        public string Slot { get; set; } = "";   // "ChannelA-DIMM0"
        public string Manufacturer { get; set; } = "";
        public string PartNumber { get; set; } = "";
        public double CapacityGB { get; set; }
        public double SpeedMHz { get; set; }
        public string FormFactor { get; set; } = "";   // "DIMM" / "SODIMM"
        public string MemoryType { get; set; } = "";
        public double VoltageV { get; set; }
    }

    // ============================================================
    //  DISK
    // ============================================================
    public class DiskInfo
    {
        // Идентификация
        public string Name { get; set; } = "";          // буква тома "C:"
        public string VolumeLabel { get; set; } = "";   // метка тома (например "System", "Data")
        public string Model { get; set; } = "";          // полное имя диска: "CT480BX500SSD1"
        public string SerialNumber { get; set; } = "";
        public string FirmwareRev { get; set; } = "";
        public string BusType { get; set; } = "";        // "NVMe" / "SATA" / "USB" / "SCSI"
        public string FileSystem { get; set; } = "";     // "NTFS" / "exFAT"

        // Ёмкость
        public double TotalSizeGB { get; set; }
        public double FreeSpaceGB { get; set; }
        public double UsedSpaceGB { get; set; }
        public double UsagePercent { get; set; }

        // Состояние (из LHM SMART)
        public double Temperature { get; set; }          // °C
        public int HealthPercent { get; set; } = -1;     // % (-1 = нет данных)
        public ulong ReadErrorsRaw { get; set; }
        public ulong TotalReadsGB { get; set; }
        public ulong TotalWritesGB { get; set; }
        public int PowerOnHours { get; set; }            // часов работы
        public int PowerCycles { get; set; }             // кол-во включений

        // Производительность (текущая)
        public double ReadSpeedMBs { get; set; }
        public double WriteSpeedMBs { get; set; }
        public double ActiveTimePercent { get; set; }    // % занятости

        // Устаревшие свойства (обратная совместимость)
        public double TotalSize { get => TotalSizeGB; set => TotalSizeGB = value; }
        public double FreeSpace { get => FreeSpaceGB; set => FreeSpaceGB = value; }
        public double UsedSpace { get => UsedSpaceGB; set => UsedSpaceGB = value; }
    }

    // ============================================================
    //  MOTHERBOARD
    // ============================================================
    public class MotherboardInfo
    {
        public string Manufacturer { get; set; } = "";
        public string Product { get; set; } = "";   // модель платы
        public string BiosVersion { get; set; } = "";
        public string BiosDate { get; set; } = "";

        // Датчики материнской платы (SuperIO / EC)
        public List<MotherboardSensorInfo> Sensors { get; set; } = new List<MotherboardSensorInfo>();
    }

    public class MotherboardSensorInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";   // "Temperature" / "Fan" / "Voltage" / "Current"
        public double Value { get; set; }
        public string Unit { get; set; } = "";   // "°C" / "RPM" / "V" / "A"
    }

    // ============================================================
    //  NETWORK
    // ============================================================
    public class NetworkAdapterInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string MacAddress { get; set; } = "";
        public bool IsConnected { get; set; }
        public double UploadKBs { get; set; }    // КБ/с
        public double DownloadKBs { get; set; }    // КБ/с
        public double SpeedMbps { get; set; }    // скорость линка
    }
}