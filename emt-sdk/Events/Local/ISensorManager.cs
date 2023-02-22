using System.Collections.Generic;
using Naki3D.Common.Protocol;
using static emt_sdk.Events.EventManager;

namespace emt_sdk.Events.Local
{
    public interface ISensorManager
    {
        event SensorDataMessageHandler OnMessage;

        void BroadcastControlMessage(SensorControlMessage message);
        List<SensorDescriptor> GetSensorEndpoints(string filter = "");


        /// <summary>
        /// Starts listening for incoming sensor connections. This call will block the current thread.
        /// </summary>
        void Start();
    }
}
