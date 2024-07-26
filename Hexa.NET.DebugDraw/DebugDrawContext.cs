namespace Hexa.NET.DebugDraw
{
    using Hexa.NET.Mathematics;
    using System.Collections.Generic;
    using System.Numerics;

    public class DebugDrawContext : IDisposable
    {
        private readonly List<DebugDrawCommandList> commandLists = new();
        private readonly DebugDrawData drawData;
        private readonly DebugDrawCommandList immediateList;
        private DebugDrawCommandList currentCommandList;

        internal DebugDrawContext()
        {
            immediateList = currentCommandList = new(DebugDrawCommandListType.Immediate);
            commandLists.Add(currentCommandList);
            drawData = new(commandLists);
        }

        public DebugDrawCommandList CurrentList => currentCommandList;

        public Matrix4x4 Camera => drawData.Camera;

        public Viewport Viewport => drawData.Viewport;

        public nint FontTextureId { get; set; }

        public DebugDrawStyle Style { get; } = new();

        public void SetCamera(Matrix4x4 camera)
        {
            drawData.Camera = camera;
        }

        public void SetViewport(Viewport viewport)
        {
            drawData.Viewport = viewport;
        }

        public void NewFrame()
        {
            drawData.CmdLists.Clear();
            drawData.CmdLists.Add(immediateList);
            immediateList.NewFrame();
            currentCommandList = immediateList;
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