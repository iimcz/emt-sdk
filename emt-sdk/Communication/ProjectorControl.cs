using emt_sdk.Events.Local;
using Naki3D.Common.Protocol;

namespace emt_sdk.Communication
{
    public class ProjectorControl
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SensorManager _sensorManager;
        
        public ProjectorControl(SensorManager sensorManager)
        {
            _sensorManager = sensorManager;
        }

        public void PowerOn()
        {
            _sensorManager.BroadcastControlMessage(new SensorControlMessage
            {
                CecMessage = new CECMessage
                {
                    Action = CECAction.PowerOn
                }
            });

            Logger.Info("Sent power on CEC messages");
        }

        public void PowerOff()
        {
            _sensorManager.BroadcastControlMessage(new SensorControlMessage
            {
                CecMessage = new CECMessage
                {
                    Action = CECAction.PowerOff
                }
            });

            Logger.Info("Sent power off CEC messages");
        }
    }
}