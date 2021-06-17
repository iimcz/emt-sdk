using emt_sdk.Extensions;
using Naki3D.Common.Protocol;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace emt_sdk.Communication
{
    public class ExhibitConnection : IDisposable
    {
        private readonly double TIMEOUT_INTERVAL = 25_000;

        private readonly TcpClient _client;
        private readonly Stream _stream;
        private readonly JsonObjectStringReader _jsonReader;
        private readonly Timer _timeoutTimer;
        private readonly string _id;

        public bool IsConnected => _client.Connected;
        public bool Verified { get; private set; } = false;
        public EncryptionInfo EncryptionInfo { get; private set; } = null;
        public Action<LoadPackage> LoadPackageHandler { get; set; }
        public Action<ClearPackage> ClearPackageHandler { get; set; }

        public ExhibitConnection(TcpClient client, string id = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = _client.GetStream();
            _jsonReader = new JsonObjectStringReader(_stream);

            _timeoutTimer = new Timer(TIMEOUT_INTERVAL);
            _timeoutTimer.Elapsed += TimeoutElapsed;

            _id = id ?? Dns.GetHostName();
        }

        private void TimeoutElapsed(object sender, ElapsedEventArgs e)
        {
            var ping = new DeviceMessage
            {
                ConnectionId = _id,
                Ping = new Ping()
            };

            ping.WriteJsonTo(_stream);
        }

        public void Connect()
        {
            CompareVersions();
            RequestConnection();
            _timeoutTimer.Start();

            WaitForVerification();
        }

        public void SendDescriptor(DeviceDescriptor descriptor)
        {
            var msg = new DeviceMessage
            {
                ConnectionId = _id,
                DeviceDescriptor = descriptor
            };
            msg.WriteJsonTo(_stream);

            // TODO: Server actually sends no info
            //EncryptionInfo = EncryptionInfo.Parser.ParseJson(_jsonReader.NextJsonObject());
            Task.Run(HandlePackages);
        }

        private void WaitForVerification()
        {
            var ack = ConnectionAcknowledgement.Parser.ParseJson(_jsonReader.NextJsonObject());
            Verified = ack.Verified;

            if (!Verified) throw new Exception();
        }

        private void RequestConnection()
        {
            var request = new ConnectionRequest
            {
                ConnectionId = _id,
                PublicKey = Google.Protobuf.ByteString.CopyFromUtf8("") // TODO: Add key support
            };

            request.WriteJsonTo(_stream);
            var ack = ConnectionAcknowledgement.Parser.ParseJson(_jsonReader.NextJsonObject());
            Verified = ack.Verified;
            // TODO: Do we need to check hostname? Probably not
        }

        private void CompareVersions()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var version = assemblyVersion.ToVersionInfo();

            version.WriteJsonTo(_stream);
            var serverInfo = VersionInfo.Parser.ParseJson(_jsonReader.NextJsonObject());
            if (!IsVersionCompatible(serverInfo)) throw new Exception();
        }

        private bool IsVersionCompatible(VersionInfo verInfo)
        {
            return verInfo != null;
        }

        private void HandlePackages()
        {
            while (IsConnected)
            {
                var msg = ServerMessage.Parser.ParseJson(_jsonReader.NextJsonObject());
                switch (msg.MessageCase)
                {
                    case ServerMessage.MessageOneofCase.LoadPackage:
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
        }
    }
}
