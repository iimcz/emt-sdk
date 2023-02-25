using Naki3D.Common.Protocol;
using System;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class RoundTransformation : ITransformation
    {
        public string Name => "round";

        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (action.Type != Packages.TypeEnum.Float || message.DataCase != SensorDataMessage.DataOneofCase.Float) throw new InvalidOperationException("Rounding can only be performed from float to float, to round to an integer use the direct transformation");

            switch (action.Mapping.Transform.RoundingMethod)
            {
                case Packages.RoundingMethod.Down:
                    return new EffectCall(action.Effect, (float) Math.Floor(message.Float));
                case Packages.RoundingMethod.Up:
                    return new EffectCall(action.Effect, (float)Math.Ceiling(message.Float));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
