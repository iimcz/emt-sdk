using emt_sdk.Packages;
using Naki3D.Common.Protocol;

namespace emt_sdk.Events.Effect.Transformations
{
    internal interface ITransformation
    {
        string Name { get; }
        EffectCall Transform(Action action, SensorDataMessage message);
    }
}
