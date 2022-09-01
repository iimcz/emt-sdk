using Google.Protobuf;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace emt_sdk.Communication
{
    public abstract class ProtobufTcpListener<T> where T : IMessage<T>, new()
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // TODO: Changing timeout at runtime? Probably don't need it
        public int Timeout { get; set; }

        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected MessageParser<T> _parser = new MessageParser<T>(() => new T());

        protected readonly TcpListener _listener;
        protected readonly List<(TcpClient tcpClient, NetworkStream stream)> _clients = new List<(TcpClient, NetworkStream)>();

        public bool IsListening { get; private set; } = false;
        public CancellationToken CancellationToken => _tokenSource.Token;

        public ProtobufTcpListener(IPAddress listenAddress, int port)
        {
            if (listenAddress == null) throw new ArgumentNullException(nameof(listenAddress));

            _listener = new TcpListener(listenAddress, port);
        }

        public void Start()
        {
            _listener.Start();
            IsListening = true;

            using (_tokenSource.Token.Register(() => _listener.Stop()))
            {
                try
                {
                    while (true)
                    {
                        var client = _listener.AcceptTcpClient();
                        client.ReceiveTimeout = Timeout;
                        client.SendTimeout = Timeout;
                        Logger.Info($"Accepted connection from {client.Client.RemoteEndPoint}");

                        var clientInfo = (client, client.GetStream());
                        lock (_clients) _clients.Add(clientInfo);
                        Task.Run(() => HandleConnection(clientInfo, _tokenSource.Token));
                    }
                }
                catch (SocketException e)
                {
                    Logger.Warn(e, "Exception during listener shutdown");
                    throw e;
                }
                finally
                {
                    if (_listener.Server.IsBound) _listener.Stop();
                    IsListening = false;
                }
            }
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
            lock (_clients) foreach (var (tcpClient, _) in _clients) tcpClient.Dispose();
        }

        protected void BroadcastMessage(T message)
        {
            lock (_clients)
            {
                foreach (var (_, stream) in _clients)
                {
                    message.WriteDelimitedTo(stream);
                }
            }
        }

        protected virtual void HandleConnection((TcpClient tcpClient, NetworkStream stream) clientInfo, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = _parser.ParseDelimitedFrom(clientInfo.stream);
                    HandleMessage(message, clientInfo.stream);
                }
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Error("Invalid protobuf message received, closing connection", e);
            }
            catch (SocketException e)
            {
                Logger.Error("Socket error on protobuf connection ", e);
            }
            finally
            {
                lock (_clients) _clients.Remove(clientInfo);
                if (clientInfo.tcpClient.Connected) clientInfo.tcpClient.Close();

                Logger.Info($"Closed connection with {clientInfo.tcpClient.Client.RemoteEndPoint}");
            }
        }

        protected abstract void HandleMessage(T message, NetworkStream stream);
    }
}
