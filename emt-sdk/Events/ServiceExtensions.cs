using emt_sdk.Communication.Discovery;
using emt_sdk.Communication.ProjectorControl;
using emt_sdk.Events.Local;
using emt_sdk.Events.NtpSync;
using emt_sdk.Events.Relay;
using emt_sdk.Events.Remote;
using Microsoft.Extensions.DependencyInjection;

namespace emt_sdk.Events
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> adding communication channels.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds a <see cref="NtpScheduler"/> for synchronizing events across multiple devices.
        /// </summary>
        /// <param name="services">Service container</param>
        public static void AddNTPSynchronization(this IServiceCollection services)
        {
            services.AddScoped<NtpScheduler>();
        }

        /// <summary>
        /// Adds a <see cref="EventManager"/> and all its dependencies for managing events, sensors and 3D scene relays.
        /// </summary>
        /// <param name="services">Service container</param>
        public static void AddEvents(this IServiceCollection services)
        {
            // Sensor
            services.AddSingleton<ISensorManager, ProtobufTcpSensorManager>();

            // Relay
            services.AddSingleton<EventRelayClient>();
            services.AddSingleton<EventRelayServer>();

            // Remote
            services.AddSingleton<InterdeviceEventRelay>();
            services.AddSingleton<EventRelayServer>();
            services.AddScoped<OutgoingEventConnection>();

            // Main manager
            services.AddScoped<EventManager>();
        }
    }
}
