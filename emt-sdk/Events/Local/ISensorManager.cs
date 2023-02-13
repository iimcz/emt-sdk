using static emt_sdk.Events.EventManager;

namespace emt_sdk.Events.Local
{
    public interface ISensorManager
    {
        event SensorMessageHandler OnMessage;
    }
}
