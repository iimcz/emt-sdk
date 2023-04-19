using Google.Protobuf;
using Naki3D.Common.Protocol;
using System;
using System.Net.Sockets;
using System.Threading;

namespace emt_sdk.Events.Relay
{
    /// <summary>
    /// Server event relaying connection for any external applications using emt_sdk events.
    /// Relays local, remote and even relayed events to a connected <see cref="EventRelayClient"/>.
    /// This should not be used in user code and is only for the main managing application.
    /// </summary>
    public class EventRelayServer
    {
        /// <summary>
        /// Default relay listening port
        /// </summary>
        public const int RELAY_PORT = 49155;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Whether the server is connected to a matching client
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Token source for closing socket connection, may be closed after receiving one more event
        /// </summary>
        public CancellationTokenSource TokenSource;

        private NetworkStream _stream;
        private EventManager _eventManager;

        public EventRelayServer(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        /// <summary>
        /// Starts listening on <paramref name="port"/> on the loopback interface.
        /// Blocking call, run this in a separate thread/task.
        /// </summary>
        /// <exception cref="SocketException">Thrown on any socket related problem</exception>
        /// <param name="port">Listening port</param>
        public void Listen(int port = RELAY_PORT)
        {
            var listener = new TcpListener(System.Net.IPAddress.Loopback, port);

            listener.Start();
            _eventManager.OnEventReceived += OnEventReceived;
            Logger.Info($"Listening for relay messages on port {port}");

            TokenSource = new CancellationTokenSource();
            using (TokenSource.Token.Register(() => listener.Stop()))
            {
                try
                {
                    while (true)
                    {
                        var client = listener.AcceptTcpClient();
                        Logger.Info($"Accepted relay connection from {client.Client.RemoteEndPoint}");
                        IsConnected = true;

                        _stream = client.GetStream();

                        try
                        {
                            while (!TokenSource.Token.IsCancellationRequested)
                            {
                                var sensorEvent = SensorDataMessage.Parser.ParseDelimitedFrom(_stream);
                                if (sensorEvent.DataCase == SensorDataMessage.DataOneofCase.None) continue;
                                _eventManager.HandleLocalMessage(sensorEvent);
                            }
                        }
                        catch (InvalidProtocolBufferException e)
                        {
                            Logger.Error("Invalid event protobuf message received, closing relay connection", e);
                        }
                        catch (SocketException e)
                        {
                            Logger.Error("Socket error on event connection ", e);
                        }
                        finally
                        {
                            if (client.Connected) client.Close();
                        }
                    }
                }
                catch (SocketException e)
                {
                    Logger.Warn(e, "Exception during relay client shutdown");
                    throw e;
                }
                finally
                {
                    if (listener.Server.IsBound) listener.Stop();
                    _eventManager.OnEventReceived -= OnEventReceived;
                    IsConnected = false;
                }
            }
        }

        /// <summary>
        /// Relays a message to the connected client that is not sent to any other device.
        /// Should be used only for debugging purposes.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when passed event is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when server is not connected</exception>
        /// <param name="message">Event to be sent</param>
        public void RelayLocalEvent(SensorDataMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (!IsConnected)
            {
                Logger.Error("Received event despite not being connected, relay is in inconsistent state");
                //throw new InvalidOperationException("Cannot relay event, no connection estabilished");
                return; // TODO: we already log this, so just discard the event, but this should be discussed
            }

            Logger.Warn("Relaying local only event, this should not be used in final builds");
            message.WriteDelimitedTo(_stream);
        }

        private void OnEventReceived(SensorDataMessage message)
        {
            if (!IsConnected)
            {
                Logger.Error("Received event despite not being connected, relay is in inconsistent state");
                //throw new InvalidOperationException("Cannot relay event, no connection estabilished");
                return; // TODO: we already log this, so just discard the event, but this should be discussed
            }

            message.WriteDelimitedTo(_stream);
        }
    }
}
