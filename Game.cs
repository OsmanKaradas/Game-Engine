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
using JoltPhysicsSharp;

namespace GameEngine
{
    internal class Game : GameWindow
    {
        public static JoltPhysics physics = null!;
        public ShaderProgram shader = null!;

        // camera
        public Camera camera = null!;
        public GameObject player = null!;
        public GameObject lightObj = null!;
        public Light light = null!;
        public float speed = 10f;

        Mesh cubeBlenderMesh = null!;
        int width;
        int height;
        Vector3 lightDirection = new Vector3(-0.2f, -1f, -0.3f);
        float fps;

        //System.Numerics.Vector3 pos;
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

            shader = new ShaderProgram("light.vert", "light.frag");
            camera = new Camera(width, height, Vector3.Zero);
            physics = new JoltPhysics();

            Mesh sphereMesh = new Mesh(World.Type.Sphere);
            lightObj = new GameObject(sphereMesh, new Vector3(0f, 5f, 5f), new Material(new Vector3(1f, 1f, 1f)));
            light = new SpotLight(shader, lightObj);            
            Mesh cubeMesh = new Mesh(World.Type.Cube);
            Mesh groundMesh = new Mesh("Ground.glb");
            GameObject ground = new GameObject(groundMesh, new Vector3(0f, -10f, -5f), new Material(new Vector3(0.75f, 0.75f, 0.75f)), new Rigidbody(Rigidbody.BodyType.Box, true));
            
            player = new GameObject(cubeMesh, new Vector3(0f, 55f, -5f), new Material(new Vector3(1f, 0.25f, 0.25f)), new Rigidbody(Rigidbody.BodyType.Box, false));

            Mesh testDummyMesh = new Mesh("test_dummy.glb");
            GameObject testDummy = new GameObject(testDummyMesh, new Vector3(5f, 2f, -5f), new Material(new Vector3(1f, 0.25f, 0.25f)), new Rigidbody(Rigidbody.BodyType.Box, false));

            cubeBlenderMesh = new Mesh("cube.glb");
            GameObject cubeblender = new GameObject(cubeBlenderMesh, new Vector3(0f, 60f, -5f), new Material(new Vector3(1f, 1f, 0f)), new Rigidbody(Rigidbody.BodyType.Box, false));
            cubeblender.material.diffuseTex = new Texture("Dice(Diffuse).png", TextureUnit.Texture0);

            Mesh rubixCubeMesh = new Mesh("rubixCube.glb");
            GameObject rubixCube = new GameObject(rubixCubeMesh, new Vector3(0f, 65f, -5f), new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, false));
            rubixCube.material.diffuseTex = new Texture("RubixCube/rubixCube(Diffuse).png", TextureUnit.Texture0);
            rubixCube.material.specularTex = new Texture("RubixCube/rubixCube(Roughness).png", TextureUnit.Texture1);

            for (int i = 0; i < 10; i++)
            {
                GameObject cube = new GameObject(cubeMesh, new Vector3(0f, i * 5f, -5f),new Material(new Vector3(i / 10f, 0f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, false));
            }

            Enable(EnableCap.DepthTest);
            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            physics.Dispose();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fps = MathF.Round(1f / (float)args.Time);
            Console.WriteLine(fps);

            ClearColor(Color4.White);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Render(camera);
            light.Render();
            shader.SetVector3("viewPos", camera.position);

            GameObject.Render(shader);

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            MouseState mouse = MouseState;
            KeyboardState input = KeyboardState;
            base.OnUpdateFrame(args);

            Time.Update(args.Time);

            physics.System.Update(Time.deltaTime, 1, physics.JobSystem);
            
            if (!camera.cameraMode)
            {
                float moveSpeed = speed * (float)args.Time;

                if (input.IsKeyDown(Keys.LeftShift)) moveSpeed *= 2.5f;

                if (input.IsKeyDown(Keys.W)) lightObj.position.Z -= moveSpeed;
                if (input.IsKeyDown(Keys.S)) lightObj.position.Z += moveSpeed;
                if (input.IsKeyDown(Keys.A)) lightObj.position.X -= moveSpeed;
                if (input.IsKeyDown(Keys.D)) lightObj.position.X += moveSpeed;
                if (input.IsKeyDown(Keys.Space)) lightObj.position.Y += moveSpeed;
                if (input.IsKeyDown(Keys.X)) lightObj.position.Y -= moveSpeed;
            }

            if (input.IsKeyDown(Keys.J))
            {
                GameObject cubeblender = new GameObject(cubeBlenderMesh, new Vector3(5f, 60f, -5f), new Material(new Vector3(1f, 1f, 0f)), new Rigidbody(Rigidbody.BodyType.Box, false));
                cubeblender.material.diffuseTex = new Texture("Dice(Diffuse).png", TextureUnit.Texture0);
            }

            camera.Update(input, mouse, args);

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