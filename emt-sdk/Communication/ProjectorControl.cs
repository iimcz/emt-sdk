using System.Collections.Generic;
using System.Net.Sockets;
using Google.Protobuf;
using Naki3D.Common.Protocol;

namespace emt_sdk.Communication
{
    public class ProjectorControl
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly List<NetworkStream> _streams;
        
        public ProjectorControl(List<NetworkStream> streams)
        {
            _streams = streams;
        }

        public void PowerOn()
        {
            var message = new SensorControlMessage
            {
                CecMessage = new CECMessage
                {
                    Action = CECAction.PowerOn
                }
            };

            lock (_streams)
            {
                foreach (var stream in _streams) message.WriteDelimitedTo(stream);
            }

            Logger.Info("Sent power on CEC messages");
        }

        public void PowerOff()
        {
            var message = new SensorControlMessage
            {
                CecMessage = new CECMessage
                {
                    Action = CECAction.PowerOff
                }
            };

            lock (_streams)
            {
                foreach (var stream in _streams) message.WriteDelimitedTo(stream);
            }

            Logger.Info("Sent power off CEC messages");
        }
    }
}