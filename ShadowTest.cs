using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Graphics;
using OpenTK.Mathematics;
using ImGuiNET;

namespace GameEngine
{
    internal class ShadowTest : GameWindow
    {
        Quad quad = null!;
        GeometryPass geometryPass = null!;
        ShadowPass shadowPass = null!;
        LightingPass lightingPass = null!;

        ImGuiController guiController = null!;
        Camera camera = null!;

        int width, height;
        public ShadowTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;
            this.CenterWindow(new Vector2i(width, height));
            this.WindowState = WindowState.Fullscreen;
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            width = e.Width;
            height = e.Height;

            Viewport(0, 0, width, height);

            if (camera != null)
            { camera.screenWidth = width; camera.screenHeight = height; }

            if (guiController != null)
                guiController.WindowResized(width, height);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            camera = new(width, height, new Vector3(0f, 0f, -3f), 40f);

            quad = new();

            geometryPass = new("GeometryPass/GeometryPass.vert", "GeometryPass/GeometryPass.frag", width, height);
            shadowPass = new("ShadowPass/ShadowPass.vert", "ShadowPass/ShadowPass.frag", "ShadowPass/ShadowPass.geom");
            lightingPass = new("LightingPass/LightingPass.vert", "LightingPass/LightingPass.frag", quad);

            Light.shader = lightingPass.shader;
            Light.camera = camera;
            DirectionalLight directionalLight = new(new(0.75f, 0.75f, 0.75f), new Vector3(-0.45f, -0.625f, -0.75f).Normalized());
            PointLight pointLight = new(new(0.5f, 0.5f, 0f), new(0f, 5f, 2f));

            guiController = new(width, height);

            Mesh cubeMesh = new(World.Type.Cube);
            Mesh dummyMesh = new("dummy2.glb");
            GameObject ground = new(cubeMesh, new Vector3(0f, -3.4f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), null, new Vector3(20f, 1f, 20f));
            GameObject wall = new(cubeMesh, new Vector3(0f, 0f, -4f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), null, new Vector3(20f, 10f, 1f));
            GameObject cube = new(cubeMesh, new Vector3(0f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)));
            GameObject dummy = new(dummyMesh, new Vector3(5f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), null, new(0.5f, 0.5f, 0.5f));

            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            //shadowPass.RenderPointLightShadows(width, height, Light.pointLights[0].position);
            shadowPass.RenderDirLightShadows();

            geometryPass.Render(camera);

            lightingPass.Render(geometryPass, shadowPass);

            // --- GUI ---
            guiController.Update(this, (float)args.Time);

            ImGui.SetNextWindowBgAlpha(0.75f);
            ImGui.Begin("FPS Overlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.Text($"FPS: {Time.fps}");
            ImGui.SliderFloat("lightDirX", ref Light.directionalLight.direction.X, -0.75f, 0.75f);
            ImGui.SliderFloat("lightDirY", ref Light.directionalLight.direction.Y, -1f, 0f);
            ImGui.SliderFloat("lightDirZ", ref Light.directionalLight.direction.Z, -1f, 0f);

            ImGui.SliderFloat("pointLightDirX", ref Light.pointLights[0].position.X, -10f, 10f);
            ImGui.SliderFloat("pointLightDirY", ref Light.pointLights[0].position.Y, -10f, 10f);
            ImGui.SliderFloat("pointLightDirZ", ref Light.pointLights[0].position.Z, -10f, 10f);
            ImGui.End();

            guiController.Render();

            Light.directionalLight.direction.Normalize();
            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            MouseState mouseInput = MouseState;
            KeyboardState keyboardInput = KeyboardState;

            Time.Update(args.Time);

            camera.Update(this, keyboardInput, mouseInput, args);
            GameObject.Update();

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
        }
        protected override void OnUnload()
        {
            base.OnUnload();

            ShaderProgram.Delete();
            GameObject.Delete();
        }
    }
}