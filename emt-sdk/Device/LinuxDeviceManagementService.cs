using System;
using System.Diagnostics;

namespace emt_sdk.Device
{
    public class LinuxDeviceManagementService : IDeviceManagementService
    {
        public void Reboot()
        {
            Process.Start("/sbin/shutdown", "-r now");
        }

        public void Shutdown()
        {
            Process.Start("/sbin/shutdown", "now");
        }
    }
}

