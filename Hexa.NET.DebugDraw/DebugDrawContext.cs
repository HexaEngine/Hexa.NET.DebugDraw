namespace Hexa.NET.DebugDraw
{
    using Hexa.NET.Utilities;
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

    public unsafe class DebugDrawContext : IDisposable
    {
        private readonly UnsafeList<DebugDrawCommandListPtr>* allocatedCommandLists;
        private readonly DebugDrawData drawData;
        private readonly DebugDrawCommandListPtr immediateList;
        private DebugDrawCommandListPtr currentCommandList;
        private DebugDrawStatistics statistics;

        internal DebugDrawContext()
        {

            immediateList = currentCommandList = Utils.AllocT<DebugDrawCommandList>();
            *currentCommandList.Handle = new(DebugDrawCommandListType.Immediate);
            allocatedCommandLists = Utils.AllocT<UnsafeList<DebugDrawCommandListPtr>>();
            *allocatedCommandLists = [];
            allocatedCommandLists->Add(currentCommandList);
            drawData = new();
        }

        public DebugDrawCommandListPtr CurrentList => currentCommandList;

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

        public void ExecuteCommandList(DebugDrawCommandListPtr commandList)
        {
            drawData.CmdLists.Add(commandList);
        }

        public void SetCurrent(DebugDrawCommandListPtr commandList)
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

        public DebugDrawCommandListPtr CreateCommandList(DebugDrawCommandListType type = DebugDrawCommandListType.Deferred)
        {
            var cmdList = Utils.AllocT<DebugDrawCommandList>();
            *cmdList = new(type);
            allocatedCommandLists->Add(cmdList);
            return cmdList;
        }

        public void FreeCommandList(DebugDrawCommandListPtr commandList)
        {
            if (commandList == immediateList)
            {
                throw new InvalidOperationException("Cannot free the immediate command list");
            }

            if (allocatedCommandLists->Remove(commandList))
            {
                commandList.Dispose();
                Utils.Free(commandList.Handle);
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
            foreach (var cmdList in *allocatedCommandLists)
            {
                cmdList.Dispose();
                Utils.Free(cmdList.Handle);
            }
            allocatedCommandLists->Release();
            Utils.Free(allocatedCommandLists);
            drawData.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DebugDraw.SetCurrentContext(null);
            Destroy();
        }
    }
}