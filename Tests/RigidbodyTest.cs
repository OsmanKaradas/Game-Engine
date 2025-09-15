using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Physics;
using GameEngine.Graphics;
using OpenTK.Mathematics;
using ImGuiNET;

namespace GameEngine
{
    internal class RigidbodyTest : GameWindow
    {
        JoltPhysics physics = null!;
        ShaderProgram shader = null!;

        GameObject player = null!;
        Camera camera = null!;
        Rigidbody rigidbody = null!;
        Mesh cube = null!;
        float fps;
        int width, height;
        public RigidbodyTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;
            this.CenterWindow(new Vector2i(width, height));
            //this.WindowState = WindowState.Fullscreen;
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
            camera = new Camera(ClientSize.X, ClientSize.Y, new Vector3(0f, 0f, -3f), 40f);
            physics = new JoltPhysics();

            shader = new ShaderProgram("test.vert", "test.frag");

            rigidbody = new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Dynamic);
            rigidbody.Initialize(new Vector3(0f, 0f, 0f), Quaternion.Identity, new Vector3(1f, 1f, 1f));

            cube = new Mesh(World.Type.Cube);
            GameObject ground = new(cube, new(0f, -5f, 0f), Quaternion.Identity, new(new(0.75f, 0.75f, 0.75f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new(20f, 1f, 20f));

            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            shader.Render(camera);

            GameObject.Render(shader);

            cube.buffers.vao.Bind();
            cube.buffers.ibo.Bind();

            Matrix4 model = Matrix4.CreateFromQuaternion(new Quaternion(rigidbody.body.Rotation.X, rigidbody.body.Rotation.Y, rigidbody.body.Rotation.Z, rigidbody.body.Rotation.W)) * Matrix4.CreateTranslation(new Vector3(rigidbody.body.Position.X, rigidbody.body.Position.Y, rigidbody.body.Position.Z));
            shader.SetMatrix4("model", model);
            shader.SetVector3("inColor", new Vector3(1f, 0f, 0f));
            DrawElements(PrimitiveType.Triangles, cube.meshData.Indices.Count, DrawElementsType.UnsignedInt, 0);

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
            
            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }

            /*if (camera.mode == Camera.Mode.LookAround)
            {
                float moveSpeed = 8f * Time.deltaTime;
                System.Numerics.Vector3 force = System.Numerics.Vector3.Zero;
                 
                if (keyboardInput.IsKeyDown(Keys.LeftShift)) moveSpeed *= 2f;

                if (keyboardInput.IsKeyDown(Keys.W)) force.Z -= 1f;
                if (keyboardInput.IsKeyDown(Keys.S)) force.Z += 1f;
                if (keyboardInput.IsKeyDown(Keys.D)) force.X += 1f;
                if (keyboardInput.IsKeyDown(Keys.A)) force.X -= 1f;

                Console.WriteLine(physics.BodyInterface.IsActive(player.rigidbody.body.ID));

                player.rigidbody.body.AddForce(force * 50f);
            }*/
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