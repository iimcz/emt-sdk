using emt_sdk.Events.Local;
using emt_sdk.Events.Remote;
using emt_sdk.Generated.ScenePackage;
using emt_sdk.Settings;
using Naki3D.Common.Protocol;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Action = emt_sdk.Generated.ScenePackage.Action;

namespace emt_sdk.Events
{
    public class EventManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Handler for processing sensor data
        /// </summary>
        /// <param name="message"></param>
        public delegate void SensorMessageHandler(SensorMessage message);

        /// <summary>
        /// Handler for executing effects
        /// </summary>
        public delegate void EffectHandler(EffectCall e);

        /// <summary>
        /// Called whenever an event is received either locally, from other device or from a relay
        /// </summary>
        public event SensorMessageHandler OnEventReceived;

        /// <summary>
        /// Called whenever an effect is executed
        /// </summary>
        public event EffectHandler OnEffectCalled;

        // Singleton
        public static EventManager Instance { get; } = new EventManager();
        private EventManager() { }

        private InterdeviceEventRelay _interdeviceEventRelay;
        private OutgoingEventConnection _outgoingEventConnection;

        public SensorManager SensorManager { get; private set; }
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
        public List<Action> Actions { get; } = new List<Action>();

        private bool _logEvents = false;

        /// <summary>
        /// Hosts a local sensor server. This method will not block the current thread.
        /// </summary>
        /// <param name="settings"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void ConnectSensor(CommunicationSettings settings)
        {
            if (SensorManager != null)
            {
                Logger.Error("Attempted to connect to sensors more than once");
                throw new InvalidOperationException("EventManager is already connected to sensors");
            }

            _logEvents = settings.LogEvents;
            if (_logEvents) Logger.Debug("Sensor event logging is enabled");

            SensorManager = new SensorManager(settings);
            SensorManager.OnMessage += HandleMessage;
            SensorManager.OnMessage += HandleLocalMessage;

            Task.Run(() => SensorManager.Start());
        }

        /// <summary>
        /// Connects to remote interdevice relay or hosts one depending on <paramref name="sync"/>. This method will not block the current thread.
        /// </summary>
        /// <param name="sync">emt_sdk device information</param>
        /// <param name="settings">Network settings</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void ConnectRemote(Sync sync, CommunicationSettings settings)
        {
            if (_interdeviceEventRelay != null || _outgoingEventConnection != null)
            {
                Logger.Error("Attempted to connect to remote more than once");
                throw new InvalidOperationException("EventManager is already connected to remote");
            }

            ConnectedRemote = true;

            // Check whether we're hosting
            var selfIndex = sync.Elements
                .FirstOrDefault(e => e.Hostname.ToLowerInvariant() == Dns.GetHostName().ToLowerInvariant());

            if (selfIndex == null)
            {
                Logger.Error($"Could not find current hostname ({Dns.GetHostName()}) in sync data! Assuming this isn't host!");
            }

            if (selfIndex == sync.Elements.First())
            {
                IsInterdeviceRelay = true;

                Logger.Info($"Device is first in Sync.Elements, starting InterdeviceEventRelay");
                _interdeviceEventRelay = new InterdeviceEventRelay(settings);
                _interdeviceEventRelay.OnMessage += HandleMessage;

                Task.Run(() => _interdeviceEventRelay.Start());
            }
            else
            {
                IsInterdeviceRelay = false;

                Logger.Info($"Device is secondary in Sync.Elements, starting OutgoingEventConnection");
                _outgoingEventConnection = new OutgoingEventConnection(sync, settings);
                _outgoingEventConnection.OnMessage += HandleMessage;

                Task.Run(() => _outgoingEventConnection.Connect());
            }
        }

        public void BroadcastEvent(SensorMessage message)
        {
            if (!ConnectedRemote) {
                // This is not an error here, it just means we're operating alone.
                Logger.Debug("EventManager is not connected to remote");
                return;
            }

            if (IsInterdeviceRelay) InterdeviceEventRelay.BroadcastSensorMessage(message);
            else OutgoingEventConnection.SendEvent(message);
        }

        private void HandleMessage(SensorMessage message)
        {
            if (_logEvents) Logger.Debug(message);

            OnEventReceived?.Invoke(message);

            var raisedEffects = Actions
                .Where(a => a.ShouldExecute(message))
                .Select(a => new EffectCall
                {
                    Name = a.Effect,
                    Value = a.MapValue(message)
                });

            foreach (var raisedEffect in raisedEffects)
            {
                Logger.Debug($"Executing effect '{raisedEffect.Name}'");
                OnEffectCalled?.Invoke(raisedEffect);
            }
        }

        public void HandleLocalMessage(SensorMessage message)
        {
            // Relay all local events
            message.SensorId = $"/{Dns.GetHostName()}/{message.SensorId}";
            BroadcastEvent(message);
        }
    }
}
