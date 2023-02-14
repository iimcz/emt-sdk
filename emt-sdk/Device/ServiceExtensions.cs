using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

namespace emt_sdk.Device
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> adding device management.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds management of OS functions like shutdown and reboot.
        /// </summary>
        /// <param name="services">Service container</param>
        public static void AddOSManagament(this IServiceCollection services)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddTransient<IDeviceManagementService, WindowsDeviceManagementService>();
            }
            else
            {
                services.AddTransient<IDeviceManagementService, LinuxDeviceManagementService>();
            }
        }
    }
}
