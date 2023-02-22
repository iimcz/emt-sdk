using emt_sdk.Communication.Protobuf;
using emt_sdk.Settings;
using emt_sdk.Settings.EMT;
using Google.Protobuf;
using Naki3D.Common.Protocol;
using System.Net;
using System.Net.Sockets;
using static emt_sdk.Events.EventManager;

namespace emt_sdk.Events.Remote
{
    public class InterdeviceEventRelay : ProtobufTcpListener<SensorDataMessage>
    {
        public event SensorDataMessageHandler OnMessage;

        public InterdeviceEventRelay(IConfigurationProvider<EMTSetting> settings) :
            base(IPAddress.Parse(settings.Configuration.Communication.InterdeviceListenIp), settings.Configuration.Communication.InterdeviceListenPort) { }

        public void BroadcastSensorMessage(SensorDataMessage message)
        {
            BroadcastMessage(message);
        }

        protected override void HandleMessage(SensorDataMessage message, NetworkStream stream)
        {
            // TODO: Do we still need this?
            if (message.DataCase == SensorDataMessage.DataOneofCase.String && message.String.EndsWith("ping"))
            {
                // Don't relay pings
                new SensorDataMessage 
                {
                    Path = $"{Dns.GetHostName()}/sdk",
                    String = "pong"
                }.WriteDelimitedTo(stream);
                return;
            }

            BroadcastMessage(message);
            OnMessage?.Invoke(message);
        }
    }
}
