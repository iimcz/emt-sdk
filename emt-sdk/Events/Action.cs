using System;
using System.Collections.Generic;
using Naki3D.Common.Protocol;

namespace emt_sdk.Generated.ScenePackage
{
    public partial class Action
    {
        public double? MapValue(SensorMessage message)
        {
            if (!ShouldExecute(message)) return null;
            if (Type != TypeEnum.Value) return null;

            var value = FloatValue(message) ?? IntValue(message) ?? throw new NotImplementedException();
            var clamped = Clamp(value, Mapping.InMin ?? double.MinValue, Mapping.InMax ?? double.MaxValue);

            return Map(
                clamped,
                Mapping.InMin ?? double.MinValue,
                Mapping.InMax ?? double.MaxValue,
                Mapping.OutMin ?? double.MinValue,
                Mapping.OutMax ?? double.MaxValue);
        }
        
        public bool ShouldExecute(SensorMessage message)
        {
            // TODO: Only simplified version
            if (!message.SensorId.EndsWith(Mapping.Source)) return false;
                
            switch (Type)
            {
                case TypeEnum.Event:
                    string eventName;
                    switch (message.DataCase)
                    {
                        case SensorMessage.DataOneofCase.Event:
                            eventName = message.Event.Name;
                            break;
                        case SensorMessage.DataOneofCase.Gesture:
                            eventName = message.Gesture.Type.ToString();
                            break;
                        default:
                            return false;
                    }
                    
                    return string.Equals(Mapping.EventName, eventName, StringComparison.CurrentCultureIgnoreCase);
                case TypeEnum.Value:
                    return true;
                case TypeEnum.ValueTrigger:
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
            if (!Mapping.Condition.HasValue) return false;

            switch (Mapping.ThresholdType)
            {
                case ThresholdType.Integer:
                    var iValue = IntValue(message) ?? throw new NotImplementedException();
                    var iThreshold = int.Parse(Mapping.Threshold);
                    return _intComparisons[Mapping.Condition.Value](iValue, iThreshold);
                case ThresholdType.Float:
                    var fValue = FloatValue(message) ?? throw new NotImplementedException();
                    var fThreshold = float.Parse(Mapping.Threshold); 
                    return _floatComparisons[Mapping.Condition.Value](fValue, fThreshold);
                default:
                    throw new NotImplementedException();
            }
        }

        private float? FloatValue(SensorMessage message)
        {
            switch (message.DataCase)
            {
                case SensorMessage.DataOneofCase.UltrasonicDistance:
                    return message.UltrasonicDistance.Distance;
                case SensorMessage.DataOneofCase.LightLevel:
                    return message.LightLevel.Level;
                default:
                    return null;
            }
        }

        private int? IntValue(SensorMessage message)
        {
            switch (message.DataCase)
            {
                case SensorMessage.DataOneofCase.PirMovement:
                    return (int) message.PirMovement.Event;
                default:
                    return null;
            }
        }
        
        private double Map(double x, double inMin, double inMax, double outMin, double outMax) {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
        
        private static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;
            
            return val;
        }
    }
}