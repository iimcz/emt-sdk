using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Timers;
using emt_sdk.Settings;
using emt_sdk.Settings.EMT;
using Naki3D.Common.Protocol;
using Google.Protobuf;
using NLog;

namespace emt_sdk.Communication.Discovery
{
    public class UDPDiscoveryService : IDiscoveryService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private const int DISCOVERY_PORT = 37514;
        private const int DISCOVERY_PERIOD = 15_000; // In milliseconds

        private readonly UdpClient _udpClient = new UdpClient();
        private readonly byte[] _beacon;
        private readonly Timer _broadcastTimer = new Timer(DISCOVERY_PERIOD);
        private readonly IPEndPoint _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);

        private readonly IConfigurationProvider<EMTSetting> _emtSetting;

        public bool IsBroadcasting => _broadcastTimer.Enabled;

        public UDPDiscoveryService(IConfigurationProvider<EMTSetting> emtSetting)
        {
            _emtSetting = emtSetting;

            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            _beacon = SerializeBeacon();
            _broadcastTimer.Elapsed += (sender, args) => Broadcast();
        }

        private byte[] SerializeBeacon()
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CodedOutputStream(ms))
                {
                    var beacon = new BeaconMessage
                    {
                        DeviceType = _emtSetting.Configuration.Type.GetName(),
                        Hostname = Dns.GetHostName(),
                        ProtocolVersion = 2
                    };

                    beacon.WriteTo(cs);
                }

                return ms.ToArray();
            }
        }

        private void Broadcast()
        {
            Logger.Log(LogLevel.Debug, $"Broadcasting beacon packet on port {DISCOVERY_PORT}");
            _udpClient.Send(_beacon, _beacon.Length, _broadcastEndPoint);
        }

        public void StartBroadcast()
        {
            _broadcastTimer.Start();
        }

        public void StopBroadcast()
        {
            _broadcastTimer.Stop();
        }
    }
}
