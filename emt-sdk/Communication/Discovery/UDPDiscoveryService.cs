using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Timers;
using emt_sdk.Settings;
using emt_sdk.Settings.EMT;
using Naki3D.Common.Protocol;
using Google.Protobuf;

namespace emt_sdk.Communication.Discovery
{
    public class UDPDiscoveryService : IDiscoveryService
    {
        private const int DISCOVERY_PORT = 37514;
        private const int DISCOVERY_PERIOD = 15_000; // In milliseconds

        private readonly UdpClient _udpClient = new UdpClient();
        private readonly byte[] _beacon;
        private readonly Timer _broadcastTimer = new Timer(DISCOVERY_PERIOD);

        private readonly IConfigurationProvider<EMTSetting> _emtSetting;

        public bool IsBroadcasting => _broadcastTimer.Enabled;

        public UDPDiscoveryService(IConfigurationProvider<EMTSetting> emtSetting)
        {
            _emtSetting = emtSetting;

            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, DISCOVERY_PORT));
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
            _udpClient.Send(_beacon, _beacon.Length);
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
