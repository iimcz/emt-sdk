using Naki3D.Common.Protocol;
using System;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class NegateTransformation : ITransformation
    {
        public string Name => "negate";

        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (action.Type != Packages.TypeEnum.Bool || message.DataCase != SensorDataMessage.DataOneofCase.Bool) throw new InvalidOperationException("Negation is a bool -> bool operation only");

            return new EffectCall(action.Effect, !message.Bool);
        }
    }
}
