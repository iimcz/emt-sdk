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

        /// <summary>
        /// Stops listening to connections (at some point in the near future) and unblocks the thread blocked by Start.
        /// </summary>
        void Stop();
    }
}
