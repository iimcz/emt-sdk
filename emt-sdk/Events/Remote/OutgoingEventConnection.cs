using emt_sdk.Communication.Protobuf;
using emt_sdk.Packages;
using emt_sdk.Settings;
using emt_sdk.Settings.EMT;
using Google.Protobuf;
using Naki3D.Common.Protocol;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using static emt_sdk.Events.EventManager;

namespace emt_sdk.Events.Remote
{
    /// <summary>
    /// Connection between two emt_sdk devices - client and server event server
    /// </summary>
    public class OutgoingEventConnection : ProtobufTcpClient<SensorDataMessage>
    {
        private static readonly new Logger Logger = LogManager.GetCurrentClassLogger();

        public event SensorDataMessageHandler OnMessage;

        public OutgoingEventConnection(IConfigurationProvider<Sync> sync, IConfigurationProvider<EMTSetting> settings) :
            base(sync.Configuration.RelayAddress, settings.Configuration.Communication.InterdeviceListenPort) { }

        /// <summary>
        /// Sends an event to target connected device
        /// </summary>
        /// <param name="message">Event to be sent</param>
        /// <exception cref="ArgumentNullException">Thrown when passed event is null</exception>
        public void SendEvent(SensorDataMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            SendMessage(message);
        }

        protected override void Ping()
        {
            // TODO: Cache calls of DNS gethostname to some separate service
            try
            {
                SendMessage(new SensorDataMessage
                {
                    String = "ping",
                    Path = $"{Dns.GetHostName()}/sdk/ping"
                });
            }
            catch (SocketException e)
            {
                Logger.Error($"Failed to send ping to sync target {_hostname}:{_port}", e);
                return;
            }
        }

        protected override void HandleMessage(SensorDataMessage message)
        {
            try
            {
                // Don't relay pings
                if (message.DataCase == SensorDataMessage.DataOneofCase.String && message.String == "pong") return;

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