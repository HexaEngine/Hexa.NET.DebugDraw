namespace Hexa.NET.DebugDraw
{
    using System.Collections.Generic;
    using System.Numerics;

    public struct DebugDrawStatistics
    {
        public uint VertexCount;
        public uint IndexCount;
        public uint DrawCalls;

        public DebugDrawStatistics(uint vertexCount, uint indexCount, uint drawCalls)
        {
            VertexCount = vertexCount;
            IndexCount = indexCount;
            DrawCalls = drawCalls;
        }
    }

    public class DebugDrawContext : IDisposable
    {
        private readonly List<DebugDrawCommandList> commandLists = new();
        private readonly DebugDrawData drawData;
        private readonly DebugDrawCommandList immediateList;
        private DebugDrawCommandList currentCommandList;
        private DebugDrawStatistics statistics;

        internal DebugDrawContext()
        {
            immediateList = currentCommandList = new(DebugDrawCommandListType.Immediate);
            commandLists.Add(currentCommandList);
            drawData = new(commandLists);
        }

        public DebugDrawCommandList CurrentList => currentCommandList;

        public Matrix4x4 Camera => drawData.Camera;

        public DebugDrawViewport Viewport => drawData.Viewport;

        public nint FontTextureId { get; set; }

        public DebugDrawStyle Style { get; } = new();

        public void SetCamera(Matrix4x4 camera)
        {
            drawData.Camera = camera;
        }

        public void SetViewport(DebugDrawViewport viewport)
        {
            drawData.Viewport = viewport;
        }

        public void SetViewport(Vector2 offset, Vector2 size)
        {
            drawData.Viewport = new(offset, size);
        }

        public void NewFrame()
        {
            drawData.CmdLists.Clear();
            drawData.CmdLists.Add(immediateList);
            immediateList.NewFrame();
            currentCommandList = immediateList;
        }

        internal void EndFrame(DebugDrawStatistics statistics)
        {
            this.statistics = statistics;
        }

        public void ExecuteCommandList(DebugDrawCommandList commandList)
        {
            commandLists.Add(commandList);
        }

        public void SetCurrent(DebugDrawCommandList? commandList)
        {
            if (commandList == null)
            {
                currentCommandList = immediateList;
            }
            else
            {
                currentCommandList = commandList;
            }
        }

        internal DebugDrawData GetDrawData()
        {
            return drawData;
        }

        internal DebugDrawStatistics GetStatistics()
        {
            return statistics;
        }

        internal void Destroy()
        {
            immediateList.Dispose();
            commandLists.Clear();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DebugDraw.SetCurrentContext(null);
            Destroy();
        }
    }
}