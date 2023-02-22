using Google.Protobuf;
using Naki3D.Common.Protocol;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static emt_sdk.Events.EventManager;
using emt_sdk.Settings.EMT;
using emt_sdk.Settings;
using emt_sdk.Communication.Protobuf;
using System.Collections.Generic;

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

        protected override void HandleMessage(SensorMessage message, NetworkStream stream)
        {
            OnMessage?.Invoke(message);
        }

        public List<SensorDescriptor> GetSensorEndpoints(string filter = "")
        {
            throw new System.NotImplementedException();
        }
    }
}
