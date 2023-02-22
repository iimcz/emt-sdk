using Google.Protobuf;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace emt_sdk.Communication.Protobuf
{
    public abstract class ProtobufTcpListener<T> where T : IMessage<T>, new()
    {
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public int Timeout { get; set; }

        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected MessageParser<T> _parser = new MessageParser<T>(() => new T());

        protected readonly TcpListener _listener;
        protected readonly List<(TcpClient tcpClient, NetworkStream stream)> _clients = new List<(TcpClient, NetworkStream)>();

        public bool IsListening { get; private set; } = false;
        public CancellationToken CancellationToken => _tokenSource.Token;

        public ProtobufTcpListener(IPAddress listenAddress, int port)
        {
            Logger = LogManager.GetCurrentClassLogger();
            if (listenAddress == null) throw new ArgumentNullException(nameof(listenAddress));

            Logger.Info($"Listening on {listenAddress}:{port}");
            _listener = new TcpListener(listenAddress, port);
        }

        /// <summary>
        /// Listens for new connections and receives messages from each created connection. This call will block the current thread.
        /// </summary>
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
                Logger.Error("Socket error on protobuf connection", e);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Exception occuer when handling sensor message");
            }
            finally
            {
                Logger.Info($"Closed connection with {clientInfo.tcpClient.Client.RemoteEndPoint}");

                lock (_clients) _clients.Remove(clientInfo);
                if (clientInfo.tcpClient.Connected) clientInfo.tcpClient.Close();
            }
        }

        protected abstract void HandleMessage(T message, NetworkStream stream);
    }
}
