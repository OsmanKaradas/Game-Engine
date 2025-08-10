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
using JoltPhysicsSharp;

namespace GameEngine
{
    internal class Game : GameWindow
    {
        // Jolt Physics
        PhysicsSystem physicsSystem;
        JobSystem jobSystem;
        PhysicsSystemSettings settings;
        ShaderProgram program;
        Texture texture;
        
        // camera
        Camera camera;

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

            Foundation.Init();
            jobSystem = new JobSystemThreadPool();
            settings = new PhysicsSystemSettings();            
            physicsSystem = new PhysicsSystem(settings);

            Mesh cubeMesh = new Mesh(World.Type.Cube);

            GameObject cube = new GameObject(cubeMesh, new Vector3(-3f, 0f, -5f), new Vector3(1f, 0f, 0f));
            GameObject cube1 = new GameObject(cubeMesh, new Vector3(0f, 0f, -5f), new Vector3(1f, 0.5f, 0f));
            GameObject cube2 = new GameObject(cubeMesh, new Vector3(3f, 0f, -5f), new Vector3(1f,1f, 0f));
            GameObject ground = new GameObject(cubeMesh, new Vector3(0f, -5f, -0f), new Vector3(0f, 0.25f, 1f));
            ground.scale = new Vector3(50f, 0.5f, 50f);
            BodyInterface bodyInterface = physicsSystem.BodyInterface;
            var boxShape = new BoxShape(new System.Numerics.Vector3(10f, 10f, 10f), 0.1f);
            var bodySettings = new BodyCreationSettings(
                boxShape,
                new System.Numerics.Vector3(0f, -3f, 0f),
                System.Numerics.Quaternion.Identity,
                MotionType.Dynamic,
                new ObjectLayer(0)
            );
            Body body = bodyInterface.CreateBody(bodySettings);
            bodyInterface.AddBody(body, Activation.Activate);

            program = new ShaderProgram("Default.vert", "Default.frag");

            texture = new Texture("dirtTexture.JPG");
            Enable(EnableCap.DepthTest);

            camera = new Camera(width, height, Vector3.Zero);
            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            foreach (GameObject obj in GameObject.gameObjects)
            {
                Console.WriteLine(obj);
                obj.mesh.vao.Delete();
                obj.mesh.ibo.Delete();
                obj.mesh.vbo.Delete();
            }

            texture.Delete();
            program.Delete();
            Foundation.Shutdown();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fps = MathF.Round(1f / (float)args.Time);

            ClearColor(Color4.White);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.Bind();
            texture.Bind();


            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            yRot += 0.001f;

            Matrix4 translation = Matrix4.CreateTranslation(0f, 0f, -3f);

            int viewLocation = GetUniformLocation(program.ID, "view");
            int projectionLocation = GetUniformLocation(program.ID, "projection");

            UniformMatrix4(viewLocation, true, ref view);
            UniformMatrix4(projectionLocation, true, ref projection);

            foreach (GameObject obj in GameObject.gameObjects)
            {
                if (obj != GameObject.gameObjects[3])
                {
                    obj.rotation = new Vector3(yRot, 0f, 0f);
                }
                obj.Render(program);
            }

            /*for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    foreach (GameObject obj in GameObject.gameObjects)
                    {
                        obj.position += new Vector3(x * 0.00001f);
                        obj.Render(program);
                    }
                }
                foreach (GameObject obj in GameObject.gameObjects)
                {
                    obj.position += new Vector3(y * 0.00001f, 0f, 0f);
                    obj.Render(program);
                }
            }*/

            // Console.WriteLine(fps);

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            MouseState mouse = MouseState;
            KeyboardState input = KeyboardState;

            base.OnUpdateFrame(args);

            physicsSystem.Update((float)args.Time, 1, jobSystem);
            camera.Update(input, mouse, args);

            // show cursor
            if (input.IsKeyDown(Keys.Escape))
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