using System.Collections.Generic;
using Naki3D.Common.Protocol;
using static emt_sdk.Events.EventManager;

namespace emt_sdk.Events.Local
{
    public interface ISensorManager
    {
        event SensorMessageHandler OnMessage;

        List<SensorDescriptor> GetSensorEndpoints(string filter = "");
    }
}
