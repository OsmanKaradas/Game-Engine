using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Physics;
using GameEngine.Graphics;
using OpenTK.Mathematics;
using JoltPhysicsSharp;
using ImGuiNET;

namespace GameEngine
{
    internal class LightTest : GameWindow
    {
        JoltPhysics physics = null!;
        ShaderProgram shader = null!;

        Mesh cube = null!;
        Mesh capsule = null!;
        Camera camera = null!;

        Player player = null!;

        GameObject lightObj = null!;
        Vector3 lightDir = new(0.45f, -0.625f, 0.75f);
        Vector3 lightPos = new(0f, 5f, 0f);

        ImGuiController guiController = null!;
        int width, height;
        public LightTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            //this.WindowState = WindowState.Fullscreen;
            this.width = width;
            this.height = height;
            this.CenterWindow(new Vector2i(width, height));
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            width = e.Width;
            height = e.Height;

            Viewport(0, 0, width, height);

            if (camera != null)
            { camera.screenWidth = width; camera.screenHeight = height; }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            camera = new(width, height, new Vector3(0f, 0f, -3f), 40f);
            physics = new();

            shader = new("test.vert", "test.frag");

            guiController = new(width, height);

            cube = new(World.Type.Cube);
            capsule = new("capsule.glb");
            Mesh sphere = new(World.Type.Sphere);
            Mesh dummy = new("dummy2.glb");

            GameObject ground = new(cube, new Vector3(0f, -4f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(100f, 1f, 100f));
            GameObject wall = new(cube, new Vector3(0f, 0f, 10f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(20f, 10f, 1f));

            GameObject playerObject = new(capsule, new(0f, 0f, 0f), Quaternion.Identity, new(new(0f, 0f, 1f)));
            player = new(playerObject, new CapsuleShape(new(1.8f, 0.9f)), 60f, camera, physics);

            GameObject dummyObject = new(dummy, new(-5f, 0f, 0f), new(0f, 1f, 0f, 0f), new(new(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Kinematic), new(0.5f, 0.5f, 0.5f));

            lightObj = new(sphere, new(0f, 5f, -2f), Quaternion.Identity, new(new(0f, 0f, 0f)));

            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            shader.SetVector3("viewPos", camera.position);
            shader.SetVector3("directionalLight.color", new(0.75f, 0.75f, 0.75f));
            shader.SetVector3("directionalLight.direction", lightDir);
            
            shader.SetVector3("pointLight.color", lightObj.material.color);
            shader.SetVector3("pointLight.position", lightObj.position);
            shader.SetFloat("pointLight.linear", 0.045f);
            shader.SetFloat("pointLight.quadratic", 0.0075f);

            shader.Render(camera);
            GameObject.Render(shader);

            shader.Render(camera);

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            MouseState mouseInput = MouseState;
            KeyboardState keyboardInput = KeyboardState;

            Time.Update(args.Time);
            physics.System.Update(Time.deltaTime, 1, physics.JobSystem);
            camera.Update(this, keyboardInput, mouseInput, args);
            GameObject.Update();

            if (camera.mode == Camera.Mode.LookAround)
            {
                player.Update(keyboardInput);
            }

            if (keyboardInput.IsKeyDown(Keys.D0))
            {
                GameObject cubeObj = new(cube, new(0f, 25f, 0f), Quaternion.Identity, new(new(1f, 0f, 0f)), new(physics, Rigidbody.BodyType.Box, MotionType.Dynamic));
            }

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
            
            if(camera.mode == Camera.Mode.Locked)
                if(keyboardInput.IsKeyDown(Keys.W)){ lightObj.position.Z += 5f * Time.deltaTime; }
                if(keyboardInput.IsKeyDown(Keys.A)){ lightObj.position.X += 5f * Time.deltaTime; }
                if(keyboardInput.IsKeyDown(Keys.S)){ lightObj.position.Z -= 5f * Time.deltaTime; }
                if(keyboardInput.IsKeyDown(Keys.D)){ lightObj.position.X -= 5f * Time.deltaTime; }
        }
        protected override void OnUnload()
        {
            base.OnUnload();

            physics.Dispose();
            ShaderProgram.Delete();
            GameObject.Delete();
        }
    }
}