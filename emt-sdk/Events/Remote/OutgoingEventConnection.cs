using emt_sdk.Communication;
using emt_sdk.Events.Local;
using emt_sdk.Generated.ScenePackage;
using emt_sdk.Settings;
using Google.Protobuf;
using Naki3D.Common.Protocol;
using NLog;
using System;
using System.Net.Sockets;
using static emt_sdk.Events.EventManager;

namespace emt_sdk.Events.Remote
{
    /// <summary>
    /// Connection between two emt_sdk devices - client and server event server
    /// </summary>
    public class OutgoingEventConnection : ProtobufTcpClient<SensorMessage>
    {
        private static readonly new Logger Logger = LogManager.GetCurrentClassLogger();

        public event SensorMessageHandler OnMessage;

        public OutgoingEventConnection(Sync sync, CommunicationSettings settings) : base(sync.Elements[0].Hostname, settings.InterdeviceListenPort) { }

        /// <summary>
        /// Sends an event to target connected device
        /// </summary>
        /// <param name="message">Event to be sent</param>
        /// <exception cref="ArgumentNullException">Thrown when passed event is null</exception>
        public void SendEvent(SensorMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            SendMessage(message);
        }

        protected override void Ping()
        {
            try
            {
                SendMessage(new SensorMessage
                {
                    Event = new EventData { Name = "ping" }
                });
            }
            catch (SocketException e)
            {
                Logger.Error($"Failed to send ping to sync target {_hostname}:{_port}", e);
                return;
            }
        }

        protected override void HandleMessage(SensorMessage message)
        {
            try
            {
                // Don't relay pings
                if (message.DataCase == SensorMessage.DataOneofCase.Event && message.Event.Name == "pong") return;

                OnMessage?.Invoke(message);
            }
            catch (Exception e) when (e is SocketException || e is InvalidProtocolBufferException)
            {
                Logger.Error(e, $"Sync target {_hostname}:{_port} message could not be handled, reconnecting");
                return;
            }
        }
    }
}