#nullable disable

namespace Hexa.NET.DebugDraw
{
    using Hexa.NET.Utilities;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents debug draw data.
    /// </summary>
    public unsafe class DebugDrawData
    {
        /// <summary>
        /// Gets the list of debug draw command lists.
        /// </summary>
        public UnsafeList<DebugDrawCommandListPtr> CmdLists;

        /// <summary>
        /// Gets or sets the total number of vertices.
        /// </summary>
        public uint TotalVertices;

        /// <summary>
        /// Gets or sets the total number of indices.
        /// </summary>
        public uint TotalIndices;

        /// <summary>
        /// Gets or sets the viewport information.
        /// </summary>
        public DebugDrawViewport Viewport;

        /// <summary>
        /// Gets or sets the camera matrix.
        /// </summary>
        public Matrix4x4 Camera;

        public DebugDrawData()
        {
            CmdLists = [];
        }

        public void Clear()
        {
            TotalIndices = 0;
            TotalVertices = 0;
        }

        public void Dispose()
        {
            CmdLists.Release();
        }
    }
}