﻿#nullable disable

namespace Hexa.NET.DebugDraw
{
    using Hexa.NET.Utilities;
    using System.Numerics;

    /// <summary>
    /// Represents a queue of debugging drawing commands for rendering primitives.
    /// </summary>
    public unsafe class DebugDrawCommandList : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private UnsafeList<DebugDrawVert> vertices = new(vertexBufferSize);
        private UnsafeList<uint> indices = new(indexBufferSize);

        private UnsafeList<DebugDrawCommand> queue = [];
        private UnsafeDictionary<(ShapeId id, nint texId), int> map = [];
        private UnsafeList<InstanceData> instanceData = [];

        private uint nVerticesCmd;
        private uint nIndicesCmd;
        private bool finished;
        private uint nVerticesTotal;
        private uint nIndicesTotal;
        private bool disposedValue;

        private const int vertexBufferSize = 5000;
        private const int indexBufferSize = 10000;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDrawCommandList"/> class.
        /// </summary>
        /// <param name="type">The type of the debug draw command list.</param>
        public DebugDrawCommandList(DebugDrawCommandListType type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the total count of vertices used in the commands in the queue.
        /// </summary>
        public uint VertexCount => nVerticesTotal;

        /// <summary>
        /// Gets the total count of indices used in the commands in the queue.
        /// </summary>
        public uint IndexCount => nIndicesTotal;

        /// <summary>
        /// Gets the vertices of the debug draw command list.
        /// </summary>
        public DebugDrawVert* Vertices => vertices.Data;

        /// <summary>
        /// Gets the indices of the debug draw command list.
        /// </summary>
        public uint* Indices => indices.Data;

        /// <summary>
        /// Gets the list of debug draw commands.
        /// </summary>
        public UnsafeList<DebugDrawCommand> Commands => queue;

        /// <summary>
        /// Gets the type of the debug draw command list.
        /// </summary>
        public DebugDrawCommandListType Type { get; }

        public void BeginDraw()
        {
            semaphore.Wait();
        }

        public void BeginDraw(ShapeId id, nint texId, Matrix4x4 transform, Vector4 color)
        {
            semaphore.Wait();
            var key = (id, texId);

            if (map.TryGetValue(key, out var index))
            {
                var pCmd = queue.GetPointer(index);
            }
        }

        /// <summary>
        /// Reserves space for geometry in the debug draw command list.
        /// </summary>
        /// <param name="nVertices">The number of vertices to reserve.</param>
        /// <param name="nIndices">The number of indices to reserve.</param>
        public void ReserveGeometry(uint nVertices, uint nIndices)
        {
            vertices.Resize(vertices.Size + (int)nVertices);
            indices.Resize(indices.Size + (int)nIndices);
            nVerticesCmd += nVertices;
            nIndicesCmd += nIndices;
        }

        /// <summary>
        /// Reserves space for vertices in the debug draw command list.
        /// </summary>
        /// <param name="nVertices">The number of vertices to reserve.</param>
        public void ReserveVerts(uint nVertices)
        {
            vertices.Resize(vertices.Size + (int)nVertices);
            nVerticesCmd += nVertices;
        }

        /// <summary>
        /// Reserves space for indices in the debug draw command list.
        /// </summary>
        /// <param name="nIndices">The number of indices to reserve.</param>
        public void ReserveIndices(uint nIndices)
        {
            indices.Resize(indices.Size + (int)nIndices);
            nIndicesCmd += nIndices;
        }

        /// <summary>
        /// Adds a vertex to the debug draw command list.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        public void AddVertex(DebugDrawVert vertex)
        {
            vertices.PushBack(vertex);
            nVerticesCmd++;
        }

        /// <summary>
        /// Adds a range of vertices to the debug draw command list.
        /// </summary>
        /// <param name="vertices">The array of vertices to add.</param>
        public void AddVertexRange(DebugDrawVert[] vertices)
        {
            this.vertices.AppendRange(vertices);
            nVerticesCmd += (uint)vertices.Length;
        }

        /// <summary>
        /// Adds a range of indices to the debug draw command list.
        /// </summary>
        /// <param name="indices">The array of indices to add.</param>
        public void AddIndexRange(uint[] indices)
        {
            this.indices.AppendRange(indices);
            nIndicesCmd += (uint)indices.Length;
        }

        /// <summary>
        /// Adds an index to the debug draw command list.
        /// </summary>
        /// <param name="index">The index to add.</param>
        public void AddIndex(uint index)
        {
            indices.PushBack(index);
            nIndicesCmd++;
        }

        /// <summary>
        /// Adds a face (triangle) defined by three indices to the debug draw command list.
        /// </summary>
        /// <param name="i0">The first index of the face.</param>
        /// <param name="i1">The second index of the face.</param>
        /// <param name="i2">The third index of the face.</param>
        public void AddFace(uint i0, uint i1, uint i2)
        {
            indices.PushBack(i0);
            indices.PushBack(i1);
            indices.PushBack(i2);
            nIndicesCmd += 3;
        }

        /// <summary>
        /// Records a command in the debug draw command list with the specified primitive topology.
        /// </summary>
        /// <param name="topology">The primitive topology of the command.</param>
        public void RecordCmd(DebugDrawPrimitiveTopology topology, Matrix4x4 transform)
        {
            RecordCmd(topology, 0, transform);
        }

        /// <summary>
        /// Records a command in the debug draw command list with the specified primitive topology, texture ID, and depth enabling.
        /// </summary>
        /// <param name="topology">The primitive topology of the command.</param>
        /// <param name="texId">The texture ID of the command.</param>
        /// <param name="enableDepth">Whether depth should be enabled for the command.</param>
        public void RecordCmd(DebugDrawPrimitiveTopology topology, nint texId, Matrix4x4 transform)
        {
            DebugDrawCommand cmd = new(topology, nVerticesCmd, nIndicesCmd, nVerticesTotal, nIndicesTotal, texId, transform);
            queue.Add(cmd);
            nVerticesTotal += nVerticesCmd;
            nIndicesTotal += nIndicesCmd;
            nIndicesCmd = 0;
            nVerticesCmd = 0;
            semaphore.Release();
        }

        /// <summary>
        /// Compacts the debug draw command list by merging compatible commands.
        /// </summary>
        public void Finish()
        {
            if (finished)
            {
                return;
            }

            semaphore.Wait();

            finished = true;

            if (nVerticesTotal == 0)
            {
                return;
            }

            if (vertices.Count + vertexBufferSize < vertices.Capacity)
            {
                vertices.ShrinkToFit();
            }

            if (indices.Count + indexBufferSize < indices.Capacity)
            {
                indices.ShrinkToFit();
            }

            if (queue.Count < 2)
            {
                return;
            }

            int writeIndex = 0; // This will keep track of where to write in the list
            DebugDrawCommand* last = queue.GetPointer(0); // Get the pointer to the first element
            int merged = 0;
            const int mergeThreshold = 4;
            for (int i = 1; i < queue.Count; i++)
            {
                DebugDrawCommand* cmd = queue.GetPointer(i); // Get the pointer to the current element

                if ((merged < mergeThreshold || Type == DebugDrawCommandListType.Deferred) && last->CanMerge(*cmd))
                {
                    // Merge cmd into last
                    uint vOffset = last->VertexCount;
                    for (int j = 0; j < cmd->IndexCount; j++)
                    {
                        indices[(int)cmd->IndexOffset + j] += vOffset;
                    }

                    Matrix4x4.Invert(last->Transform, out var inverseBaseMatrix);
                    Matrix4x4 relativeMatrix = cmd->Transform * inverseBaseMatrix;
                    for (int j = 0; j < cmd->VertexCount; j++)
                    {
                        var pVertex = vertices.GetPointer((int)cmd->VertexOffset + j);
                        pVertex->Position = Vector3.Transform(pVertex->Position, relativeMatrix);
                    }

                    merged++;

                    // Update the last element with merged values
                    last->VertexCount += cmd->VertexCount;
                    last->IndexCount += cmd->IndexCount;
                }
                else
                {
                    merged = 0;
                    // Move the "write" pointer up, if necessary, to avoid overlap
                    writeIndex++;
                    if (writeIndex != i)
                    {
                        // Copy the cmd to the new write position
                        *queue.GetPointer(writeIndex) = *cmd;
                    }

                    // Update last to the new "last" element
                    last = queue.GetPointer(writeIndex);
                }
            }

            // Adjust the size of the queue if necessary to remove any unused elements at the end
            queue.Resize(writeIndex + 1);
        }

        /// <summary>
        /// Clears the debug draw command list.
        /// </summary>
        public void NewFrame()
        {
            queue.Clear();
            vertices.Clear();
            indices.Clear();
            nVerticesTotal = 0;
            nVerticesCmd = 0;
            nIndicesTotal = 0;
            nIndicesCmd = 0;
            finished = false;
            semaphore.Release();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                semaphore.Dispose();
                vertices.Release();
                indices.Release();
                queue.Release();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}