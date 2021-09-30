using emt_sdk.Communication;
using emt_sdk.Extensions;
using emt_sdk.Generated.ScenePackage;
using emt_sdk.Settings;
using Google.Protobuf;
using Naki3D.Common.Protocol;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace emt_sdk.Events
{
    public class EventManager
    {
        public const int SENSOR_MESSAGE_PORT = 5000;

        public static EventManager Instance { get; } = new EventManager();

        public bool IsConnectedOutgoing => _outgoingClients.Count == (_sync.Elements.Count - 1);
        public bool IsConnectedIncoming => _listeners == (_sync.Elements.Count - 1);
        public bool IsConnected => IsConnectedOutgoing && IsConnectedOutgoing;
        public bool IsListening { get; private set; } = false;

        public delegate void SensorMessageHandler(object sender, SensorMessage e);
        public event SensorMessageHandler OnEventReceived;

        private CancellationTokenSource _tokenSource;
        private Sync _sync;

        // TODO: We need a way to get the encryption keys here
        private TcpListener _listener;
        private int _listeners = 0;

        private readonly List<TcpClient> _outgoingClients = new List<TcpClient>();
        private readonly List<NetworkStream> _outgoingStreams = new List<NetworkStream>();

        public void BroadcastEvent(SensorMessage message)
        {
            // In case it's a local event
            OnEventReceived?.Invoke(this, message);

            foreach (var client in _outgoingStreams) message.WriteJsonTo(client);
        }

        public void Start(Sync sync, CommunicationSettings settings)
        {
            Start(sync, settings.SensorListenIp, settings.SensorListenPort);
        }

        public void Start(Sync sync, string ip = null, int port = SENSOR_MESSAGE_PORT)
        {
            _sync = sync;
            ip = ip ?? string.Empty;

            if (!IPAddress.TryParse(ip, out var ipAddr)) ipAddr = IPAddress.Any;
            _listener = new TcpListener(ipAddr, port);

            _tokenSource = new CancellationTokenSource();

            if (sync == null)
            {
                Debug.WriteLine("No sync info specified, running in listen only mode");
                Task.Run(() => EstabilishOutgoing(sync), _tokenSource.Token);
            }

            _listener.Start();
            IsListening = true;

            using (_tokenSource.Token.Register(() => _listener.Stop()))
            {
                try
                {
                    while (true)
                    {
                        var client = _listener.AcceptTcpClient();
                        Task.Run(() => HandleConnection(client, _tokenSource.Token), _tokenSource.Token);
                    }
                }
                catch { } // We just eat the exception, everything is closing anyways
                finally
                {
                    if (_listener.Server.IsBound) _listener.Stop();
                    IsListening = false;
                }
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            foreach (var client in _outgoingClients) client.Close();
        }

        private void EstabilishOutgoing(Sync sync)
        {
            for (int i = 0; i < sync.Elements.Count; i++)
            {
                if (sync.SelfIndex == i) continue;
                TcpClient client = new TcpClient(sync.Elements[i].Hostname, SENSOR_MESSAGE_PORT);

                _outgoingClients.Add(client);
                _outgoingStreams.Add(client.GetStream());
            }
        }

        private void HandleConnection(TcpClient client, CancellationToken token)
        {
            Interlocked.Increment(ref _listeners);
            var stream = client.GetStream();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // TODO: that read could use that cancellation token for sure...
                    var sensorEvent = SensorMessage.Parser.ParseDelimitedFrom(stream);
                    if (sensorEvent.DataCase == SensorMessage.DataOneofCase.None) continue;
                    OnEventReceived?.Invoke(this, sensorEvent);
                }
            }
            catch (InvalidProtocolBufferException e)
            {
                Debug.WriteLine("Invalid message received, ignoring.");
            }
            catch (SocketException e)
            {
                // TODO: Install nlog
                // TODO: Test unexpected cases
                // TODO: Test on linux
                // TODO: Remove user counting
                Debug.WriteLine(e);
            }
            finally
            {
                Interlocked.Decrement(ref _listeners);
                if (client.Connected) client.Close();
            }
        }
    }
}
