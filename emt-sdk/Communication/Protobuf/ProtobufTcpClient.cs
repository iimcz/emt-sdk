using Google.Protobuf;
using NLog;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Timer = System.Timers.Timer;

namespace emt_sdk.Communication.Protobuf
{
    public abstract class ProtobufTcpClient<T> : IDisposable where T : IMessage<T>, new()
    {
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected const double TIMEOUT_INTERVAL = 5_000;
        protected const double RECONNECT_INTERVAL = 5_000;

        protected TcpClient _client;
        protected NetworkStream _stream;

        protected readonly Timer _timeoutTimer = new Timer(TIMEOUT_INTERVAL);
        protected readonly Timer _reconnectTimer = new Timer(RECONNECT_INTERVAL);

        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected MessageParser<T> _parser = new MessageParser<T>(() => new T());

        protected string _hostname;
        protected int _port;

        public ProtobufTcpClient(string hostname, int port)
        {
            Logger = LogManager.GetCurrentClassLogger();

            _hostname = hostname;
            _port = port;

            _timeoutTimer.Elapsed += (sender, e) => Ping();
            _reconnectTimer.Elapsed += (sender, e) => Reconnect();
        }

        /// <summary>
        /// Connects to specified remote server and listens for messages. This call will block the current thread.
        /// </summary>
        public void Connect()
        {
            if (string.IsNullOrWhiteSpace(_hostname)) throw new ArgumentException("Hostname cannot be null or empty", nameof(_hostname));
            
            _client?.Dispose();
            _client = new TcpClient
            {
                ReceiveTimeout = (int)TIMEOUT_INTERVAL * 2,
                SendTimeout = (int)TIMEOUT_INTERVAL * 2
            };

            try
            {
                Logger.Info($"Attempting to connect to target at {_hostname}:{_port}");

                _reconnectTimer.Start();
                _client.Connect(_hostname, _port);

                _stream = _client.GetStream();
                _timeoutTimer.Start();

                Logger.Info($"Connected to target at {_hostname}:{_port}");

                ReadMessages();
            }
            catch (Exception e) when (e is SocketException || e is IOException)
            {
                Logger.Error(e, $"Failed to connect to target {_hostname}:{_port}");
            }
        }

        public void Disconnect()
        {
            _reconnectTimer.Stop();
            _timeoutTimer.Stop();

            _tokenSource.Cancel();
        }

        public void Dispose()
        {
            _client?.Dispose();

            _reconnectTimer.Stop();
            _timeoutTimer.Stop();
        }

        protected void Reconnect()
        {
            if (_client.Connected) return;

            Logger.Info("Target disconnected, reconneting");
            Connect();
        }

        protected void ReadMessages()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    var message = _parser.ParseDelimitedFrom(_stream);
                    HandleMessage(message);
                }
                catch (Exception e) when (e is SocketException || e is InvalidProtocolBufferException)
                {
                    Logger.Error(e, "Failed to parse message from {Hostname}:{Port}", _hostname, _port);
                    return;
                }
            }
        }

        protected void SendMessage(T message)
        {
            if (!_client.Connected) return;
                message.WriteDelimitedTo(_stream);
        }

        protected abstract void Ping();
        protected abstract void HandleMessage(T message);
    }
}
