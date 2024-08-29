#nullable disable

namespace Hexa.NET.DebugDraw
{
    using System.Collections.Concurrent;

    public static class ShapeRegistry
    {
        private static long index;
        private readonly static ConcurrentDictionary<string, ShapeId> nameToId = [];
        private readonly static ConcurrentDictionary<ShapeId, string> idToName = [];

        public static ShapeId Register(string name)
        {
            ShapeId id = Interlocked.Increment(ref index);
            nameToId.TryAdd(name, id);
            idToName.TryAdd(id, name);
            return id;
        }

        public static ShapeId GetByName(string name)
        {
            return nameToId[name];
        }

        public static string GetName(ShapeId id)
        {
            return idToName[id];
        }
    }
}