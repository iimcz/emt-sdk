using emt_sdk.Packages;
using emt_sdk.Settings;
using emt_sdk.Settings.EMT;
using Grpc.Core;
using NLog;
using System.Threading.Tasks;

namespace emt_sdk.Communication.Exhibit
{
    public class GrpcServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Server _server;

        public GrpcServer(ConnectionService connection, DeviceService device, PackageService package, IConfigurationProvider<EMTSetting> config)
        {
            Logger.Info($"Creating GRPC server connection on '0.0.0.0:{config.Configuration.Communication.ContentPort}'");

            _server = new Server
            {
                Ports =
                {
                    new ServerPort("0.0.0.0", config.Configuration.Communication.ContentPort, ServerCredentials.Insecure)
                },

                Services =
                {
                    Naki3D.Common.Protocol.ConnectionService.BindService(connection),
                    Naki3D.Common.Protocol.DeviceService.BindService(device),
                    Naki3D.Common.Protocol.PackageService.BindService(package)
                }
            };
        }

        /// <summary>
        /// <inheritdoc cref="Server.Start" />
        /// </summary>
        public void Start()
        {
            Logger.Info($"Starting GRPC server, listening for connections");
            _server.Start();
        }

        /// <summary>
        /// <inheritdoc cref="Server.ShutdownAsync" />
        /// </summary>
        public async Task ShutdownAsync()
        {
            Logger.Info($"Shutting down GRPC server");
            await _server.ShutdownAsync();
        }

        /// <summary>
        /// <inheritdoc cref="Server.KillAsync" />
        /// </summary>
        public async Task KillAsync()
        {
            Logger.Warn($"Killing GRPC server");
            await _server.KillAsync();
        }
    }
}
