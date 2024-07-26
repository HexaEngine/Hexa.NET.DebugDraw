#nullable disable

namespace Hexa.NET.DebugDraw
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents debug draw data.
    /// </summary>
    public unsafe class DebugDrawData
    {
        /// <summary>
        /// Gets the list of debug draw command lists.
        /// </summary>
        public List<DebugDrawCommandList> CmdLists { get; }

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
        public Viewport Viewport;

        /// <summary>
        /// Gets or sets the camera matrix.
        /// </summary>
        public Matrix4x4 Camera;

        public DebugDrawData()
        {
            CmdLists = [];
        }

        public DebugDrawData(List<DebugDrawCommandList> cmdLists)
        {
            CmdLists = cmdLists;
        }
    }
}