using emt_sdk.Settings.EMT;
using emt_sdk.Settings.IPW;
using emt_sdk.Settings.PGE;
using Microsoft.Extensions.DependencyInjection;

namespace emt_sdk.Settings
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> adding EMT related configuration providers.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds a <see cref="IConfigurationProvider"/> for all EMT related devices and this library itself backed by local JSON files.
        /// </summary>
        /// <param name="services">Service container</param>
        public static void AddJsonFileSettings(this IServiceCollection services)
        {
            services.AddSingleton<IConfigurationProvider<EMTSetting>, EMTConfigurationProvider>();
            services.AddSingleton<IConfigurationProvider<IPWSetting>, IPWConfigurationProvider>();
            services.AddSingleton<IConfigurationProvider<PGESetting>, PGEConfigurationProvider>();
        }
    }
}
