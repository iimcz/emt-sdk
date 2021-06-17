using emt_sdk.Communication;
using emt_sdk.ScenePackage;
using System;
using System.IO;
using System.Net.Sockets;
using emt_sdk.Extensions;

namespace emt_sdk_poc
{
    class Program
    {
        static void Main(string[] args)
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

            Console.ReadLine();
        }
    }
}
