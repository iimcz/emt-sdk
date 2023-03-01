using emt_sdk.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace emt_sdk.Packages
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> adding package managing.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds local cached package handling.
        /// </summary>
        /// <param name="services">Service container</param>
        public static void AddLocalPackages(this IServiceCollection services)
        {
            services.AddScoped<PackageLoader>();
            services.AddScoped<Naki3D.Common.Protocol.PackageService.PackageServiceBase, PackageService>();
        }

        public static void AddPackageRunnerProxy(this IServiceCollection services)
        {
            services.AddSingleton<IPackageRunner, PackageRunnerProxy>();
        }

        public static void AddSyncConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<IConfigurationProvider<PackageDescriptor>, PackageDescriptorProvider>();
        }
    }
}
