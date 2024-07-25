﻿#nullable disable

namespace Hexa.NET.DebugDraw
{
    /// <summary>
    /// Represents a command for debugging drawing, used for rendering primitives.
    /// </summary>
    public unsafe struct DebugDrawCommand : IEquatable<DebugDrawCommand>
    {
        /// <summary>
        /// Gets or sets the primitive topology used for rendering.
        /// </summary>
        public DebugDrawPrimitiveTopology Topology;

        /// <summary>
        /// Gets or sets the number of vertices.
        /// </summary>
        public uint VertexCount;

        /// <summary>
        /// Gets or sets the number of indices.
        /// </summary>
        public uint IndexCount;

        /// <summary>
        /// Gets or sets the vertex offset in the vertex buffer (globally).
        /// </summary>
        public uint VertexOffset;

        /// <summary>
        /// Gets or sets the index offset in the index buffer (globally).
        /// </summary>
        public uint IndexOffset;

        /// <summary>
        /// Gets or sets a native integer representing a texture ID, if applicable.
        /// </summary>
        public nint TextureId;

        /// <summary>
        /// Gets or sets a value indicating whether depth testing should be enabled for rendering.
        /// </summary>
        public bool EnableDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDrawCommand"/> struct.
        /// </summary>
        /// <param name="topology">The primitive topology used for rendering.</param>
        /// <param name="vertexCount">The number of vertices.</param>
        /// <param name="indexCount">The number of indices.</param>
        /// <param name="vertexOffset">The vertex offset in the vertex buffer (globally).</param>
        /// <param name="indexOffset">The index offset in the index buffer (globally).</param>
        /// <param name="textureId">A native integer representing a texture ID, if applicable.</param>
        /// <param name="enableDepth">A value indicating whether depth testing should be enabled for rendering.</param>
        public DebugDrawCommand(DebugDrawPrimitiveTopology topology, uint vertexCount, uint indexCount, uint vertexOffset, uint indexOffset, nint textureId, bool enableDepth)
        {
            Topology = topology;
            VertexCount = vertexCount;
            IndexCount = indexCount;
            VertexOffset = vertexOffset;
            IndexOffset = indexOffset;
            TextureId = textureId;
            EnableDepth = enableDepth;
        }

        /// <summary>
        /// Determines whether this <see cref="DebugDrawCommand"/> instance can be merged the specified other <see cref="DebugDrawCommand"/> instance.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        ///   <c>true</c> if this instance can be merged the specified other; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool CanMerge(DebugDrawCommand other)
        {
            // Adjacent topologies are not mergeable!
            if (Topology != DebugDrawPrimitiveTopology.PointList &&
                Topology != DebugDrawPrimitiveTopology.LineList &&
                Topology != DebugDrawPrimitiveTopology.TriangleList)
            {
                return false;
            }

            if (other.Topology != DebugDrawPrimitiveTopology.PointList &&
                other.Topology != DebugDrawPrimitiveTopology.LineList &&
                other.Topology != DebugDrawPrimitiveTopology.TriangleList)
            {
                return false;
            }

            if (other.IndexCount > 8000 || other.VertexCount > 5000)
            {
                return false;
            }

            return Topology == other.Topology && TextureId == other.TextureId && EnableDepth == other.EnableDepth;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is DebugDrawCommand command && Equals(command);
        }

        public readonly bool Equals(DebugDrawCommand other)
        {
            return Topology == other.Topology &&
                   VertexCount == other.VertexCount &&
                   IndexCount == other.IndexCount &&
                   VertexOffset == other.VertexOffset &&
                   IndexOffset == other.IndexOffset &&
                   TextureId.Equals(other.TextureId) &&
                   EnableDepth == other.EnableDepth;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Topology, VertexCount, IndexCount, VertexOffset, IndexOffset, TextureId, EnableDepth);
        }

        public static bool operator ==(DebugDrawCommand left, DebugDrawCommand right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DebugDrawCommand left, DebugDrawCommand right)
        {
            return !(left == right);
        }
    }
}