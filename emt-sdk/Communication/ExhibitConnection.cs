using emt_sdk.Extensions;
using Naki3D.Common.Protocol;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using emt_sdk.Generated.ScenePackage;
using emt_sdk.Settings;
using Google.Protobuf;

namespace emt_sdk.Communication
{
    public enum ConnectionStateEnum
    {
        Disconnected,
        Connected,
        VersionCheck,
        VerifyRequest,
        VerifyWait,
        Verified,
        DescriptorSent,
        PackageInfoReceived,
        VerificationDenied
    }

    public class ExhibitConnection : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly double TIMEOUT_INTERVAL = 5_000; // Not changeable, set by protocol

        private readonly Timer _timeoutTimer;
        private readonly Timer _reconnectTimer;
        private readonly string _id;
        private readonly IPEndPoint _target;
        private readonly DeviceDescriptor _descriptor;

        private TcpClient _client;
        private Stream _stream;
        private JsonObjectStringReader _jsonReader;

        public bool IsConnected => _client.Connected;
        public bool Verified { get; private set; } = false;
        
        public VersionInfo ClientVersion { get; private set; }
        public VersionInfo ServerVersion { get; private set; }
        public ConnectionStateEnum ConnectionState { get; private set; } = ConnectionStateEnum.Disconnected;
        public EncryptionInfo EncryptionInfo { get; private set; } = null;

        /// <summary>
        /// Gets or sets the reconnect interval in ms
        /// </summary>
        public float ReconnectInterval { get; set; } = 5000;
        public Action<LoadPackage> LoadPackageHandler { get; set; }
        public Action<ClearPackage> ClearPackageHandler { get; set; }

        public ExhibitConnection(CommunicationSettings settings, DeviceDescriptor descriptor, string id = null)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _id = id ?? Dns.GetHostName();
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            
            _target = new IPEndPoint(IPAddress.Parse(settings.ContentHostname), settings.ContentPort);

            _timeoutTimer = new Timer(TIMEOUT_INTERVAL);
            _timeoutTimer.Elapsed += TimeoutElapsed;

