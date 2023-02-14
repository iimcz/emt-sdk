using emt_sdk.Communication.Discovery;
using emt_sdk.Communication.ProjectorControl;
using Microsoft.Extensions.DependencyInjection;

namespace emt_sdk.Communication
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> adding communication channels.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds a <see cref="IDiscoveryService"/> broadcasting over UDP.
        /// </summary>
        /// <param name="services">Service container</param>
        public static void AddUDPDiscovery(this IServiceCollection services)
        {
            services.AddSingleton<IDiscoveryService, UDPDiscoveryService>();
        }

        /// <summary>
        /// Adds a <see cref="IProjectorControl"/> using protobuf defined messages sent to sensors.
        /// </summary>
        /// <param name="services"></param>
        public static void AddSensorProjectorControl(this IServiceCollection services)
        {
            services.AddTransient<IProjectorControl, SensorProjectorControl>();
        }

        /// <summary>
        /// Adds Grpc based exhibit connection services.
        /// </summary>
        /// <param name="services"></param>
        public static void AddGrpcExhibitConnection(this IServiceCollection services)
        {
            services.AddScoped<ConnectionService.ConnectionServiceBase, emt_sdk.Communication.Exhibit.ConnectionService>();
            // TODO: Thing
        }
    }
}
