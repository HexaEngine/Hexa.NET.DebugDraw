#define GL

namespace Example
{
    using Hexa.NET.DebugDraw;
    using System;

#if GLES
using Silk.NET.OpenGLES;
#elif GL

    using Silk.NET.OpenGL;

#elif LEGACY
using Silk.NET.OpenGL.Legacy;
#endif

    using Silk.NET.Windowing;
    using System.Numerics;
    using Silk.NET.SDL;
    using System.Diagnostics;
    using Hexa.NET.ImGui;

    public class DebugDrawRenderer
    {
        private GL _gl;

        private int _attribLocationTex;
        private int _attribLocationProjMtx;
        private int _attribLocationVtxPos;
        private int _attribLocationVtxUV;
        private int _attribLocationVtxColor;
        private uint _vboHandle;
        private uint _elementsHandle;

        private Texture _fontTexture;
        private Shader _shader;

        private DebugDrawContext context;

#nullable disable

        /// <summary>
        /// Constructs a new ImGuiController with font configuration and onConfigure Action.
        /// </summary>
        public DebugDrawRenderer(GL gl, IWindow window)
        {
            _gl = gl;
            context = DebugDraw.CreateContext();
            context.SetViewport(Vector2.Zero, new(window.Size.X, window.Size.Y));
            CreateDeviceObjects();
        }

#nullable restore

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void EndDraw()
        {
            Profiler.Begin("DebugDraw.Render");
            DebugDraw.Render();
            Profiler.EndImGui("DebugDraw.Render");
            Profiler.Begin("DebugDraw.Render.OpenGL");
            Render(DebugDraw.GetDrawData());
            Profiler.EndImGui("DebugDraw.Render.OpenGL");
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void BeginDraw()
        {
            DebugDraw.SetCurrentContext(context);
            DebugDraw.NewFrame();
        }

        private unsafe void SetupRenderState(DebugDrawData drawData, uint vertex_array_object)
        {
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, polygon fill
            _gl.Enable(GLEnum.Blend);
            _gl.BlendEquation(GLEnum.FuncAdd);
            _gl.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
            _gl.Disable(GLEnum.CullFace);
            _gl.Enable(GLEnum.DepthTest);
            _gl.DepthMask(false);
            _gl.Enable(GLEnum.DepthClamp);
            _gl.Enable(GLEnum.LineSmooth);
            _gl.Hint(GLEnum.LineSmoothHint, GLEnum.Nicest);
            _gl.DepthFunc(DepthFunction.Lequal);
            _gl.Disable(GLEnum.StencilTest);
            _gl.Disable(GLEnum.ScissorTest); //_gl.Enable(GLEnum.ScissorTest);
#if !GLES && !LEGACY
            _gl.Disable(GLEnum.PrimitiveRestart);
            _gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
#endif

            _shader.UseShader();
            _gl.Uniform1(_attribLocationTex, 0);
            /*GL_CALL*/

            _gl.BindSampler(0, 0);

            _gl.BindVertexArray(vertex_array_object);
            /*GL_CALL*/

            // Bind vertex/index buffers and setup attributes for ImDrawVert
            _gl.BindBuffer(GLEnum.ArrayBuffer, _vboHandle);
            _gl.BindBuffer(GLEnum.ElementArrayBuffer, _elementsHandle);
            _gl.EnableVertexAttribArray((uint)_attribLocationVtxPos);
            _gl.EnableVertexAttribArray((uint)_attribLocationVtxUV);
            _gl.EnableVertexAttribArray((uint)_attribLocationVtxColor);
            _gl.VertexAttribPointer((uint)_attribLocationVtxPos, 3, GLEnum.Float, false, (uint)sizeof(DebugDrawVert), (void*)0);
            _gl.VertexAttribPointer((uint)_attribLocationVtxUV, 2, GLEnum.Float, false, (uint)sizeof(DebugDrawVert), (void*)12);
            _gl.VertexAttribPointer((uint)_attribLocationVtxColor, 4, GLEnum.UnsignedByte, true, (uint)sizeof(DebugDrawVert), (void*)20);
        }

        private unsafe void Render(DebugDrawData drawData)
        {
            DebugDrawViewport viewport = drawData.Viewport;
            int framebufferWidth = (int)(viewport.Width);
            int framebufferHeight = (int)(viewport.Height);
            if (framebufferWidth <= 0 || framebufferHeight <= 0)
                return;

            // Backup GL state
            _gl.GetInteger(GLEnum.ActiveTexture, out int lastActiveTexture);
            _gl.ActiveTexture(GLEnum.Texture0);

            _gl.GetInteger(GLEnum.CurrentProgram, out int lastProgram);
            _gl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);

            _gl.GetInteger(GLEnum.SamplerBinding, out int lastSampler);

            _gl.GetInteger(GLEnum.ArrayBufferBinding, out int lastArrayBuffer);
            _gl.GetInteger(GLEnum.VertexArrayBinding, out int lastVertexArrayObject);

#if !GLES
            Span<int> lastPolygonMode = stackalloc int[2];
            _gl.GetInteger(GLEnum.PolygonMode, lastPolygonMode);
#endif

            Span<int> lastScissorBox = stackalloc int[4];
            _gl.GetInteger(GLEnum.ScissorBox, lastScissorBox);

            _gl.GetInteger(GLEnum.BlendSrcRgb, out int lastBlendSrcRgb);
            _gl.GetInteger(GLEnum.BlendDstRgb, out int lastBlendDstRgb);

            _gl.GetInteger(GLEnum.BlendSrcAlpha, out int lastBlendSrcAlpha);
            _gl.GetInteger(GLEnum.BlendDstAlpha, out int lastBlendDstAlpha);

            _gl.GetInteger(GLEnum.BlendEquationRgb, out int lastBlendEquationRgb);
            _gl.GetInteger(GLEnum.BlendEquationAlpha, out int lastBlendEquationAlpha);

            bool lastEnableBlend = _gl.IsEnabled(GLEnum.Blend);
            bool lastEnableCullFace = _gl.IsEnabled(GLEnum.CullFace);
            bool lastEnableDepthTest = _gl.IsEnabled(GLEnum.DepthTest);
            bool lastEnableStencilTest = _gl.IsEnabled(GLEnum.StencilTest);
            bool lastEnableScissorTest = _gl.IsEnabled(GLEnum.ScissorTest);

#if !GLES && !LEGACY
            bool lastEnablePrimitiveRestart = _gl.IsEnabled(GLEnum.PrimitiveRestart);
#endif

            // Setup desired GL state
            // Recreate the VAO every time (this is to easily allow multiple GL contexts to be rendered to. VAO are not shared among GL contexts)
            // The renderer would actually work without any VAO bound, but then our VertexAttrib calls would overwrite the default one currently bound.
            uint vertex_array_object = 0;

            /*GL_CALL*/
            _gl.GenVertexArrays(1, &vertex_array_object);

            SetupRenderState(drawData, vertex_array_object);

            var camera = drawData.Camera;

            for (int i = 0; i < drawData.CmdLists.Count; i++)
            {
                var list = drawData.CmdLists[i];
                // Upload vertex/index buffers

                _gl.BufferData(GLEnum.ArrayBuffer, (nuint)(list.VertexCount * sizeof(DebugDrawVert)), list.Vertices, GLEnum.StreamDraw);
                /*GL_CALL*/
                _gl.BufferData(GLEnum.ElementArrayBuffer, list.IndexCount * sizeof(uint), list.Indices, GLEnum.StreamDraw);
                /*GL_CALL*/

                int voffset = 0;
                uint ioffset = 0;
                for (int cmd_i = 0; cmd_i < list.Commands.Count; cmd_i++)
                {
                    DebugDrawCommand cmd = list.Commands[cmd_i];

                    Matrix4x4 mvp = cmd.Transform * camera;

                    _gl.UniformMatrix4(_attribLocationProjMtx, 1, false, (float*)&mvp);

                    /*
                    Vector4 clipRect;
                    clipRect.X = (cmdPtr.ClipRect.X - clipOff.X) * clipScale.X;
                    clipRect.Y = (cmdPtr.ClipRect.Y - clipOff.Y) * clipScale.Y;
                    clipRect.Z = (cmdPtr.ClipRect.Z - clipOff.X) * clipScale.X;
                    clipRect.W = (cmdPtr.ClipRect.W - clipOff.Y) * clipScale.Y;
                    */

                    // Apply scissor/clipping rectangle
                    //_gl.Scissor((int)clipRect.X, (int)(framebufferHeight - clipRect.W), (uint)(clipRect.Z - clipRect.X), (uint)(clipRect.W - clipRect.Y));
                    //_gl.CheckGlError("Scissor");

                    var texId = (uint)cmd.TextureId;
                    if (texId == 0)
                    {
                        texId = _fontTexture.GlTexture;
                    }

                    // Bind texture, Draw
                    _gl.BindTexture(GLEnum.Texture2D, texId);
                    /*GL_CALL*/

                    GLEnum prim = GetPrim(cmd.Topology);

                    _gl.DrawElementsBaseVertex(prim, cmd.IndexCount, GLEnum.UnsignedInt, (void*)(ioffset * sizeof(uint)), voffset);
                    //  _gl.DrawElementsInstancedBaseVertexBaseInstance
                    /*GL_CALL*/

                    voffset += (int)cmd.VertexCount;
                    ioffset += cmd.IndexCount;
                }
            }

            // Destroy the temporary VAO
            _gl.DeleteVertexArray(vertex_array_object);

            // Restore modified GL state
            _gl.UseProgram((uint)lastProgram);
            _gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);

