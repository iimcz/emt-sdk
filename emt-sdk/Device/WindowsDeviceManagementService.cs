using System;
using System.Diagnostics;

namespace emt_sdk.Device
{
    public class WindowsDeviceManagementService : IDeviceManagementService
    {
        public void Reboot()
        {
            Process.Start("shutdown", "/r /t 0 /f");
        }

        public void Shutdown()
        {
            Process.Start("shutdown", "/s /t 0 /f");
        }
    }
}

