using emt_sdk.Communication;
using emt_sdk.ScenePackage;
using System;
using System.IO;
using System.Net.Sockets;
using emt_sdk.Extensions;
using emt_sdk.Events;
using System.Threading.Tasks;

namespace emt_sdk_poc
{
    class Program
    {
        static void ContentManager()
        {
            var client = new TcpClient("127.0.0.1", 3917);
            Console.WriteLine("Socket connected");
            var connection = new ExhibitConnection(client)
            {
                ClearPackageHandler = pckg => { Console.WriteLine($"Clearing: {pckg}"); },
                LoadPackageHandler = pckg => {
                    var loader = new PackageLoader("package-schema.json");
                    Console.WriteLine($"Loading: {pckg}");
                    var package = loader.LoadPackage(new StringReader(pckg.DescriptorJson), false);
                    Console.WriteLine($"Downloading package: {package.PackagePackage.Url}");
                    package.DownloadFile(".");
                    Console.WriteLine("Checksum ok");
                }
            };

            connection.Connect();
            Console.WriteLine("Connection verified");
            if (!connection.Verified) throw new Exception();
            var descriptor = new Naki3D.Common.Protocol.DeviceDescriptor
            {
                PerformanceCap = Naki3D.Common.Protocol.PerformanceCap.Fast,
                Type = Naki3D.Common.Protocol.DeviceType.Ipw
            };
            descriptor.LocalSensors.Add(Naki3D.Common.Protocol.SensorType.Gesture);
            connection.SendDescriptor(descriptor);
            Console.WriteLine("Crypto info received, waiting for package");
        }

        static void EventServer()
        {
            Console.WriteLine("Starting server on port 5000");
            EventManager.Instance.OnEventReceived += Instance_OnEventReceived;
            EventManager.Instance.Start(new emt_sdk.Generated.ScenePackage.Sync
            {
                Elements = new System.Collections.Generic.List<emt_sdk.Generated.ScenePackage.Element>()
            });
        }

        private static void Instance_OnEventReceived(object sender, Naki3D.Common.Protocol.SensorMessage e)
        {
            Console.WriteLine($"[{e.Timestamp}] ({e.SensorId}) - {e.DataCase}");
            if (e.DataCase == Naki3D.Common.Protocol.SensorMessage.DataOneofCase.Gesture)
            {
                Console.WriteLine(e.Gesture.Type);
            }
        }

        static async Task Relay()
        {
            Console.WriteLine("(c)lient or  (s)erver?");
            var opt = Console.ReadKey(true).KeyChar;

            switch (opt)
            {
                case 'c':
                    var client = new EventRelayClient();
                    client.OnEventReceived += (s, e) =>
                    {
                        Console.WriteLine(e.DataCase);
                    };
                    client.Connect();
                    break;
                case 's':
                    var server = new EventRelayServer();
                    var serverTask = Task.Run(() => server.Listen());
                    while (!server.IsConnected) { await Task.Delay(100); }

                    while (true)
                    {
                        var key = Console.ReadKey(true).Key;
                        server.RelayLocalEvent(new Naki3D.Common.Protocol.SensorMessage
                        {
                            KeyboardUpdate = new Naki3D.Common.Protocol.KeyboardUpdateData
                            {
                                Keycode = ((int)key),
                                Type = Naki3D.Common.Protocol.KeyActionType.KeyDown
                            }
                        });
                    }
            }
        }

        static async Task Main(string[] args)
        {
            await Relay();

            Console.WriteLine("End of POC");
            Console.ReadLine();
        }
    }
}
