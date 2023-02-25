using emt_sdk.Packages;
using Naki3D.Common.Protocol;
using System;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class DiscardTransformation : ITransformation
    {
        public string Name => "discard";

        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (action.Type != TypeEnum.Void) throw new InvalidOperationException("Discard can only output void, maybe you meant to use a constant?");
            return new EffectCall(action.Effect);
        }
    }
}
