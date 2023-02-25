using Naki3D.Common.Protocol;
using System;
using System.Globalization;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class ParseTransformation : ITransformation
    {
        public string Name => "parse";

        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (message.DataCase != SensorDataMessage.DataOneofCase.String) throw new InvalidOperationException("Cannot parse a non-string type");

            switch (action.Type)
            {
                case Packages.TypeEnum.Bool:
                    return new EffectCall(action.Effect, bool.Parse(message.String));
                case Packages.TypeEnum.Integer:
                    return new EffectCall(action.Effect, int.Parse(message.String, CultureInfo.InvariantCulture));
                case Packages.TypeEnum.Float:
                    return new EffectCall(action.Effect, float.Parse(message.String, CultureInfo.InvariantCulture));
                case Packages.TypeEnum.String:
                    return new EffectCall(action.Effect, bool.Parse(message.String));
                default:
                    throw new InvalidOperationException($"Cannot parse string to type '{action.Type}'");
            }
        }
    }
}
