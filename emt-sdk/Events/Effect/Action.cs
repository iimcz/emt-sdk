using System;
using System.Globalization;
using emt_sdk.Events.Effect;
using Naki3D.Common.Protocol;

namespace emt_sdk.Packages
{
    public partial class Action
    {
        public bool ShouldExecute(SensorDataMessage message) => message.Path == Mapping.Source;

        private bool IsTypeMatching(SensorDataMessage.DataOneofCase dataType)
        {
            switch (dataType)
            {
                case SensorDataMessage.DataOneofCase.None:
                    throw new InvalidOperationException("Empty sensor message does not match any action type");
                case SensorDataMessage.DataOneofCase.Void:
                    return Type == TypeEnum.Void;
                case SensorDataMessage.DataOneofCase.Bool:
                    return Type == TypeEnum.Bool;
                case SensorDataMessage.DataOneofCase.Integer:
                    return Type == TypeEnum.Integer;
                case SensorDataMessage.DataOneofCase.Float:
                    return Type == TypeEnum.Float;
                case SensorDataMessage.DataOneofCase.String:
                    return Type == TypeEnum.String;
                case SensorDataMessage.DataOneofCase.Vector2:
                    return Type == TypeEnum.Complex;
                case SensorDataMessage.DataOneofCase.Vector3:
                    return Type == TypeEnum.Complex;
                default:
                    throw new NotImplementedException();
            }
        }

        public EffectCall Transform(SensorDataMessage message)
        {
            if (Mapping.Transform == null)
            {
                if (IsTypeMatching(message.DataCase)) return Passthrough(message);
                else throw new InvalidOperationException($"Cannot passthrough message of type '{message.DataCase}' into an Action requiring type '{Type}'");
            }

            switch (Mapping.Transform.Type) 
            {
                case "constant":
                    return TransformConstant(message);
                case "discard":
                    return Discard();
            }
        }

        private EffectCall Discard()
        {
            if (Type != TypeEnum.Void) throw new InvalidOperationException("Discard can only output void, maybe you meant to use a constant?");
            return new EffectCall(Effect);
        }

        private EffectCall Passthrough(SensorDataMessage message)
        {
            switch (message.DataCase)
            {
                case SensorDataMessage.DataOneofCase.None:
                    throw new InvalidOperationException("Received an empty (not void) message from sensor, this is not a valid Action input");
                case SensorDataMessage.DataOneofCase.Void:
                    return new EffectCall(Effect);
                case SensorDataMessage.DataOneofCase.Bool:
                    return new EffectCall(Effect, message.Bool);
                case SensorDataMessage.DataOneofCase.Integer:
                    return new EffectCall(Effect, message.Integer);
                case SensorDataMessage.DataOneofCase.Float:
                    return new EffectCall(Effect, message.Float);
                case SensorDataMessage.DataOneofCase.String:
                    return new EffectCall(Effect, message.String);
                case SensorDataMessage.DataOneofCase.Vector2:
                    return new EffectCall(Effect, message.Vector2);
                case SensorDataMessage.DataOneofCase.Vector3:
                    return new EffectCall(Effect, message.Vector3);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Transforms a void data message to a constant of any non-complex type
        /// </summary>
        /// <returns>Transformed effect</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to use a complex constant</exception>
        /// <exception cref="NotImplementedException">Thrown when transforming to a constant of unsupported type</exception>
        private EffectCall TransformConstant(SensorDataMessage message)
        {
            if (message.DataCase != SensorDataMessage.DataOneofCase.Void) throw new InvalidOperationException("Cannot transform a non-void message into a constant");

            switch (Type)
            {
                case TypeEnum.Bool:
                    return new EffectCall(Effect, bool.Parse(Mapping.Transform.Value));
                case TypeEnum.Complex:
                    throw new InvalidOperationException("Cannot define constant complex");
                case TypeEnum.Float:
                    return new EffectCall(Effect, float.Parse(Mapping.Transform.Value, CultureInfo.InvariantCulture));
                case TypeEnum.Integer:
                    return new EffectCall(Effect, int.Parse(Mapping.Transform.Value, CultureInfo.InvariantCulture));
                case TypeEnum.String:
                    return new EffectCall(Effect, Mapping.Transform.Value);
                 default:
                    throw new NotImplementedException();
            }
        }

        /*
        private readonly Dictionary<Condition, Func<int, int, bool>> _intComparisons =
            new Dictionary<Condition, Func<int, int, bool>>
            {
                {Condition.Above, (value, threshold) => value > threshold },
                {Condition.Below, (value, threshold) => value < threshold },
                {Condition.Equals, (value, threshold) => value == threshold },
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
        */
    }
}