            _gl.BindSampler(0, (uint)lastSampler);

            _gl.ActiveTexture((GLEnum)lastActiveTexture);

            _gl.BindVertexArray((uint)lastVertexArrayObject);

            _gl.BindBuffer(GLEnum.ArrayBuffer, (uint)lastArrayBuffer);
            _gl.BlendEquationSeparate((GLEnum)lastBlendEquationRgb, (GLEnum)lastBlendEquationAlpha);
            _gl.BlendFuncSeparate((GLEnum)lastBlendSrcRgb, (GLEnum)lastBlendDstRgb, (GLEnum)lastBlendSrcAlpha, (GLEnum)lastBlendDstAlpha);

            if (lastEnableBlend)
            {
                _gl.Enable(GLEnum.Blend);
            }
            else
            {
                _gl.Disable(GLEnum.Blend);
            }

            if (lastEnableCullFace)
            {
                _gl.Enable(GLEnum.CullFace);
            }
            else
            {
                _gl.Disable(GLEnum.CullFace);
            }

            if (lastEnableDepthTest)
            {
                _gl.Enable(GLEnum.DepthTest);
            }
            else
            {
                _gl.Disable(GLEnum.DepthTest);
            }
            if (lastEnableStencilTest)
            {
                _gl.Enable(GLEnum.StencilTest);
            }
            else
            {
                _gl.Disable(GLEnum.StencilTest);
            }

