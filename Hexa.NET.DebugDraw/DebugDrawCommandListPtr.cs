namespace Hexa.NET.DebugDraw
{
    using Hexa.NET.Utilities;
    using System;
    using System.Numerics;

    public unsafe struct DebugDrawCommandListPtr : IEquatable<DebugDrawCommandListPtr>
    {
        public DebugDrawCommandList* Handle;

        public DebugDrawCommandListPtr(DebugDrawCommandList* handle)
        {
            Handle = handle;
        }

        public uint VertexCount => Handle->VertexCount;

        public uint IndexCount => Handle->IndexCount;


        /// <summary>
        /// Gets the vertices of the debug draw command list.
        /// </summary>
        public DebugDrawVert* Vertices => Handle->Vertices;

        /// <summary>
        /// Gets the indices of the debug draw command list.
        /// </summary>
        public uint* Indices => Handle->Indices;

        /// <summary>
        /// Gets the list of debug draw commands.
        /// </summary>
        public UnsafeList<DebugDrawCommand> Commands => Handle->Commands;

        public DebugDrawCommandListType Type => Handle->Type;

        public void BeginDraw()
        {
            Handle->BeginDraw();
        }

        public void BeginDraw(ShapeId id, nint texId, Matrix4x4 transform, Vector4 color)
        {
            Handle->BeginDraw(id, texId, transform, color);
        }

        public void ReserveGeometry(uint nVertices, uint nIndices)
        {
            Handle->ReserveGeometry(nVertices, nIndices);
        }

        public void ReserveVerts(uint nVertices)
        {
            Handle->ReserveVerts(nVertices);
        }

        public void ReserveIndices(uint nIndices)
        {
            Handle->ReserveIndices(nIndices);
        }

        public void AddVertex(DebugDrawVert vertex)
        {
            Handle->AddVertex(vertex);
        }

        public void AddVertexRange(ReadOnlySpan<DebugDrawVert> vertices)
        {
            Handle->AddVertexRange(vertices);
        }

        public void AddIndexRange(ReadOnlySpan<uint> indices)
        {
            Handle->AddIndexRange(indices);
        }

        public void AddIndex(uint index)
        {
            Handle->AddIndex(index);
        }

        public void AddFace(uint i0, uint i1, uint i2)
        {
            Handle->AddFace(i0, i1, i2);
        }

        public void RecordCmd(DebugDrawPrimitiveTopology topology, Matrix4x4 transform)
        {
            Handle->RecordCmd(topology, transform);
        }

        public void RecordCmd(DebugDrawPrimitiveTopology topology, nint texId, Matrix4x4 transform)
        {
            Handle->RecordCmd(topology, texId, transform);
        }

        public void Finish()
        {
            Handle->Finish();
        }

        public void NewFrame()
        {
            Handle->NewFrame();
        }

        public void Dispose()
        {
            Handle->Dispose();
        }

        public override readonly bool Equals(object? obj) => obj is DebugDrawCommandListPtr ptr && Equals(ptr);

        public readonly bool Equals(DebugDrawCommandListPtr other) => Handle == other.Handle;

        public override readonly int GetHashCode() => HashCode.Combine((nint)Handle);

        public static bool operator ==(DebugDrawCommandListPtr left, DebugDrawCommandListPtr right) => left.Equals(right);

        public static bool operator !=(DebugDrawCommandListPtr left, DebugDrawCommandListPtr right) => !(left == right);

        public static implicit operator DebugDrawCommandList*(DebugDrawCommandListPtr ptr) => ptr.Handle;
        public static implicit operator DebugDrawCommandListPtr(DebugDrawCommandList* handle) => new(handle);
    }
}