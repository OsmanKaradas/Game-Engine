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

/*namespace GameEngine
{
    internal class Game : GameWindow
    {
        ShaderProgram program;
        Simulation simulation;
        BufferPool bufferPool;
        // camera
        Camera camera;
        GameObject player;
        GameObject ground;
        StaticHandle groundRigidbody;
        BodyHandle cubeRigidbody;
        BodyHandle cubeRigidbody1;
        GameObject cube;
        GameObject cube1;
        World.Mesh cubeMesh;
        public float speed = 10f;

        // transformation
        float yRot = 0f;

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
            
            bufferPool = new BufferPool();

            simulation = Simulation.Create(bufferPool, new BepuPhysicsSetup.NarrowPhaseCallbacks(), new BepuPhysicsSetup.PoseIntegratorCallbacks(new System.Numerics.Vector3(0f, -10f, 0f)), new SolveDescription(8, 1));
            program = new ShaderProgram("Default.vert", "Default.frag");
            Enable(EnableCap.DepthTest);
            camera = new Camera(width, height, Vector3.Zero);

            /*
            var groundPosition = new System.Numerics.Vector3(0f, -5f, 0f);
            var groundScale = new Vector3(50f, 1f, 50f);
            var groundBody = new Box(groundScale.X, groundScale.Y, groundScale.Z);
            groundRigidbody = simulation.Statics.Add(new StaticDescription(new RigidPose(groundPosition), simulation.Shapes.Add(groundBody)));

            var cubePosition = new System.Numerics.Vector3(0f, 25f, 0f);
            var cubePosition1 = new System.Numerics.Vector3(-0.5f, 20f, 0f);

            var cubeBody = new Box(1f, 1f, 1f);
            var cubeInertia = cubeBody.ComputeInertia(1f);
            cubeRigidbody = simulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(cubePosition), cubeInertia, simulation.Shapes.Add(cubeBody), 0.01f));
            cubeRigidbody1 = simulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(cubePosition1), cubeInertia, simulation.Shapes.Add(cubeBody), 0.01f));

            cubeMesh = new World.Mesh(World.Type.Cube);
            ground = new GameObject(cubeMesh, new Vector3(groundPosition.X, groundPosition.Y, groundPosition.Z), new Vector4(0f, 1f, 1f, 1f), true, simulation, new Vector3(groundScale.X, groundScale.Y, groundScale.Z));
            cube = new GameObject(cubeMesh, new Vector3(cubePosition.X, cubePosition.Y, cubePosition.Z), new Vector4(1f, 0f, 0f, 1f), false, simulation);
            cube1 = new GameObject(cubeMesh, new Vector3(cubePosition1.X, cubePosition1.Y, cubePosition1.Z), new Vector4(1f, 0f, 0f, 1f), false, simulation);
            

            cubeMesh = new World.Mesh(World.Type.Cube);

            World.Mesh testDummyMesh = new World.Mesh("Models/test_dummy.glb");

            GameObject testDummy = new GameObject(testDummyMesh, new Vector3(9f, 0f, -5f), new Vector4(1f, 1f, 0.5f, 1f), false, simulation, new Vector3(0.5f, 0.5f, 0.5f));
            player = new GameObject(cubeMesh, new Vector3(-2.25f, 3f, -5f), new Vector4(0f, 1f, 0f, 1f), false, simulation);

            GameObject cube = new GameObject(cubeMesh, new Vector3(-3f, 0f, -5f), new Vector4(0f, 0f, 1f, 1f), false, simulation);
            GameObject cube1 = new GameObject(cubeMesh, new Vector3(0f, 0f, -5f), new Vector4(0f, 0.5f, 1f, 1f), false, simulation);
            GameObject cube2 = new GameObject(cubeMesh, new Vector3(3f, 0f, -5f), new Vector4(0f, 1f, 1f, 1f), false, simulation);
            GameObject cube3 = new GameObject(cubeMesh, new Vector3(2.5f, 6f, -5f), new Vector4(0f, 1f, 1f, 1f), false, simulation);
            GameObject cube4 = new GameObject(cubeMesh, new Vector3(3f, 3f, -5f), new Vector4(0f, 1f, 1f, 1f), false, simulation);
            GameObject ground = new GameObject(cubeMesh, new Vector3(0f, -5f, -0f), new Vector4(0.75f, 0.75f, 0.75f, 1f), true, simulation, new Vector3(50f, 0.5f, 50f));
            ground.tag = "Ground";

            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            foreach (GameObject obj in GameObject.gameObjects)
            {
                obj.mesh.buffers.vao.Delete();
                obj.mesh.buffers.ibo.Delete();
                obj.mesh.buffers.vbo.Delete();
            }
            simulation.Dispose();
            bufferPool.Clear();
            program.Delete();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fps = MathF.Round(1f / (float)args.Time);
            Console.WriteLine(fps);

            ClearColor(Color4.White);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.Bind();

            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            yRot += 0.001f;

            int viewLocation = GetUniformLocation(program.ID, "view");
            int projectionLocation = GetUniformLocation(program.ID, "projection");

            UniformMatrix4(viewLocation, true, ref view);
            UniformMatrix4(projectionLocation, true, ref projection);

            //cube.position = new Vector3(simulation.Bodies[cubeRigidbody].Pose.Position.X, simulation.Bodies[cubeRigidbody].Pose.Position.Y, simulation.Bodies[cubeRigidbody].Pose.Position.Z);
            //cube1.position = new Vector3(simulation.Bodies[cubeRigidbody1].Pose.Position.X, simulation.Bodies[cubeRigidbody1].Pose.Position.Y, simulation.Bodies[cubeRigidbody1].Pose.Position.Z);

            foreach (GameObject obj in GameObject.gameObjects)
            {
                obj.Update();
                obj.Render(program);
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

            /*foreach (GameObject obj in GameObject.gameObjects)
            {
                obj.rigidbody.Update(Time.deltaTime, GameObject.gameObjects);
            }

            simulation.Timestep((float)args.Time);

            /*
            Vector3 inputVel = Vector3.Zero;
            if (!camera.cameraMode)
            {
                float moveSpeed = speed;
                if (input.IsKeyDown(Keys.LeftShift)) moveSpeed *= 2.5f;

                if (input.IsKeyDown(Keys.W)) inputVel.Z -= moveSpeed;
                if (input.IsKeyDown(Keys.S)) inputVel.Z += moveSpeed;
                if (input.IsKeyDown(Keys.A)) inputVel.X -= moveSpeed;
                if (input.IsKeyDown(Keys.D)) inputVel.X += moveSpeed;

                // keep Y velocity from rigidbody (gravity/jump)
                //player.rigidbody.velocity.X = inputVel.X;
                //player.rigidbody.velocity.Z = inputVel.Z;
                
                simulation.Bodies[cubeRigidbody].Velocity.Linear.X = inputVel.X;
                simulation.Bodies[cubeRigidbody].Velocity.Linear.Z = inputVel.Z;
            }

            if (input.IsKeyPressed(Keys.F))
            {
                camera.cameraMode = !camera.cameraMode;
            }

            camera.Update(input, mouse, args);

            // show cursor
            if (input.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
        }
    }
}*/


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