            if (lastEnableScissorTest)
            {
                _gl.Enable(GLEnum.ScissorTest);
            }
            else
            {
                _gl.Disable(GLEnum.ScissorTest);
            }

#if !GLES && !LEGACY
            if (lastEnablePrimitiveRestart)
            {
                _gl.Enable(GLEnum.PrimitiveRestart);
            }
            else
            {
                _gl.Disable(GLEnum.PrimitiveRestart);
            }

            _gl.PolygonMode(GLEnum.FrontAndBack, (GLEnum)lastPolygonMode[0]);
#endif

            _gl.Scissor(lastScissorBox[0], lastScissorBox[1], (uint)lastScissorBox[2], (uint)lastScissorBox[3]);
        }

        private static GLEnum GetPrim(DebugDrawPrimitiveTopology topology)
        {
            return topology switch
            {
                DebugDrawPrimitiveTopology.Undefined => throw new NotImplementedException(),
                DebugDrawPrimitiveTopology.PointList => GLEnum.Points,
                DebugDrawPrimitiveTopology.LineList => GLEnum.Lines,
                DebugDrawPrimitiveTopology.LineStrip => GLEnum.LineStrip,
                DebugDrawPrimitiveTopology.TriangleList => GLEnum.Triangles,
                DebugDrawPrimitiveTopology.TriangleStrip => GLEnum.TriangleStrip,
                DebugDrawPrimitiveTopology.LineListAdjacency => GLEnum.LinesAdjacency,
                DebugDrawPrimitiveTopology.LineStripAdjacency => GLEnum.LineStripAdjacency,
                DebugDrawPrimitiveTopology.TriangleListAdjacency => GLEnum.TrianglesAdjacency,
                DebugDrawPrimitiveTopology.TriangleStripAdjacency => GLEnum.TriangleStripAdjacency,
                DebugDrawPrimitiveTopology.PatchListWith1ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith2ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith3ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith4ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith5ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith6ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith7ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith8ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith9ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith10ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith11ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith12ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith13ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith14ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith15ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith16ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith17ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith18ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith19ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith20ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith21ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith22ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith23ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith24ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith25ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith26ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith27ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith28ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith29ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith30ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith31ControlPoints => GLEnum.Patches,
                DebugDrawPrimitiveTopology.PatchListWith32ControlPoints => GLEnum.Patches,
                _ => throw new ArgumentOutOfRangeException(nameof(topology), topology, null)
            };
        }

