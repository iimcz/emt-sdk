using emt_sdk.Communication;
using emt_sdk.ScenePackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using emt_sdk.Extensions;
using System.Threading.Tasks;
using emt_sdk.Scene;
using Naki3D.Common.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using emt_sdk.Generated.ScenePackage;
using emt_sdk.Settings;
using Action = emt_sdk.Generated.ScenePackage.Action;
using emt_sdk.Events.Local;
using emt_sdk.Events.Relay;
using emt_sdk.Events;
using NLog;

namespace emt_sdk_poc
{
    class Program
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static async Task ContentManager()
        {
            var settings = new CommunicationSettings
            {
                ContentHostname = "localhost",
                ContentPort = 3917
            };
            
            var descriptor = new DeviceDescriptor
            {
                PerformanceCap = PerformanceCap.Fast,
                Type = DeviceType.Ipw
            };
            descriptor.LocalSensors.Add(SensorType.Gesture);
            
            Console.WriteLine("Socket connected");
            var connection = new ExhibitConnection(settings, descriptor)
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
            while (!connection.Verified) await Task.Delay(500);
            Console.WriteLine("Crypto info received, waiting for package");
        }

        static void EventServer()
        {
            Console.WriteLine("Starting server on port 5000");
            EventManager.Instance.OnEventReceived += Instance_OnEventReceived;
            EventManager.Instance.OnEffectCalled += (call) =>
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

            var commsettings = new CommunicationSettings
            {
                SensorListenPort = 5000,
                SensorListenIp = "0.0.0.0",
                InterdeviceListenIp = "1.2.3.4"
            };

            EventManager.Instance.ConnectRemote(new Sync
            {
                Elements = new List<Element>
                {
                    new Element
                    {
                        Hostname = "pgebox"
                    }
                }
            }, commsettings);
            
            EventManager.Instance.ConnectSensor(commsettings);
        }

        private static void Instance_OnEventReceived(SensorMessage e)
        {
            Console.WriteLine($"[{e.Timestamp}] ({e.SensorId}) - {e.DataCase}");
            if (e.DataCase == SensorMessage.DataOneofCase.HandTracking)
            {
                Console.WriteLine(e.HandTracking.Gesture);
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
                    client.OnEventReceived += (e) =>
                    {
                        Console.WriteLine(e.DataCase);
                    };
                    client.Connect();
                    break;
                case 's':
                    var server = new EventRelayServer();
                    var serverTask = Task.Run(() => server.Listen());
                    EventManager.Instance.OnEventReceived += e => Logger.Info(e);
                    while (!server.IsConnected) { await Task.Delay(100); }

                    while (true)
                    {
                        var key = Console.ReadKey(true).Key;
                        server.RelayLocalEvent(new SensorMessage
                        {
                            KeyboardUpdate = new KeyboardUpdateData
                            {
                                Keycode = ((int)key),
                                Type = KeyActionType.KeyDown
                            }
                        });
                    }
            }
        }

        static async Task Main(string[] args)
        {
            Logger.Info("Testing NLog output");
            Logger.Info($"Current hostname: {System.Net.Dns.GetHostName()}");
            //var loader = new PackageLoader(null);
            //var packages = loader.EnumeratePackages(false);
            //Task.Run(ContentManager);

            //Task.Run(EventServer);
            await Relay();

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
