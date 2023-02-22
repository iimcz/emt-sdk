using Naki3D.Common.Protocol;
using System;

namespace emt_sdk.Events.Effect
{
    public class EffectCall
    {
        private readonly bool _bool;
        private readonly int _integer;
        private readonly float _float;
        private readonly string _string;
        private readonly Vector2Data _vector2;
        private readonly Vector3Data _vector3;

        public string Name { get; }
        public DataType DataType { get; }

        public bool Bool { get { return DataType == DataType.Bool ? _bool : throw new InvalidOperationException($"DataType '{DataType}' does not match 'Bool'"); } }
        public int Integer { get { return DataType == DataType.Integer ? _integer : throw new InvalidOperationException($"DataType '{DataType}' does not match 'Integer'"); } }
        public float Float { get { return DataType == DataType.Float ? _float : throw new InvalidOperationException($"DataType '{DataType}' does not match 'Float'"); } }
        public string String { get { return DataType == DataType.String ? _string : throw new InvalidOperationException($"DataType '{DataType}' does not match 'String'"); } }
        public Vector2Data Vector2 { get { return DataType == DataType.Vector2 ? _vector2 : throw new InvalidOperationException($"DataType '{DataType}' does not match 'Vector2'"); } }
        public Vector3Data Vector3 { get { return DataType == DataType.Vector3 ? _vector3 : throw new InvalidOperationException($"DataType '{DataType}' does not match 'Vector3'"); } }

        public EffectCall(string name) { Name = name; DataType = DataType.Void; }
        public EffectCall(string name, bool boolean) { Name = name; DataType = DataType.Bool; _bool = boolean; }
        public EffectCall(string name, int integer) { Name = name; DataType = DataType.Integer; _integer = integer; }
        public EffectCall(string name, float flt) { Name = name; DataType = DataType.Float; _float = flt; }
        public EffectCall(string name, string str) { Name = name; DataType = DataType.String; _string = str; }
        public EffectCall(string name, Vector2Data vector2) { Name = name; DataType = DataType.Vector2; _vector2 = vector2; }
        public EffectCall(string name, Vector3Data vector3) { Name = name; DataType = DataType.Vector3; _vector3 = vector3; }
    }
}