using emt_sdk.Events.Effect;
using emt_sdk.Events.Local;
using emt_sdk.Events.Remote;
using emt_sdk.Packages;
using emt_sdk.Settings;
using emt_sdk.Settings.EMT;
using Naki3D.Common.Protocol;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace emt_sdk.Events
{
    public class EventManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Handler for processing sensor data
        /// </summary>
        /// <param name="message"></param>
        public delegate void SensorDataMessageHandler(SensorDataMessage message);

        /// <summary>
        /// Handler for executing effects
        /// </summary>
        public delegate void EffectHandler(EffectCall e);

        /// <summary>
        /// Called whenever an event is received either locally, from other device or from a relay
        /// </summary>
        public event SensorDataMessageHandler OnEventReceived;

        /// <summary>
        /// Called whenever an effect is executed
        /// </summary>
        public event EffectHandler OnEffectCalled;

        private readonly InterdeviceEventRelay _interdeviceEventRelay;
        private readonly OutgoingEventConnection _outgoingEventConnection;

        private readonly ISensorManager _sensorManager;
        private readonly IConfigurationProvider<EMTSetting> _config;
        private readonly IConfigurationProvider<PackageDescriptor> _packageConfig;

        private bool _isHosting = false;

        public EventManager(ISensorManager sensorManager, IConfigurationProvider<EMTSetting> config, IConfigurationProvider<PackageDescriptor> packageConfig, InterdeviceEventRelay interdeviceEventRelay, OutgoingEventConnection outgoingEventConnection)
        {
            _sensorManager = sensorManager;
            _config = config;
            _packageConfig = packageConfig;
            _interdeviceEventRelay = interdeviceEventRelay;
            _outgoingEventConnection = outgoingEventConnection;
        }

        public InterdeviceEventRelay InterdeviceEventRelay
        {
            get
            {
                if (!ConnectedRemote) throw new InvalidOperationException("EventManager is not connected");

                if (_interdeviceEventRelay != null) return _interdeviceEventRelay;
                else throw new InvalidOperationException("This device is not an interdevice relay");
            }
        }
        public OutgoingEventConnection OutgoingEventConnection
        {
            get
            {
                if (!ConnectedRemote) throw new InvalidOperationException("EventManager is not connected");

                if (_outgoingEventConnection != null) return _outgoingEventConnection;
                else throw new InvalidOperationException("This device is not a client");
            }
        }

        public bool IsInterdeviceRelay { get; private set; }
        public bool ConnectedRemote { get; private set; } = false;
        public List<Packages.Action> Actions { get; } = new List<Packages.Action>();

        private bool _logEvents = false;
        private Task _relayTask;

        /// <summary>
        /// Subscribes to events of the local sensor server.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void ConnectSensor()
        {
            _logEvents = _config.Configuration.Communication.LogEvents;
            if (_logEvents) Logger.Debug("Sensor event logging is enabled");

            _sensorManager.OnMessage += HandleMessage;
            _sensorManager.OnMessage += HandleLocalMessage;

            // NOTE: this is now done explicitly in the Unity/client application.
            //Task.Run(() => _sensorManager.Start());
        }

        /// <summary>
        /// Connects to remote interdevice relay or hosts one depending on <paramref name="sync"/>. This method will not block the current thread.
        /// </summary>
        /// <param name="sync">emt_sdk device information</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void ConnectRemote()
        {
            if (string.IsNullOrWhiteSpace(_packageConfig.Configuration.Sync.RelayAddress))
            {
                Logger.Info("Sync is missing a relay - not starting.");
                return;
            }
            if (ConnectedRemote)
            {
                Logger.Error("Attempted to connect to remote more than once");
                throw new InvalidOperationException("EventManager is already connected to remote");
            }

            ConnectedRemote = true;

            // Check whether we're hosting
            _isHosting = _config.Configuration.Communication.InterdeviceListenIp == _packageConfig.Configuration.Sync.RelayAddress;
            if (_isHosting)
            {
                IsInterdeviceRelay = true;

                Logger.Info($"Interdevice listen IP matches package RelayAddress - this is the relay host");
                _interdeviceEventRelay.OnMessage += HandleMessage;

                _relayTask = Task.Run(() => _interdeviceEventRelay.Start());
            }
            else
            {
                IsInterdeviceRelay = false;

                Logger.Info($"Interdevice listen IP differs from package RelayAddress - this is a client");
                _outgoingEventConnection.OnMessage += HandleMessage;

                _relayTask = Task.Run(() => _outgoingEventConnection.Connect());
            }
        }

        public void BroadcastEvent(SensorDataMessage message)
        {
            if (!ConnectedRemote) {
                // This is not an error here, it just means we're operating alone.
                //Logger.Debug("EventManager is not connected to remote");
                return;
            }

            if (IsInterdeviceRelay) InterdeviceEventRelay.BroadcastSensorMessage(message);
            else OutgoingEventConnection.SendEvent(message);
        }

        public void HandleMessage(SensorDataMessage message)
        {
            if (_logEvents) Logger.Debug(message);

            OnEventReceived?.Invoke(message);

            var raisedEffects = Actions
                .Where(a => a.ShouldExecute(message))
                .Select(a => a.Transform(message))
                .Where(ec => ec != null); // Filter out failed ifs

            foreach (var raisedEffect in raisedEffects)
            {
                if (ShouldReport(raisedEffect.DataType))
                {
                    Logger.Debug($"Executing effect '{raisedEffect.Name}'");
                }
                OnEffectCalled?.Invoke(raisedEffect);
            }
        }

        private bool ShouldReport(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Void:
                case DataType.Bool:
                case DataType.Integer:
                case DataType.Float:
                case DataType.String:
                    return true;
                default:
                    return false;
            }
        }

        public void HandleLocalMessage(SensorDataMessage message)
        {
            // Relay all local events
            message.Path = $"/{Dns.GetHostName()}/{message.Path}";
            BroadcastEvent(message);
        }

        public void Dispose()
        {
            _sensorManager.OnMessage -= HandleLocalMessage;
            _sensorManager.OnMessage -= HandleMessage;

            if (ConnectedRemote)
            {
                ConnectedRemote = false;
                if (_isHosting)
                {
                    _interdeviceEventRelay.OnMessage -= HandleMessage;
                    _interdeviceEventRelay.Stop();
                }
                else
                {
                    _outgoingEventConnection.OnMessage -= HandleMessage;
                    _outgoingEventConnection.Disconnect();
                }

                // 2 second timeout 
                _relayTask.Wait(TimeSpan.FromSeconds(2));
            }
        }
    }
}
