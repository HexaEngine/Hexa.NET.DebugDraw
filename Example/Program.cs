namespace Example
{
    using Hexa.NET.DebugDraw;
    using Hexa.NET.Mathematics;
    using Silk.NET.Input;
    using Silk.NET.OpenGL;
    using Silk.NET.Windowing;
    using System.Numerics;

    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create a Silk.NET window as usual
            using var window = Window.Create(WindowOptions.Default);

            // Declare some variables
            DebugDrawRenderer renderer = null;
            GL gl = null;
            IInputContext inputContext = null;

            // Our loading function
            window.Load += () =>
            {
                renderer = new DebugDrawRenderer(
                    gl = window.CreateOpenGL(), // load OpenGL,
                    window
                );
                inputContext = window.CreateInput();
            };

            // Handle resizes
            window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                gl.Viewport(s);
                DebugDraw.SetViewport(new Viewport(0, 0, s.X, s.Y));
            };

            Vector3 position = new(0, 5, -1);
            Quaternion rotation = new Vector3(0, 0, 0).ToRad().ToQuaternion();
            Matrix4x4 projection = MathUtil.PerspectiveFovLH(
                MathF.PI / 2, window.Size.X / window.Size.Y, 0.1f, 1000f
            );
            Vector3 target = Vector3.Transform(Vector3.UnitZ, rotation);
            Vector3 up = Vector3.Transform(Vector3.UnitY, rotation);
            Matrix4x4 view = MathUtil.LookAtLH(position, position + target, up);

            Matrix4x4 vp = view * projection;

            // The render function
            window.Render += delta =>
            {
                // Make sure ImGui is up-to-date
                renderer.BeginDraw();

                DebugDraw.SetCamera(vp);

                // This is where you'll do any rendering beneath the ImGui context
                // Here, we just have a blank screen.
                gl.ClearColor(System.Drawing.Color.FromArgb(255, (int)(.45f * 255), (int)(.55f * 255), (int)(.60f * 255)));
                gl.Clear((uint)ClearBufferMask.ColorBufferBit);
                gl.ClearDepth(1.0f);
                gl.Clear((uint)ClearBufferMask.DepthBufferBit);

                DebugDraw.DrawGrid(Matrix4x4.Identity, 100, Vector4.One);

                DebugDraw.DrawBox(new(2, 4, 5), Quaternion.Identity, 1, 1, 1, new(1, 0, 0, 1));

                DebugDraw.DrawSphere(new(-2, 4, 5), Quaternion.Identity, 1, new(0, 1, 0, 1));

                DebugDraw.DrawCapsule(new(0, 4, 5), Quaternion.Identity, 0.5f, 2, new(0, 0, 1, 1));

                renderer.EndDraw();
            };

            // The closing function
            window.Closing += () =>
            {
                // Dispose our controller first
                renderer?.Dispose();

                // Dispose the input context
                inputContext?.Dispose();

                // Unload OpenGL
                gl?.Dispose();
            };

            // Now that everything's defined, let's run this bad boy!
            window.Run();

            window.Dispose();
        }
    }
}