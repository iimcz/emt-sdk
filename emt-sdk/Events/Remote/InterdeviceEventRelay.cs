using emt_sdk.Communication;
using emt_sdk.Settings;
using Google.Protobuf;
using Naki3D.Common.Protocol;
using System.Net;
using System.Net.Sockets;
using static emt_sdk.Events.EventManager;

namespace emt_sdk.Events.Remote
{
    public class InterdeviceEventRelay : ProtobufTcpListener<SensorMessage>
    {
        public event SensorMessageHandler OnMessage;

        public InterdeviceEventRelay(CommunicationSettings settings) : base(IPAddress.Parse(settings.InterdeviceListenIp), settings.EventListenPort) { }

        public void BroadcastSensorMessage(SensorMessage message)
        {
            BroadcastMessage(message);
        }

        protected override void HandleMessage(SensorMessage message, NetworkStream stream)
        {
            if (message.DataCase == SensorMessage.DataOneofCase.Event && message.Event.Name == "ping")
            {
                // Don't relay pings
                new SensorMessage{ Event = new EventData { Name = "pong" } }.WriteDelimitedTo(stream);
                return;
            }

            BroadcastMessage(message);
            OnMessage?.Invoke(message);
        }
    }
}
