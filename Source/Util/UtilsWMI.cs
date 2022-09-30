using System;
using System.Management;

namespace GateFailoverService.Source.Util
{
    class UtilsWMI
    {
        public static ulong GetUnusedRAM()
        {
            ulong freeRam = 0;
            ManagementObjectSearcher ramMonitor = 
                new ManagementObjectSearcher("SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (ManagementObject objram in ramMonitor.Get())
            {
                freeRam = Convert.ToUInt64(objram["FreePhysicalMemory"]);
            }
            return freeRam;
        }
    }
}
