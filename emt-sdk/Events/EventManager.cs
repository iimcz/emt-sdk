using emt_sdk.Generated.ScenePackage;
using emt_sdk.Settings;
using Google.Protobuf;
using Naki3D.Common.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using emt_sdk.Communication;
using Action = emt_sdk.Generated.ScenePackage.Action;

namespace emt_sdk.Events
{
    /// <summary>
    /// Main emt_sdk event communication server-client used for both receiving and sending events from/to other devices. Should not be used in user code.
    /// </summary>
    public class EventManager
    {
        /// <summary>
        /// Default event listening port and also target port for other devices
        /// </summary>
        public const int SENSOR_MESSAGE_PORT = 5000;

        /// <summary>
        /// Singleton instance of <see cref="EventManager"/> for easier state management
        /// </summary>
        public static EventManager Instance { get; } = new EventManager();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Whether the manager is currently listening for new connections
        /// </summary>
        public bool IsListening { get; private set; } = false;

        /// <summary>
        /// Gets the amount of currently connected listeners
        /// </summary>
        public int Listeners => _listeners;

        public ProjectorControl ProjectorControl { get; private set; }

        public List<Action> Actions { get; } = new List<Action>();

        /// <summary>
        /// Handler for receiving sensor events
        /// </summary>
        /// <param name="sender">Sender of event, <see cref="EventManager"/> in most cases</param>
        /// <param name="e">Received message</param>
        public delegate void SensorMessageHandler(object sender, SensorMessage e);
        
        /// <summary>
        /// Handler for executing effects
        /// </summary>
        public delegate void EffectHandler(object sender, EffectCall e);

        /// <summary>
        /// Called whenever an event is received either locally, from other device or from a relay
        /// </summary>
        public event SensorMessageHandler OnEventReceived;
        
        /// <summary>
        /// Called whenever an effect is executed
        /// </summary>
        public event EffectHandler OnEffectCalled;

        /// <summary>
        /// Token for closing all socket connections, may be closed after receiving one more event per socket
        /// </summary>
        public CancellationToken Token => _tokenSource.Token;

        private CancellationTokenSource _tokenSource;
        private Sync _sync;

        // TODO: We need a way to get the encryption keys here
        private TcpListener _listener;
        private int _listeners = 0;

        private readonly List<TcpClient> _outgoingClients = new List<TcpClient>();
        private readonly List<NetworkStream> _outgoingStreams = new List<NetworkStream>();

        private readonly List<NetworkStream> _localIncoming = new List<NetworkStream>();

        public EventManager()
        {
            ProjectorControl = new ProjectorControl(_localIncoming);
        }

        /// <summary>
        /// Broadcasts an event to all connected devices and relays (if connected)
        /// </summary>
        /// <param name="message">Event to be sent</param>
        /// <exception cref="ArgumentNullException">Thrown when passed event is null</exception>
        public void BroadcastEvent(SensorMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // In case it's a local event
            OnEventReceived?.Invoke(this, message);

            if (_outgoingStreams.Count == 0)
            {
                Logger.Warn("No remote listeners connected, broadcasting event as local only");
                return;
            }

            foreach (var client in _outgoingStreams) message.WriteDelimitedTo(client);
        }

        /// <summary>
        /// Starts listening for incoming connections and connects to other available devices.
        /// Calls <seealso cref="Start(Sync, string, int)"/> with specified <see cref="CommunicationSettings"/>.
        /// </summary>
        /// <param name="sync">Sync data used for connecting to other devices</param>
        /// <param name="settings">Socket settings</param>
        public void Start(Sync sync, CommunicationSettings settings)
        {
            Start(sync, settings.SensorListenIp, settings.SensorListenPort);
        }

        /// <summary>
        /// Starts listening for incoming connections and connects to other available devices.
        /// </summary>
        /// <param name="sync">Sync data used for connecting to other devices</param>
        /// <param name="ip">Listening IP address</param>
        /// <param name="port">Listening and target port for both incoming and outgoing sockets</param>
        /// <exception cref="SocketException">Throw on any socket related problems</exception>
        public void Start(Sync sync, string ip = null, int port = SENSOR_MESSAGE_PORT)
        {
            if (_listener != null && _listener.Server.IsBound)
            {
                Logger.Info("Attempted to restart EventManager even though it's already running, ignoring.");
                return;
            }

            _sync = sync;
            ip = ip ?? string.Empty;

            if (!IPAddress.TryParse(ip, out var ipAddr))
            {
                ipAddr = IPAddress.Any;
                Logger.Warn($"Failed to parse listening address {ip}, defaulting to any interface.");
            }
            
            _outgoingClients.Clear();
            _outgoingStreams.Clear();
            lock (_localIncoming) _localIncoming.Clear();

            _listener = new TcpListener(ipAddr, port);
            _tokenSource = new CancellationTokenSource();

            if (sync == null)
            {
                Logger.Warn("No sync info specified, running in listen only mode");
                Task.Run(() => EstabilishOutgoing(sync), _tokenSource.Token);
            }

            _listener.Start();
            Logger.Info($"Listening on port {port} for incoming events");
            IsListening = true;

            using (_tokenSource.Token.Register(() => _listener.Stop()))
            {
                try
                {
                    while (true)
                    {
                        var client = _listener.AcceptTcpClient();
                        Logger.Info($"Accepted connection from {client.Client.RemoteEndPoint}");
                        Task.Run(() => HandleConnection(client, _tokenSource.Token), _tokenSource.Token);
                    }
                }
                catch (SocketException e)
                {
                    Logger.Warn(e, "Exception during event manager shutdown");
                    throw e;
                }
                finally
                {
                    if (_listener.Server.IsBound) _listener.Stop();
                    IsListening = false;
                }
            }
        }

        /// <summary>
        /// Stops listening for new connections and closes all outgoing sockets
        /// </summary>
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

            IPEndPoint endPoint = client.Client.LocalEndPoint as IPEndPoint;
            var address = endPoint.Address.GetAddressBytes();
            if (true /* some custom address filtering here for 2nd interface*/)
            {
                lock (_localIncoming) _localIncoming.Add(stream);
            }
            
            // Attempt to start projector
            ProjectorControl.PowerOn();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // TODO: that read could use that cancellation token for sure...
                    var sensorEvent = SensorMessage.Parser.ParseDelimitedFrom(stream);
                    if (sensorEvent.DataCase == SensorMessage.DataOneofCase.None) continue;
                    
                    OnEventReceived?.Invoke(this, sensorEvent);
                    
                    var raisedEffects = Actions
                        .Where(a => a.ShouldExecute(sensorEvent))
                        .Select(a => new EffectCall
                        {
                            Name = a.Effect,
                            Value = a.MapValue(sensorEvent)
                        });

                    foreach (var raisedEffect in raisedEffects) OnEffectCalled?.Invoke(this, raisedEffect);
                }
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Error("Invalid event protobuf message received, closing connection", e);
            }
            catch (SocketException e)
            {
                // TODO: Test unexpected cases
                // TODO: Test on linux
                Logger.Error("Socket error on event connection ", e);
            }
            finally
            {
                Interlocked.Decrement(ref _listeners);
                lock (_localIncoming) _localIncoming.Remove(stream);
                if (client.Connected) client.Close();
            }
        }
    }
}
