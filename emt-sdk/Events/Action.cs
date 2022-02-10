using System;
using System.Collections.Generic;
using Naki3D.Common.Protocol;

namespace emt_sdk.Generated.ScenePackage
{
    public partial class Action
    {
        public void Execute()
        {
            
            // TODO: Something based on name
        }
        
        public bool ShouldExecute(SensorMessage message)
        {
            // TODO: Only simplified version
            if (!message.SensorId.EndsWith(Mapping.Source)) return false;
                
            switch (Type)
            {
                case TypeEnum.Event:
                    return message.DataCase == SensorMessage.DataOneofCase.Event && 
                           Mapping.EventName == message.Event.Name;
                case TypeEnum.Gesture:
                    return message.DataCase == SensorMessage.DataOneofCase.Gesture &&
                           Mapping.GestureName == message.Gesture.Type.ToString();
                case TypeEnum.Value:
                    return CompareValue(message);
                default:
                    throw new NotImplementedException();
            }
        }

        private readonly Dictionary<Condition, Func<int, int, bool>> _intComparisons =
            new Dictionary<Condition, Func<int, int, bool>>
            {
                {Condition.Above, (value, threshold) => value > threshold },
                {Condition.Below, (value, threshold) => value < threshold },
                {Condition.Equals, (value, threshold) => value - threshold <= float.Epsilon },
                {Condition.AboveOrEquals, (value, threshold) => value >= threshold },
                {Condition.BelowOrEquals, (value, threshold) => value <= threshold }
            };
        
        private readonly Dictionary<Condition, Func<float, float, bool>> _floatComparisons =
            new Dictionary<Condition, Func<float, float, bool>>
            {
                {Condition.Above, (value, threshold) => value > threshold },
                {Condition.Below, (value, threshold) => value < threshold },
                {Condition.Equals, (value, threshold) => value - threshold <= float.Epsilon },
                {Condition.AboveOrEquals, (value, threshold) => value >= threshold },
                {Condition.BelowOrEquals, (value, threshold) => value <= threshold }
            };

        private bool CompareValue(SensorMessage message)
        {
            if (!Enum.TryParse<SensorMessage.DataOneofCase>(Mapping.Source, out var type)) throw new NotSupportedException();
            if (message.DataCase != type || !Mapping.Condition.HasValue) return false;

            switch (Mapping.Threshold) // TODO: REGENERATE SCHEMA AND REPLACE
            {
                case "integer":
                    var iValue = IntValue(message);
                    var iThreshold = int.Parse(Mapping.Threshold);
                    return _intComparisons[Mapping.Condition.Value](iValue, iThreshold);
                case "float":
                    var fValue = FloatValue(message);
                    var fThreshold = float.Parse(Mapping.Threshold); 
                    return _floatComparisons[Mapping.Condition.Value](fValue, fThreshold);
                default:
                    throw new NotImplementedException();
            }
        }

        private float FloatValue(SensorMessage message)
        {
            switch (message.DataCase)
            {
                case SensorMessage.DataOneofCase.UltrasonicDistance:
                    return message.UltrasonicDistance.Distance;
                default:
                    throw new NotImplementedException();
            }
        }

        private int IntValue(SensorMessage message)
        {
            switch (message.DataCase)
            {
                case SensorMessage.DataOneofCase.LightLevel:
                    return (int) message.LightLevel.Level; // TODO: Maybe float?
                default:
                    throw new NotImplementedException();
            }
        }
    }
}