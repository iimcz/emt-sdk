using emt_sdk.Packages;
using Naki3D.Common.Protocol;
using System;
using System.Globalization;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class ConstantTransformation : ITransformation
    {
        public string Name => "constant";

        /// <summary>
        /// Transforms a void data message to a constant of any non-complex type
        /// </summary>
        /// <returns>Transformed effect</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to use a complex constant</exception>
        /// <exception cref="NotImplementedException">Thrown when transforming to a constant of unsupported type</exception>
        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (message.DataCase != SensorDataMessage.DataOneofCase.Void) throw new InvalidOperationException("Cannot transform a non-void message into a constant");

            switch (action.Type)
            {
                case TypeEnum.Bool:
                    return new EffectCall(action.Effect, bool.Parse(action.Mapping.Transform.Value));
                case TypeEnum.Complex:
                    throw new InvalidOperationException("Cannot define constant complex");
                case TypeEnum.Float:
                    return new EffectCall(action.Effect, float.Parse(action.Mapping.Transform.Value, CultureInfo.InvariantCulture));
                case TypeEnum.Integer:
                    return new EffectCall(action.Effect, int.Parse(action.Mapping.Transform.Value, CultureInfo.InvariantCulture));
                case TypeEnum.String:
                    return new EffectCall(action.Effect, action.Mapping.Transform.Value);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
