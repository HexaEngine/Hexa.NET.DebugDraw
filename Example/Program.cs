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
            DebugDrawRenderer renderer = null!;
            GL gl = null!;
            IInputContext inputContext = null!;
            IMouse mouse = null!;
            IKeyboard keyboard = null!;
            Vector2 lastMousePos = default;

            // Our loading function
            window.Load += () =>
            {
                renderer = new DebugDrawRenderer(
                    gl = window.CreateOpenGL(), // load OpenGL,
                    window
                );
                inputContext = window.CreateInput();
                mouse = inputContext.Mice[0];
                keyboard = inputContext.Keyboards[0];
                lastMousePos = mouse.Position;
            };

            // Handle resizes
            window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                gl.Viewport(s);
                DebugDraw.SetViewport(Vector2.Zero, new(s.X, s.Y));
            };

            Vector3 position = new(-5, 5, -5);
            Vector3 rotation = new(45, 45, 0);
            Quaternion orientation = rotation.ToRad().ToQuaternion();
            Matrix4x4 projection = MathUtil.PerspectiveFovLH(
                90f.ToRad(), window.Size.X / window.Size.Y, 0.1f, 1000f
            );
            Vector3 target = Vector3.Transform(Vector3.UnitZ, orientation);
            Vector3 up = Vector3.Transform(Vector3.UnitY, orientation);
            Matrix4x4 view = MathUtil.LookAtLH(position, position + target, up);

            Matrix4x4 vp = view * projection;

            // The render function
            window.Render += delta =>
            {
                var now = mouse.Position;
                var mouseDelta = now - lastMousePos;
                lastMousePos = now;

                if (mouseDelta != Vector2.Zero && mouse.IsButtonPressed(MouseButton.Left))
                {
                    rotation += new Vector3(mouseDelta * 0.004f * 20, 0);
                    Update(position, rotation, out orientation, projection, out vp);
                }

                Vector3 direction = default;
                if (keyboard.IsKeyPressed(Key.W))
                {
                    direction += Vector3.UnitZ;
                }
                if (keyboard.IsKeyPressed(Key.A))
                {
                    direction -= Vector3.UnitX;
                }
                if (keyboard.IsKeyPressed(Key.D))
                {
                    direction += Vector3.UnitX;
                }
                if (keyboard.IsKeyPressed(Key.S))
                {
                    direction -= Vector3.UnitZ;
                }
                if (keyboard.IsKeyPressed(Key.Space))
                {
                    direction += Vector3.UnitY;
                }
                if (keyboard.IsKeyPressed(Key.C))
                {
                    direction -= Vector3.UnitY;
                }

                if (direction != Vector3.Zero)
                {
                    direction *= 2;
                    if (keyboard.IsKeyPressed(Key.ShiftLeft))
                    {
                        direction *= 2;
                    }
                    direction = Vector3.Transform(direction, orientation);
                    position += direction * (float)delta;
                    Update(position, rotation, out orientation, projection, out vp);
                }

                renderer.BeginDraw();

                DebugDraw.SetCamera(vp);

                gl.ClearColor(System.Drawing.Color.FromArgb(255, (int)(.45f * 255), (int)(.55f * 255), (int)(.60f * 255)));
                gl.Clear((uint)ClearBufferMask.ColorBufferBit);
                gl.ClearDepth(1.0f);
                gl.Clear((uint)ClearBufferMask.DepthBufferBit);

                DebugDraw.PushStyleVar(DebugDrawStyleVar.GridAxisSize, 5);
                DebugDraw.DrawGrid(Matrix4x4.Identity, GridFlags.DrawAxis);
                DebugDraw.PopStyleVar();

                DebugDraw.DrawBox(new(2, 4, 5), Quaternion.Identity, 1, 1, 1, new(1, 0, 0, 1));

                DebugDraw.DrawSphere(new(-2, 4, 5), Quaternion.Identity, 1, new(0, 1, 0, 1));

                DebugDraw.DrawCapsule(new(0, 4, 5), Quaternion.Identity, 0.5f, 2, new(0, 0, 1, 1));

                renderer.EndDraw();
            };

            // The closing function
            window.Closing += () =>
            {
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

        private static void Update(Vector3 position, Vector3 rotation, out Quaternion orientation, Matrix4x4 projection, out Matrix4x4 vp)
        {
            orientation = rotation.ToRad().ToQuaternion();
            Vector3 target = Vector3.Transform(Vector3.UnitZ, orientation);
            Vector3 up = Vector3.Transform(Vector3.UnitY, orientation);
            Matrix4x4 view = MathUtil.LookAtLH(position, position + target, up);
            vp = view * projection;
        }
    }
}