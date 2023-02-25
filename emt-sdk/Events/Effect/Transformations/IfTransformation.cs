using Naki3D.Common.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace emt_sdk.Events.Effect.Transformations
{
    internal class IfTransformation : ITransformation
    {
        public string Name => "if";

        private static Dictionary<string, Func<string, string, bool>> _stringComparators = new Dictionary<string, Func<string, string, bool>>
        {
            { "startsWith", (value, comp) => value.StartsWith(comp) },
            { "endsWith", (value, comp) => value.EndsWith(comp) },
            { "equals", (value, comp) => value.Equals(comp) },
            { "contains", (value, comp) => value.Contains(comp) }
        };

        public EffectCall Transform(Packages.Action action, SensorDataMessage message)
        {
            if (message.DataCase == SensorDataMessage.DataOneofCase.String) return IfString(action, message);

            // TODO: Finish
            throw new NotImplementedException();
        }

        private EffectCall IfString(Packages.Action action, SensorDataMessage message)
        {
            var comparator = _stringComparators.FirstOrDefault(sc => sc.Key == action.Mapping.Transform.ComparisonType);
            if (comparator.Key == null) throw new InvalidOperationException($"Comparator '{action.Mapping.Transform.ComparisonType}' is not defined for strings");

            var result = comparator.Value(message.String, action.Mapping.Transform.ComparisonValue.Value.String);
            if (!result) return null;

            // TODO: Finish
            throw new NotImplementedException();
        }
    }
}
