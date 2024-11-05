using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.D3D11;
using Hexa.NET.ImGui.Backends.D3D12;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL2;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.ImGui.Backends.SDL2;
using Hexa.NET.ImGui.Backends.Vulkan;
using Silk.NET.Core.Contexts;
using Silk.NET.Windowing;

namespace Example
{
    public class ImGuiController : IDisposable
    {
        private bool _frameBegun;

        public ImGuiContextPtr Context;

        private enum WindowingBackendMode
        {
            None,
            SDL2,
            GLFW
        }

        private enum RendererBackendMode
        {
            None,
            OpenGL2,
            OpenGL3,
            D3D11,
            D3D12,
            Vulkan
        }

        private WindowingBackendMode windowMode;
        private RendererBackendMode rendererMode;

        public void MakeCurrent()
        {
            ImGui.SetCurrentContext(Context);
        }

        private unsafe void Init(IView view)
        {
            Context = ImGui.CreateContext();
            ImGui.SetCurrentContext(Context);
            ImGui.StyleColorsDark();

            var kind = view.Native!.Kind;

            if (kind.HasFlag(NativeWindowFlags.Glfw))
            {
                ImGuiImplGLFW.SetCurrentContext(Context);
                switch (rendererMode)
                {
                    case RendererBackendMode.OpenGL2:
                    case RendererBackendMode.OpenGL3:
                        ImGuiImplGLFW.InitForOpenGL(new GLFWwindowPtr((GLFWwindow*)view.Native.Glfw!.Value), true);
                        break;

                    case RendererBackendMode.D3D11:
                    case RendererBackendMode.D3D12:
                        ImGuiImplGLFW.InitForOther(new GLFWwindowPtr((GLFWwindow*)view.Native.Glfw!.Value), true);
                        break;

                    case RendererBackendMode.Vulkan:
                        ImGuiImplGLFW.InitForVulkan(new GLFWwindowPtr((GLFWwindow*)view.Native.Sdl!.Value), true);
                        break;
                }

                windowMode = WindowingBackendMode.GLFW;
            }

            if (kind.HasFlag(NativeWindowFlags.Sdl))
            {
                ImGuiImplSDL2.SetCurrentContext(Context);
                switch (rendererMode)
                {
                    case RendererBackendMode.OpenGL2:
                    case RendererBackendMode.OpenGL3:
                        ImGuiImplSDL2.InitForOpenGL(new SDLWindowPtr((SDLWindow*)view.Native.Sdl!.Value), (void*)view.GLContext!.Handle);
                        break;

                    case RendererBackendMode.D3D11:
                    case RendererBackendMode.D3D12:
                        ImGuiImplSDL2.InitForD3D(new SDLWindowPtr((SDLWindow*)view.Native.Sdl!.Value));
                        break;

                    case RendererBackendMode.Vulkan:
                        ImGuiImplSDL2.InitForVulkan(new SDLWindowPtr((SDLWindow*)view.Native.Sdl!.Value));
                        break;
                }

                windowMode = WindowingBackendMode.SDL2;
            }

            if (windowMode == WindowingBackendMode.None)
            {
                throw new NotSupportedException();
            }
        }

        public unsafe void InitForOpenGL2(IView view)
        {
            rendererMode = RendererBackendMode.OpenGL2;
            Init(view);
            ImGuiImplOpenGL2.SetCurrentContext(Context);
            ImGuiImplOpenGL2.Init();
        }

        public unsafe void InitForOpenGL3(IView view)
        {
            rendererMode = RendererBackendMode.OpenGL3;
            Init(view);
            ImGuiImplOpenGL3.SetCurrentContext(Context);
            ImGuiImplOpenGL3.Init((string)null!);
        }

        public unsafe void InitForD3D11(IView view, ID3D11DevicePtr device, ID3D11DeviceContextPtr context)
        {
            rendererMode = RendererBackendMode.D3D11;
            Init(view);
            ImGuiImplD3D11.SetCurrentContext(Context);
            ImGuiImplD3D11.Init(device, context);
        }

        public unsafe void InitForD3D12(IView view, ID3D12DevicePtr device, int numFramesInFlight, uint rtvFormat, ID3D12DescriptorHeapPtr cbvSrvHeap, D3D12CpuDescriptorHandle fontSrvCpuDescHandle, D3D12GpuDescriptorHandle fontSrvGpuDescHandle)
        {
            rendererMode = RendererBackendMode.D3D12;
            Init(view);
            ImGuiImplD3D12.SetCurrentContext(Context);
            ImGuiImplD3D12.Init(device, numFramesInFlight, rtvFormat, cbvSrvHeap, fontSrvCpuDescHandle, fontSrvGpuDescHandle);
        }

