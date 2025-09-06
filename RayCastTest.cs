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

namespace GameEngine
{
    internal class RayCastTest : GameWindow
    {
        JoltPhysics physics = null!;
        ShaderProgram shader = null!;

        Mesh sphere = null!;
        Mesh capsule = null!;
        Camera camera = null!;

        Player player = null!;

        int width, height;
        public RayCastTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            //this.WindowState = WindowState.Fullscreen;
            this.width = width;
            this.height = height;
            this.CenterWindow(new Vector2i(width, height));

        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            Viewport(0, 0, e.Width, e.Height);

            width = e.Width;
            height = e.Height;

            if (camera != null)
            { camera.screenWidth = e.Width; camera.screenHeight = e.Height; }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            camera = new(ClientSize.X, ClientSize.Y, new Vector3(0f, 0f, -3f), 40f);
            physics = new();

            shader = new("test.vert", "test.frag");
            
            Mesh cube = new(World.Type.Cube);
            sphere = new(World.Type.Sphere);
            capsule = new("capsule.glb");

            GameObject ground = new(cube, new Vector3(0f, -5f, 0f), Quaternion.Identity, new Material(new Vector3(0.75f, 0.75f, 0.75f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(20f, 1f, 20f));
            GameObject wall = new(cube, new Vector3(0f, 0f, 10f), Quaternion.Identity, new Material(new Vector3(0.5f, 0.5f, 0.5f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(20f, 10f, 1f));

            GameObject playerObject = new(capsule, new(0f, 0f, 0f), Quaternion.Identity, new(new(0f, 0f, 1f)));
            player = new(playerObject, new CapsuleShape(new(1.8f, 0.9f)), 60f, camera, physics);

            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            shader.Render(camera);
            GameObject.Render(shader);

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

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
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