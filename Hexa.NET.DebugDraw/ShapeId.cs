#nullable disable

namespace Hexa.NET.DebugDraw
{
    public struct ShapeId : IEquatable<ShapeId>
    {
        public long Value;

        public ShapeId(long value)
        {
            Value = value;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is ShapeId id && Equals(id);
        }

        public readonly bool Equals(ShapeId other)
        {
            return Value == other.Value;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(ShapeId left, ShapeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShapeId left, ShapeId right)
        {
            return !(left == right);
        }

        public static implicit operator ShapeId(long id)
        {
            return new ShapeId(id);
        }

        public static implicit operator long(ShapeId id)
        {
            return id.Value;
        }
    }
}