        public unsafe void InitForVulkan(IView view, ImGuiImplVulkanInitInfoPtr info)
        {
            rendererMode = RendererBackendMode.Vulkan;
            Init(view);
            ImGuiImplVulkan.SetCurrentContext(Context);
            ImGuiImplVulkan.Init(info);
        }

        public void Render()
        {
            if (_frameBegun)
            {
                var oldCtx = ImGui.GetCurrentContext();

                if (oldCtx != Context)
                {
                    ImGui.SetCurrentContext(Context);
                }

                _frameBegun = false;
                ImGui.Render();
                switch (rendererMode)
                {
                    case RendererBackendMode.OpenGL2:
                        ImGuiImplOpenGL2.RenderDrawData(ImGui.GetDrawData());
                        break;

                    case RendererBackendMode.OpenGL3:
                        ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
                        break;

                    case RendererBackendMode.D3D11:
                        ImGuiImplD3D11.RenderDrawData(ImGui.GetDrawData());
                        break;
                }

                if (oldCtx != Context)
                {
                    ImGui.SetCurrentContext(oldCtx);
                }
            }
        }

        public void RenderD3D12(ID3D12GraphicsCommandListPtr commandList)
        {
            if (_frameBegun)
            {
                var oldCtx = ImGui.GetCurrentContext();

                if (oldCtx != Context)
                {
                    ImGui.SetCurrentContext(Context);
                }

                _frameBegun = false;
                ImGui.Render();
                ImGuiImplD3D12.RenderDrawData(ImGui.GetDrawData(), commandList);

                if (oldCtx != Context)
                {
                    ImGui.SetCurrentContext(oldCtx);
                }
            }
        }

        public void RenderVk(VkCommandBuffer commandBuffer, VkPipeline pipeline)
        {
            if (_frameBegun)
            {
                var oldCtx = ImGui.GetCurrentContext();

                if (oldCtx != Context)
                {
                    ImGui.SetCurrentContext(Context);
                }

                _frameBegun = false;
                ImGui.Render();
                ImGuiImplVulkan.RenderDrawData(ImGui.GetDrawData(), commandBuffer, pipeline);

                if (oldCtx != Context)
                {
                    ImGui.SetCurrentContext(oldCtx);
                }
            }
        }

        public void Update()
        {
            var oldCtx = ImGui.GetCurrentContext();

            if (oldCtx != Context)
            {
                ImGui.SetCurrentContext(Context);
            }

            if (_frameBegun)
            {
                ImGui.Render();
            }

            _frameBegun = true;
            ImGuiImplOpenGL3.NewFrame();
            switch (rendererMode)
            {
                case RendererBackendMode.OpenGL2:
                    ImGuiImplOpenGL2.NewFrame();
                    break;

                case RendererBackendMode.OpenGL3:
                    ImGuiImplOpenGL3.NewFrame();
                    break;

                case RendererBackendMode.D3D11:
                    ImGuiImplD3D11.NewFrame();
                    break;

                case RendererBackendMode.D3D12:
                    ImGuiImplD3D12.NewFrame();
                    break;

                case RendererBackendMode.Vulkan:
                    ImGuiImplVulkan.NewFrame();
                    break;
            }

            switch (windowMode)
            {
                case WindowingBackendMode.SDL2:
                    ImGuiImplSDL2.NewFrame();
                    break;

                case WindowingBackendMode.GLFW:
                    ImGuiImplGLFW.NewFrame();
                    break;
            }
            ImGui.NewFrame();

            if (oldCtx != Context)
            {
                ImGui.SetCurrentContext(oldCtx);
            }
        }

        public void Dispose()
        {
            switch (rendererMode)
            {
                case RendererBackendMode.OpenGL2:
                    ImGuiImplOpenGL2.Shutdown();
                    break;

                case RendererBackendMode.OpenGL3:
                    ImGuiImplOpenGL3.Shutdown();
                    break;

                case RendererBackendMode.D3D11:
                    ImGuiImplD3D11.Shutdown();
                    break;

                case RendererBackendMode.D3D12:
                    ImGuiImplD3D12.Shutdown();
                    break;

                case RendererBackendMode.Vulkan:
                    ImGuiImplVulkan.Shutdown();
                    break;
            }

            switch (windowMode)
            {
                case WindowingBackendMode.SDL2:
                    ImGuiImplSDL2.Shutdown();
                    break;

                case WindowingBackendMode.GLFW:
                    ImGuiImplGLFW.Shutdown();
                    break;
            }

            ImGui.DestroyContext(Context);
            GC.SuppressFinalize(this);
        }
    }
}