namespace GameEngine
{
    internal class Game : GameWindow
    {
        public static JoltPhysicsSample physicsSample;
        ShaderProgram program;
        // camera
        Camera camera;
        public GameObject player;
        public float speed = 10f;

        // transformation
        float yRot = 0f;

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
            physicsSample = new JoltPhysicsSetup();
            physicsSample.Initialize();

            Mesh cubeMesh = new Mesh(World.Type.Cube);
            GameObject ground = new GameObject(cubeMesh, new Vector3(0f, -10f, -5f), new Vector4(0.75f, 0.75f, 0.75f, 1f), true, new Vector3(50f, 1f, 50f));
            player = new GameObject(cubeMesh, new Vector3(0f, 0f, -5f), new Vector4(1f, 0.25f, 0.25f, 1f), false);

            for (int i = 0; i < 50; i++)
            {
                GameObject cube = new GameObject(cubeMesh, new Vector3(0f, i * 5f, -5f), new Vector4(0f, 1f, 0.25f, 1f), false);
            }

            program = new ShaderProgram("Default.vert", "Default.frag");
            Enable(EnableCap.DepthTest);
            camera = new Camera(width, height, Vector3.Zero);

            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            JoltPhysicsSharp.Foundation.Shutdown();
            program.Delete();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fps = MathF.Round(1f / (float)args.Time);
            Console.WriteLine(fps);

            ClearColor(Color4.White);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.Bind();

            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            int viewLocation = GetUniformLocation(program.ID, "view");
            int projectionLocation = GetUniformLocation(program.ID, "projection");

            UniformMatrix4(viewLocation, true, ref view);
            UniformMatrix4(projectionLocation, true, ref projection);

            foreach (GameObject obj in GameObject.gameObjects)
            {
                obj.Update();
                obj.Render(program);
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