namespace Hexa.NET.DebugDraw
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct StyleVarValue
    {
        [FieldOffset(0)] public StyleVarType Type;

        [FieldOffset(2)] public bool BoolValue;
        [FieldOffset(2)] public float FloatValue;
        [FieldOffset(2)] public Vector2 Vector2Value;
        [FieldOffset(2)] public Vector3 Vector3Value;
        [FieldOffset(2)] public Vector4 Vector4Value;

        public StyleVarValue(bool value)
        {
            BoolValue = value;
            Type = StyleVarType.Bool;
        }

        public StyleVarValue(float value)
        {
            FloatValue = value;
            Type = StyleVarType.Float;
        }

        public StyleVarValue(Vector2 value)
        {
            Vector2Value = value;
            Type = StyleVarType.Vector2;
        }

        public StyleVarValue(Vector3 value)
        {
            Vector3Value = value;
            Type = StyleVarType.Vector3;
        }

        public StyleVarValue(Vector4 value)
        {
            Vector4Value = value;
            Type = StyleVarType.Vector4;
        }

        public static StyleVarValue From<T>(T value)
        {
            if (value is bool b)
            {
                return new(b);
            }
            if (value is float f)
            {
                return new(f);
            }
            if (value is Vector2 v2)
            {
                return new(v2);
            }
            if (value is Vector3 v3)
            {
                return new(v3);
            }
            if (value is Vector4 v4)
            {
                return new(v4);
            }
            throw new InvalidCastException();
        }
    }
}