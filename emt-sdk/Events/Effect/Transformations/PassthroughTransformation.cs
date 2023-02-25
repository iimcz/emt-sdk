using emt_sdk.Packages;
using Naki3D.Common.Protocol;
using System;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class PassthroughTransformation : ITransformation
    {
        public string Name => "passthrough";

        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (!IsTypeMatching(action, message.DataCase))
            {
                throw new InvalidOperationException($"Cannot passthrough message of type '{message.DataCase}' into an Action requiring type '{action.Type}'");
            }

            switch (message.DataCase)
            {
                case SensorDataMessage.DataOneofCase.None:
                    throw new InvalidOperationException("Received an empty (not void) message from sensor, this is not a valid Action input");
                case SensorDataMessage.DataOneofCase.Void:
                    return new EffectCall(action.Effect);
                case SensorDataMessage.DataOneofCase.Bool:
                    return new EffectCall(action.Effect, message.Bool);
                case SensorDataMessage.DataOneofCase.Integer:
                    return new EffectCall(action.Effect, message.Integer);
                case SensorDataMessage.DataOneofCase.Float:
                    return new EffectCall(action.Effect, message.Float);
                case SensorDataMessage.DataOneofCase.String:
                    return new EffectCall(action.Effect, message.String);
                case SensorDataMessage.DataOneofCase.Vector2:
                    return new EffectCall(action.Effect, message.Vector2);
                case SensorDataMessage.DataOneofCase.Vector3:
                    return new EffectCall(action.Effect, message.Vector3);
                default:
                    throw new NotImplementedException();
            }
        }

        private bool IsTypeMatching(Packages.Action action, SensorDataMessage.DataOneofCase dataType)
        {
            switch (dataType)
            {
                case SensorDataMessage.DataOneofCase.None:
                    throw new InvalidOperationException("Empty sensor message does not match any action type");
                case SensorDataMessage.DataOneofCase.Void:
                    return action.Type == TypeEnum.Void;
                case SensorDataMessage.DataOneofCase.Bool:
                    return action.Type == TypeEnum.Bool;
                case SensorDataMessage.DataOneofCase.Integer:
                    return action.Type == TypeEnum.Integer;
                case SensorDataMessage.DataOneofCase.Float:
                    return action.Type == TypeEnum.Float;
                case SensorDataMessage.DataOneofCase.String:
                    return action.Type == TypeEnum.String;
                case SensorDataMessage.DataOneofCase.Vector2:
                    return action.Type == TypeEnum.Complex;
                case SensorDataMessage.DataOneofCase.Vector3:
                    return action.Type == TypeEnum.Complex;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
