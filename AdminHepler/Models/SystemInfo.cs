using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminHelper.Models
{
    public class SystemInfo
    {
        // CPU
        public double CpuLoad { get; set; }
        public double CpuTemperature { get; set; }
        public double CpuFrequency { get; set; } // MHz
        public double CpuPower { get; set; } // Watts

        // GPU
        public double GpuLoad { get; set; }
        public double GpuTemperature { get; set; }
        public double GpuMemoryUsed { get; set; } // MB
        public double GpuMemoryTotal { get; set; } // MB
        public double GpuFrequency { get; set; } // MHz
        public double GpuFanSpeed { get; set; } // RPM

        // RAM
        public double RamUsed { get; set; } // GB
        public double RamTotal { get; set; } // GB
        public double RamLoad { get; set; } // %

        // Disk (список дисков)
        public List<DiskInfo> Disks { get; set; } = new List<DiskInfo>();

        // Время обновления
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }

    public class DiskInfo
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public double TotalSize { get; set; } // GB
        public double FreeSpace { get; set; } // GB
        public double UsedSpace { get; set; } // GB
        public double UsagePercent { get; set; }
        public double Temperature { get; set; } // °C
        public string Type { get; set; } // HDD/SSD/NVMe
    }
}
