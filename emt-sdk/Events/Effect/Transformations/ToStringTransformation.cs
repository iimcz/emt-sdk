using Naki3D.Common.Protocol;
using System;
using System.Globalization;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class ToStringTransformation : ITransformation
    {
        public string Name => "toString";

        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (action.Type != Packages.TypeEnum.String) throw new InvalidOperationException("Cannot convert to string if the output type is not a string");

            switch (message.DataCase)
            {
                case SensorDataMessage.DataOneofCase.None:
                    return new EffectCall(action.Effect, string.Empty);
                case SensorDataMessage.DataOneofCase.Void:
                    return new EffectCall(action.Effect, "void");
                case SensorDataMessage.DataOneofCase.Bool:
                    return new EffectCall(action.Effect, message.Bool.ToString(CultureInfo.InvariantCulture));
                case SensorDataMessage.DataOneofCase.Integer:
                    return new EffectCall(action.Effect, message.Integer.ToString(CultureInfo.InvariantCulture));
                case SensorDataMessage.DataOneofCase.Float:
                    return new EffectCall(action.Effect, message.Float.ToString(CultureInfo.InvariantCulture));
                case SensorDataMessage.DataOneofCase.String:
                    return new EffectCall(action.Effect, message.String);
                case SensorDataMessage.DataOneofCase.Vector2:
                    return new EffectCall(action.Effect, $"[{message.Vector2.X},{message.Vector2.Y}]");
                case SensorDataMessage.DataOneofCase.Vector3:
                    return new EffectCall(action.Effect, $"[{message.Vector3.X},{message.Vector3.Y},{message.Vector3.Z}]");
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