            _reconnectTimer = new Timer(ReconnectInterval);
            _reconnectTimer.Elapsed += ReconnectElapsed;
        }

        public void Connect()
        {
            _client?.Dispose();
            _client = new TcpClient();
            _client.ReceiveTimeout = (int)TIMEOUT_INTERVAL * 2;
            _client.SendTimeout = (int)TIMEOUT_INTERVAL * 2;

            try
            {
                _reconnectTimer.Start();
                _client.Connect(_target);
                _stream = _client.GetStream();
                _jsonReader = new JsonObjectStringReader(_stream);

                ConnectionState = ConnectionStateEnum.Connected;
            }
            catch (Exception e) when (e is SocketException || e is IOException)
            {
                Logger.Error(e, $"Failed to connect to remote host");
                return;
            }
            
            ServerVersion = null;
            
            ConnectionState = ConnectionStateEnum.VersionCheck;
            CompareVersions();
            ConnectionState = ConnectionStateEnum.VerifyRequest;
            RequestConnection();
            _timeoutTimer.Start();
            
            if (!Verified) WaitForVerification();
            SendDescriptor();
        }

        private void SendDescriptor()
        {
            if (!_client.Connected) throw new InvalidOperationException();
            var msg = new DeviceMessage
            {
                ConnectionId = _id,
                DeviceDescriptor = _descriptor
            };
            msg.WriteJsonTo(_stream);
            ConnectionState = ConnectionStateEnum.DescriptorSent;

            // TODO: Server actually sends no info
            //EncryptionInfo = EncryptionInfo.Parser.ParseJson(_jsonReader.NextJsonObject());
            Task.Run(HandlePackages);
        }
        
        private void TimeoutElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_client.Connected)
            {
                _timeoutTimer.Stop();
                return;
            }
            
            var ping = new DeviceMessage
            {
                ConnectionId = _id,
                Ping = new Ping()
            };

            try
            {
                ping.WriteJsonTo(_stream);
            }
            catch (Exception ex) when (ex is InvalidProtocolBufferException || ex is IOException)
            {
                Logger.Error(ex, "Failed to send ping, disconnecting");
                _client.Close();
                _timeoutTimer.Stop();
            }
        }
        
        private void ReconnectElapsed(object sender, ElapsedEventArgs e)
        {
            if (_client.Connected) return;
            
            ConnectionState = ConnectionStateEnum.Disconnected;
            Logger.Info("Not connected, reconneting");
            Connect();
        }

        private void WaitForVerification()
        {
            var verificationCompleted = false;
            while (!verificationCompleted)
            {
                try
                {
                    var ack = ConnectionAcknowledgement.Parser.ParseJson(_jsonReader.NextJsonObject());
                    
                    Verified = ack.Verified;
                    verificationCompleted = true;
                }
                catch (Exception e) when (e is IOException || e is InvalidDataException)
                {
                    if (_client.Connected) continue;
                    
                    Logger.Error(e, "Verification failed, disconnecting");
                    _client.Close();
                    return; // Connection failed completely
                }
            }

            if (!Verified)
            {
                Logger.Warn("Verification denied, closing connection without retry");
                
                ConnectionState = ConnectionStateEnum.VerificationDenied;
                _client.Close();
                _timeoutTimer.Stop();
                _reconnectTimer.Stop();
            }
        }

        private void RequestConnection()
        {
            var request = new ConnectionRequest
            {
                ConnectionId = _id,
                PublicKey = ByteString.CopyFromUtf8("") // TODO: Add key support
            };

            try
            {
                request.WriteJsonTo(_stream);
                ConnectionState = ConnectionStateEnum.VerifyWait;
                
                // TODO: Do we need to check hostname? Probably not
                var ack = ConnectionAcknowledgement.Parser.ParseJson(_jsonReader.NextJsonObject());
                Verified = ack.Verified;

                if (Verified)
                {
                    Logger.Info("Connection pre-verified, handshake complete");
                    ConnectionState = ConnectionStateEnum.Verified;
                }
                else
                {
                    Logger.Info("Waiting for verification");
                }
            }
            catch (Exception e) when (e is InvalidProtocolBufferException || e is InvalidJsonException)
            {
                Logger.Error(e, "Failed to verify on remote side, disconnecting");
                _client.Close();
            }
        }

        private void CompareVersions()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            ClientVersion = assemblyVersion.ToVersionInfo();

            try
            {
                ClientVersion.WriteJsonTo(_stream);
                ServerVersion = VersionInfo.Parser.ParseJson(_jsonReader.NextJsonObject());
                if (!IsVersionCompatible(ServerVersion)) throw new InvalidOperationException();
            }
            catch (Exception e) when (e is InvalidProtocolBufferException || e is IOException || e is InvalidDataException)
            {
                Logger.Error(e, "Failed to compare versions, disconnecting");
                _client.Close();
            }
        }

        private bool IsVersionCompatible(VersionInfo verInfo)
        {
            return verInfo != null;
        }

        private void HandlePackages()
        {
            while (_client.Connected)
            {
                ServerMessage msg;
                
                try
                {
                    msg = ServerMessage.Parser.ParseJson(_jsonReader.NextJsonObject());
                }
                catch (Exception e) when (e is InvalidProtocolBufferException || e is IOException || e is InvalidDataException)
                {
                    if (_client.Connected) continue;
                    
                    Logger.Error(e, "Package handling failed, disconnecting");
                    _client.Close();
                    return; // Connection failed completely
                }
                
                switch (msg.MessageCase)
                {
                    case ServerMessage.MessageOneofCase.LoadPackage:
                        ConnectionState = ConnectionStateEnum.PackageInfoReceived;
                        LoadPackageHandler(msg.LoadPackage);
                        break;
                    case ServerMessage.MessageOneofCase.ClearPackage:
                        ClearPackageHandler(msg.ClearPackage);
                        break;
                    default:
                        throw new NotImplementedException();
                }
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
