using Naki3D.Common.Protocol;
using System;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class MapTransformation : ITransformation
    {
        public string Name => "map";

        private bool IsNumeric(Packages.TypeEnum type) => type == Packages.TypeEnum.Float || type == Packages.TypeEnum.Integer;
        private bool IsNumeric(SensorDataMessage.DataOneofCase type) => type == SensorDataMessage.DataOneofCase.Float || type == SensorDataMessage.DataOneofCase.Integer;

        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (IsNumeric(action.Type) && IsNumeric(message.DataCase)) return TransformNumeric(action, message);
            else if (message.DataCase == SensorDataMessage.DataOneofCase.Bool) return TransformBool(action, message);
            else throw new InvalidOperationException($"Cannot map a non-numeric, non-boolean type '{message.DataCase}'");
        }

        private EffectCall TransformNumeric(Packages.Action action, SensorDataMessage message)
        {
            double source = message.DataCase == SensorDataMessage.DataOneofCase.Integer ? message.Integer : message.Float;
            var transform = action.Mapping.Transform;
            var value = Map(source, transform.InMin.Value.Double.Value, transform.InMax.Value.Double.Value, transform.OutMin.Value.Double.Value, transform.OutMax.Value.Double.Value);

            if (action.Type == Packages.TypeEnum.Integer) return new EffectCall(action.Effect, (int)value);
            else return new EffectCall(action.Effect, (float)value);
        }

        private EffectCall TransformBool(Packages.Action action, SensorDataMessage message)
        {
            var value = message.Bool;

            switch (action.Type)
            {
                case Packages.TypeEnum.Integer:
                    int intValue = (int)(value ? action.Mapping.Transform.OutMax.Value.Double.Value : action.Mapping.Transform.OutMin.Value.Double.Value);
                    return new EffectCall(action.Effect, intValue);
                case Packages.TypeEnum.Float:
                    float floatValue = (float)(value ? action.Mapping.Transform.OutMax.Value.Double.Value : action.Mapping.Transform.OutMin.Value.Double.Value);
                    return new EffectCall(action.Effect, floatValue);
                case Packages.TypeEnum.String:
                    string stringValue = value ? action.Mapping.Transform.OutMax.Value.String : action.Mapping.Transform.OutMin.Value.String;
                    return new EffectCall(action.Effect, stringValue);
                default:
                    throw new InvalidOperationException($"Cannot map a boolean to type '{action.Type}'");
            }
        }

        private double Map(double x, double inMin, double inMax, double outMin, double outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
}