        private void CreateDeviceObjects()
        {
            // Backup GL state

            _gl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);
            _gl.GetInteger(GLEnum.ArrayBufferBinding, out int lastArrayBuffer);
            _gl.GetInteger(GLEnum.VertexArrayBinding, out int lastVertexArray);

            string vertexSource =
#if GLES
                @"#version 300 es
        precision highp float;

        layout (location = 0) in vec3 Position;
        layout (location = 1) in vec2 UV;
        layout (location = 2) in vec4 Color;
        uniform mat4 ProjMtx;
        out vec2 Frag_UV;
        out vec4 Frag_Color;
        void main()
        {
            Frag_UV = UV;
            Frag_Color = Color;
            gl_Position = ProjMtx * vec4(Position.xyz,1.0);
        }";
#elif GL
                @"#version 330
        layout (location = 0) in vec3 Position;
        layout (location = 1) in vec2 UV;
        layout (location = 2) in vec4 Color;
        uniform mat4 ProjMtx;
        out vec2 Frag_UV;
        out vec4 Frag_Color;
        void main()
        {
            Frag_UV = UV;
            Frag_Color = Color;
            gl_Position = ProjMtx * vec4(Position.xyz,1);
        }";
#elif LEGACY
                @"#version 110
        attribute vec3 Position;
        attribute vec2 UV;
        attribute vec4 Color;

        uniform mat4 ProjMtx;

        varying vec2 Frag_UV;
        varying vec4 Frag_Color;

        void main()
        {
            Frag_UV = UV;
            Frag_Color = Color;
            gl_Position = ProjMtx * vec4(Position.xyz,1);
        }";
#endif

            string fragmentSource =
#if GLES
                @"#version 300 es
        precision highp float;

        in vec2 Frag_UV;
        in vec4 Frag_Color;
        uniform sampler2D Texture;
        layout (location = 0) out vec4 Out_Color;
        void main()
        {
            Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
        }";
#elif GL
                @"#version 330
        in vec2 Frag_UV;
        in vec4 Frag_Color;
        uniform sampler2D Texture;
        layout (location = 0) out vec4 Out_Color;
        void main()
        {
            Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
        }";
#elif LEGACY
                @"#version 110
        varying vec2 Frag_UV;
        varying vec4 Frag_Color;

        uniform sampler2D Texture;

        void main()
        {
            gl_FragColor = Frag_Color * texture2D(Texture, Frag_UV.st);
        }";
#endif

            _shader = new Shader(_gl, vertexSource, fragmentSource);

            _attribLocationTex = _shader.GetUniformLocation("Texture");
            _attribLocationProjMtx = _shader.GetUniformLocation("ProjMtx");
            _attribLocationVtxPos = _shader.GetAttribLocation("Position");
            _attribLocationVtxUV = _shader.GetAttribLocation("UV");
            _attribLocationVtxColor = _shader.GetAttribLocation("Color");

            _vboHandle = _gl.GenBuffer();
            _elementsHandle = _gl.GenBuffer();

            CreateFontsTexture();

            // Restore modified GL state
            _gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);
            _gl.BindBuffer(GLEnum.ArrayBuffer, (uint)lastArrayBuffer);

            _gl.BindVertexArray((uint)lastVertexArray);

            _gl.CheckGlError("End of ImGui setup");
        }

        /// <summary>
        /// Creates the texture used to render text.
        /// </summary>
        private unsafe void CreateFontsTexture()
        {
            // Build texture atlas (dummy texture until i implement a proper way)
            int width = 1;
            int height = 1;

            uint* pixels = AllocT<uint>(width * height);
            MemsetT(pixels, 0xffffffff, width * height);

            // Upload texture to graphics system
            _gl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);

            _fontTexture = new Texture(_gl, width, height, pixels);
            _fontTexture.Bind();
            _fontTexture.SetMagFilter(TextureMagFilter.Linear);
            _fontTexture.SetMinFilter(TextureMinFilter.Linear);

            // Store our identifier
            context.FontTextureId = (nint)_fontTexture.GlTexture;

            // Restore state
            _gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);
            Free(pixels);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            context.Dispose();

            _gl.DeleteBuffer(_vboHandle);
            _gl.DeleteBuffer(_elementsHandle);

            _fontTexture.Dispose();
            _shader.Dispose();
        }
    }
}