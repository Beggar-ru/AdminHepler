using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminHepler.Models
{
    public class SystemInfo
    {
        public double CpuUsage { get; set; }
        public double GpuUsage { get; set; }
        public double RamUsage { get; set; }
        public double RamTotal { get; set; }
        public double DiskUsage { get; set; }
        public double DiskFree { get; set; }

        public string RamUsageText => $"{RamUsage:F1} / {RamTotal:F1} GB";
        public string DiskUsageText => $"{DiskUsage:F1}% свободно: {DiskFree:F1} GB";
    }
}
