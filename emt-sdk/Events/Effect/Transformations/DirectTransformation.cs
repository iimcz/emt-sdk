using emt_sdk.Packages;
using Naki3D.Common.Protocol;
using System;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class DirectTransformation : ITransformation
    {
        public string Name => "direct";

        /// <summary>
        /// Directly casts between numeric data types with rounding on floating point numbers.
        /// </summary>
        /// <returns>Directly cast numeric effect</returns>
        /// <exception cref="NotImplementedException">When attempting to use an unsupported rounding method</exception>
        /// <exception cref="InvalidCastException">When attempting to directly cast incompatible data types</exception>
        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (action.Type == TypeEnum.Float && message.DataCase == SensorDataMessage.DataOneofCase.Integer) return new EffectCall(action.Effect, (float)message.Integer);
            else if (action.Type == TypeEnum.Integer && message.DataCase == SensorDataMessage.DataOneofCase.Float)
            {
                switch (action.Mapping.Transform.RoundingMethod)
                {
                    case RoundingMethod.Down:
                        return new EffectCall(action.Effect, (int)Math.Floor(message.Float));
                    case RoundingMethod.Up:
                        return new EffectCall(action.Effect, (int)Math.Ceiling(message.Float));
                    default:
                        throw new NotImplementedException();
                }
            }
            else throw new InvalidCastException("Only types Float and Integer can be directly cast to each other");
        }
    }
}
