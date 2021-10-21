using emt_sdk.Communication;
using emt_sdk.ScenePackage;
using System;
using System.IO;
using System.Net.Sockets;
using emt_sdk.Extensions;
using emt_sdk.Events;

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

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server on port 5000");
            EventManager.Instance.OnEventReceived += Instance_OnEventReceived;
            EventManager.Instance.Start(new emt_sdk.Generated.ScenePackage.Sync
            {
                Elements = new System.Collections.Generic.List<emt_sdk.Generated.ScenePackage.Element>()
            });

            Console.ReadLine();
        }

        private static void Instance_OnEventReceived(object sender, Naki3D.Common.Protocol.SensorMessage e)
        {
            Console.WriteLine($"[{e.Timestamp}] ({e.SensorId}) - {e.DataCase}");
            if (e.DataCase == Naki3D.Common.Protocol.SensorMessage.DataOneofCase.Gesture)
            {
                Console.WriteLine(e.Gesture.Type);
            }
        }
    }
}
