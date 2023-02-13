using Google.Protobuf;
using Naki3D.Common.Protocol;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static emt_sdk.Events.EventManager;
using emt_sdk.Settings.EMT;
using emt_sdk.Settings;
using emt_sdk.Communication.Protobuf;

namespace emt_sdk.Events.Local
{
    public class ProtobufTcpSensorManager : ProtobufTcpListener<SensorMessage>, ISensorManager
    {
        public event SensorMessageHandler OnMessage;

        public ProtobufTcpSensorManager(IConfigurationProvider<EMTSetting> settings) : 
            base(IPAddress.Parse(settings.Configuration.Communication.SensorListenIp), settings.Configuration.Communication.SensorListenPort) { }

        public void BroadcastControlMessage(SensorControlMessage message)
        {
            lock (_clients)
            {
                foreach (var (_, stream) in _clients)
                {
                    message.WriteDelimitedTo(stream);
                }
            }
        }

        protected override void HandleConnection((TcpClient tcpClient, NetworkStream stream) clientInfo, CancellationToken cancellationToken)
        {
            // TODO: Sometimes we can have a non-sensor based approach!
            // Ideally the event of getting a new sensor shouldn't automatically trigger a CEC power on

            // Attempt to power on a projector (other sensors ignore this message)
            // ProjectorControl.PowerOn();

            base.HandleConnection(clientInfo, cancellationToken);
        }

        protected override void HandleMessage(SensorMessage message, NetworkStream stream)
        {
            OnMessage?.Invoke(message);
        }
    }
}
