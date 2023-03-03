using System;
using System.Collections.Generic;
using System.Linq;
using emt_sdk.Events.Effect;
using emt_sdk.Events.Effect.Transformations;
using Naki3D.Common.Protocol;
using NLog;

namespace emt_sdk.Packages
{
    public partial class Action
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public bool ShouldExecute(SensorDataMessage message) => message.Path == Mapping.Source;

        private static List<ITransformation> _transformations = new List<ITransformation>() {
            new ConstantTransformation(),
            new DirectTransformation(),
            new DiscardTransformation(),
            new IfTransformation(),
            new MapTransformation(),
            new NegateTransformation(),
            new ParseTransformation(),
            new PassthroughTransformation(),
            new RoundTransformation(),
            new ToStringTransformation()
        };

        public EffectCall Transform(SensorDataMessage message)
        {
            var transformationType = Mapping?.Transform?.Type ?? "passthrough";
            var transformation = _transformations.FirstOrDefault(t => t.Name == transformationType);

            if (transformation == null)
            {
                // TODO: consider if throwing an exception was better
                //throw new NotImplementedException($"Unsupported transform operation '{transformationType}'");
                Logger.Warn($"Unsupported transform operation requested: '{transformationType}'");
                return null;
            }
            return transformation.Transform(this, message);
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