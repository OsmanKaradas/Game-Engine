using System;
using OpenTK;
using FreeTypeSharp;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Physics;

namespace GameEngine
{
    internal class Game : GameWindow
    {
        public static JoltPhysicsSample physicsSample = new JoltPhysicsSetup();
        public ShaderProgram shader;

        // camera
        Camera camera;
        public GameObject player;
        public GameObject light;
        public float speed = 10f;

        int width;
        int height;

        float fps;

        public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;
            this.CenterWindow(new Vector2i(width, height));
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            Viewport(0, 0, e.Width, e.Height);

            this.width = e.Width;
            this.height = e.Height;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            JoltPhysicsSharp.Foundation.Init();
            physicsSample.Initialize();

            Mesh cubeMesh = new Mesh(World.Type.Cube);
            GameObject ground = new GameObject(cubeMesh, new Vector3(0f, -10f, -5f), new Vector3(0.75f, 0.75f, 0.75f), true, new Vector3(50f, 1f, 50f));
            player = new GameObject(cubeMesh, new Vector3(0f, 55f, -5f), new Vector3(1f, 0.25f, 0.25f), false);

            Mesh testDummyMesh = new Mesh("test_dummy.glb");
            GameObject testDummy = new GameObject(testDummyMesh, new Vector3(5f, 2f, -5f), new Vector3(0f, 1f, 0.25f), false, new Vector3(0.5f, 0.5f, 0.5f));

            Mesh cubeBlenderMesh = new Mesh("cube.glb");
            GameObject cubeblender = new GameObject(cubeBlenderMesh, new Vector3(0f, 60f, -5f), new Vector3(1f, 1f, 0f), false);

            Mesh rubixCubeMesh = new Mesh("rubixCube.glb");
            GameObject rubixCube = new GameObject(rubixCubeMesh, new Vector3(0f, 65f, -5f), new Vector3(1f, 1f, 1f), false);
            rubixCube.material.diffuse = new Texture("RubixCube/rubixCube(Diffuse).png", TextureUnit.Texture0);
            rubixCube.material.diffuse = new Texture("RubixCube/rubixCube(Roughness).png", TextureUnit.Texture1);
            for (int i = 0; i < 10; i++)
            {
                GameObject cube = new GameObject(cubeMesh, new Vector3(0f, i * 5f, -5f), new Vector3(i / 10f, 0f, 1f), false);
            }

            light = new GameObject(new Mesh(World.Type.Sphere), new Vector3(0f, 0f, 5f), new Vector3(1f, 1f, 1f), true);

            shader = new ShaderProgram("light.vert", "light.frag");
            shader.SetInt("material.diffuseTex", 0);
            shader.SetInt("material.specular", 1);
            Enable(EnableCap.DepthTest);
            camera = new Camera(width, height, Vector3.Zero);

            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            JoltPhysicsSharp.Foundation.Shutdown();
            shader.Delete();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fps = MathF.Round(1f / (float)args.Time);
            Console.WriteLine(fps);

            ClearColor(Color4.White);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Bind();

            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            int viewLocation = GetUniformLocation(shader.ID, "view");
            int projectionLocation = GetUniformLocation(shader.ID, "projection");

            shader.SetVector3("viewPos", camera.position);

            shader.SetVector3("light.position", light.position);
            shader.SetVector3("light.ambient", new Vector3(0.75f, 0.75f, 0.75f));
            shader.SetVector3("light.diffuse", new Vector3(0.5f, 0.5f, 0.5f));
            shader.SetVector3("light.specular", new Vector3(0.75f, 0.75f, 0.75f));

            UniformMatrix4(viewLocation, true, ref view);
            UniformMatrix4(projectionLocation, true, ref projection);

            foreach (GameObject obj in GameObject.gameObjects)
            {
                obj.Update();
                obj.Render(shader);
            }

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            MouseState mouse = MouseState;
            KeyboardState input = KeyboardState;
            base.OnUpdateFrame(args);

            Time.Update(args.Time);

            physicsSample.System.Update(Time.deltaTime, 1, physicsSample.JobSystem);

            if (!camera.cameraMode)
            {
                float moveSpeed = speed;
                System.Numerics.Vector3 velocity = physicsSample.BodyInterface.GetLinearVelocity(player.joltRigidbody.bodyID);

                if (input.IsKeyDown(Keys.LeftShift)) moveSpeed *= 2.5f;

                if (input.IsKeyDown(Keys.W)) physicsSample.BodyInterface.AddForce(player.joltRigidbody.bodyID, new System.Numerics.Vector3(0f, 0f, moveSpeed));
                if (input.IsKeyDown(Keys.S)) physicsSample.BodyInterface.AddForce(player.joltRigidbody.bodyID, new System.Numerics.Vector3(0f, 0f, -moveSpeed));
                if (input.IsKeyDown(Keys.A)) physicsSample.BodyInterface.AddForce(player.joltRigidbody.bodyID, new System.Numerics.Vector3(-moveSpeed, 0f, 0f));
                if (input.IsKeyDown(Keys.D)) physicsSample.BodyInterface.AddForce(player.joltRigidbody.bodyID, new System.Numerics.Vector3(moveSpeed, 0f, 0f));
            }

            if (input.IsKeyPressed(Keys.F))
                camera.cameraMode = !camera.cameraMode;

            camera.Update(input, mouse, args);
            // show cursor
            if (input.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
        }
    }
}

/* Stairway
for (int y = 0; y < 10; y++)
{
    for (int x = 0; x < 10; x++)
    {
        translation = Matrix4.CreateTranslation(x, 3f, y);
        model *= translation;
        UniformMatrix4(modelLocation, true, ref model);
        DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
    }
    translation = Matrix4.CreateTranslation(y, 0f, 0f);
    model *= translation;
}*/