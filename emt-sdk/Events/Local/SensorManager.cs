using emt_sdk.Generated.ScenePackage;
using emt_sdk.Settings;
using Google.Protobuf;
using Naki3D.Common.Protocol;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using emt_sdk.Communication;
using static emt_sdk.Events.EventManager;

namespace emt_sdk.Events.Local
{
    public class SensorManager : ProtobufTcpListener<SensorMessage>
    {
        public ProjectorControl ProjectorControl { get; private set; }

        public event SensorMessageHandler OnMessage;

        public SensorManager(CommunicationSettings settings) : base(IPAddress.Parse(settings.SensorListenIp), settings.EventListenPort)
        {
            ProjectorControl = new ProjectorControl(this);
        }

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
            // Attempt to power on a projector (other sensors ignore this message)
            ProjectorControl.PowerOn();

            base.HandleConnection(clientInfo, cancellationToken);
        }

        protected override void HandleMessage(SensorMessage message, NetworkStream stream)
        {
            OnMessage?.Invoke(message);
        }
    }
}
