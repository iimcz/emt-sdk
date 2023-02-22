using Google.Protobuf;
using Naki3D.Common.Protocol;
using System.Net;
using System.Net.Sockets;
using static emt_sdk.Events.EventManager;
using emt_sdk.Settings.EMT;
using emt_sdk.Settings;
using emt_sdk.Communication.Protobuf;
using System.Collections.Generic;
using System.Linq;

namespace emt_sdk.Events.Local
{
    public class ProtobufTcpSensorManager : ProtobufTcpListener<SensorMessage>, ISensorManager
    {
        public event SensorDataMessageHandler OnMessage;

        private Dictionary<string, SensorDescriptor> _descriptors = new Dictionary<string, SensorDescriptor>();

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
            switch (message.MessageCase)
            {
                case SensorMessage.MessageOneofCase.Data:
                    OnMessage?.Invoke(message.Data);
                    break;
                case SensorMessage.MessageOneofCase.Descriptor_:
                    _descriptors[message.Descriptor_.Path] = message.Descriptor_;
                    break;
            }
        }

        public List<SensorDescriptor> GetSensorEndpoints(string filter = "")
        {
            return _descriptors.Values
                .Where(d => d.Path.Contains(filter))
                .ToList();
        }
    }
}
