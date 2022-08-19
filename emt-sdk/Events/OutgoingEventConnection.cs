using Google.Protobuf;
using Naki3D.Common.Protocol;
using NLog;
using System;
using System.IO;
using System.Net.Sockets;
using System.Timers;

namespace emt_sdk.Events
{
    public class OutgoingEventConnection : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly double TIMEOUT_INTERVAL = 5_000;
        private readonly double RECONNECT_INTERVAL = 5_000;

        private TcpClient _client;
        private NetworkStream _stream;

        private readonly Timer _timeoutTimer;
        private readonly Timer _reconnectTimer;

        private string _hostname;
        private int _port;

        public OutgoingEventConnection(string hostname, int port)
        {
            if (string.IsNullOrWhiteSpace(hostname)) throw new ArgumentException("Hostname cannot be null or empty", nameof(hostname));

            _hostname = hostname;
            _port = port;

            _client = new TcpClient(hostname, port);
            _stream = _client.GetStream();

            _timeoutTimer = new Timer(TIMEOUT_INTERVAL);
            _timeoutTimer.Elapsed += (sender, e) => Ping();

            _reconnectTimer = new Timer(RECONNECT_INTERVAL);
            _reconnectTimer.Elapsed += (sender, e) => Reconnect();
        }

        /// <summary>
        /// Broadcasts an event to target connected device
        /// </summary>
        /// <param name="message">Event to be sent</param>
        /// <exception cref="ArgumentNullException">Thrown when passed event is null</exception>
        public void BroadcastEvent(SensorMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.WriteDelimitedTo(_stream);
        }

        public void Connect()
        {
            _client?.Dispose();
            _client = new TcpClient
            {
                ReceiveTimeout = (int)TIMEOUT_INTERVAL * 2,
                SendTimeout = (int)TIMEOUT_INTERVAL * 2
            };

            try
            {
                _client.Connect(_hostname, _port);
                _stream = _client.GetStream();

                _reconnectTimer.Start();
                Logger.Info($"Connected to sync target at {_hostname}:{_port}");
            }
            catch (Exception e) when (e is SocketException || e is IOException)
            {
                Logger.Error(e, $"Failed to connect to sync target {_hostname}:{_port}");
            }
        }

        private void Reconnect()
        {
            if (_client.Connected) return;

            Logger.Info("Sync target disconnected, reconneting");
            Connect();
        }

        private void Ping()
        {
            try
            {
                var ping = new SensorMessage
                {
                    Event = new EventData { Name = "ping" }
                };

                ping.WriteDelimitedTo(_stream);
            }
            catch (SocketException e)
            {
                Logger.Error($"Failed to send ping to sync target {_hostname}:{_port}", e);
                return;
            }

            try
            { 
                var message = SensorMessage.Parser.ParseDelimitedFrom(_stream);
                if (message.DataCase != SensorMessage.DataOneofCase.Event && message.Event.Name != "pong")
                {
                    Logger.Error($"Sync target {_hostname}:{_port} failed to respond to ping, reconnecting");
                }
            }
            catch (Exception e) when (e is SocketException || e is InvalidProtocolBufferException)
            {
                Logger.Error($"Sync target {_hostname}:{_port} failed to respond to ping, reconnecting");
                return;
            }
        }

        public void Dispose()
        {
            _client?.Dispose();

            _timeoutTimer.Stop();
            _reconnectTimer.Stop();
        }
    }
}