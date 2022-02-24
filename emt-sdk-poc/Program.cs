using emt_sdk.Communication;
using emt_sdk.ScenePackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using emt_sdk.Extensions;
using emt_sdk.Events;
using System.Threading.Tasks;
using emt_sdk.Scene;
using Naki3D.Common.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using emt_sdk.Generated.ScenePackage;
using Action = emt_sdk.Generated.ScenePackage.Action;

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
                    Console.WriteLine($"Downloading package: {package.Package.Url}");
                    package.DownloadFile();
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
            EventManager.Instance.OnEffectCalled += (sender, call) =>
            {
                Console.WriteLine($"[CALL] {call.Name}: {call.Value?.ToString() ?? "void"}");
            };
            
            EventManager.Instance.Actions.Add(new Action
            {
                Effect = "test",
                Type = TypeEnum.Event,
                Mapping = new Mapping
                {
                    Source = "1",
                    EventName = "GestureSwipeDown"
                }
            });
            
            EventManager.Instance.Actions.Add(new Action
            {
                Effect = "moveStart",
                Type = TypeEnum.ValueTrigger,
                Mapping = new Mapping
                {
                    Source = "atom_1_pir_1",
                    Condition = Condition.Equals,
                    ThresholdType = ThresholdType.Integer,
                    Threshold = ((int)PirMovementEvent.MovementStarted).ToString()
                }
            });
            
            EventManager.Instance.Actions.Add(new Action
            {
                Effect = "moveAny",
                Type = TypeEnum.ValueTrigger,
                Mapping = new Mapping
                {
                    Source = "atom_1_pir_1",
                    Condition = Condition.AboveOrEquals,
                    ThresholdType = ThresholdType.Integer,
                    Threshold = "0"
                }
            });
            
            EventManager.Instance.Actions.Add(new Action
            {
                Effect = "distance",
                Type = TypeEnum.Value,
                Mapping = new Mapping
                {
                    Source = "raspi-1-ultrasonic-1",
                    InMin = 0,
                    InMax = 60,
                    OutMin = 0,
                    OutMax = 1,
                }
            });
            
            EventManager.Instance.Start(new Sync
            {
                Elements = new List<Element>()
            });
        }

        private static void Instance_OnEventReceived(object sender, Naki3D.Common.Protocol.SensorMessage e)
        {
            Console.WriteLine($"[{e.Timestamp}] ({e.SensorId}) - {e.DataCase}");
            if (e.DataCase == SensorMessage.DataOneofCase.Gesture)
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
            //var loader = new PackageLoader(null);
            //var packages = loader.EnumeratePackages(false);
            //Task.Run(ContentManager);

            Task.Run(EventServer);
            //await Relay();

            /*
            while (true)
            {
                switch (Console.ReadKey().KeyChar.ToString())
                {
                    case "p":
                        EventManager.Instance.ProjectorControl.PowerOn();
                        break;
                    case "s":
                        EventManager.Instance.ProjectorControl.PowerOff();
                        break;
                }

                Console.WriteLine("");
            }
            */

            Console.WriteLine("End of POC");
            Console.ReadLine();
        }
    }
}
