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
        public ShaderProgram lightShader = null!;
        public ShaderProgram geometryShader = null!;

        // camera
        public Camera camera = null!;
        public GameObject player = null!;
        public GameObject lightObj = null!;
        public Light light = null!;

        Mesh cubeBlenderMesh = null!;
        Mesh sphereMesh = null!;
        int width;
        int height;
        Vector3 lightDirection = new Vector3(-0.2f, -1f, -0.3f);
        float fps;

        Texture diceDiffuseTex = null!;
        List<float> lightVertices = new List<float>
        { //     COORDINATES     //
            -0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,
            0.5f, -0.5f, -0.5f,
            0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f,
            0.5f,  0.5f, -0.5f,
            0.5f,  0.5f,  0.5f
        };

        List<uint> lightIndices = new List<uint>
        {
            0, 1, 2,
            0, 2, 3,

            0, 4, 7,
            0, 7, 3,

            3, 7, 6,
            3, 6, 2,

            2, 6, 5,
            2, 5, 1,

            1, 5, 4,
            1, 4, 0,

            4, 5, 6,
            4, 6, 7
        };

        VAO lightVAO = null!;
        VBO lightVBO = null!;
        IBO lightIBO = null!;

        FBO fbo = null!;

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

            fbo = new FBO(e.Width, e.Height);

            this.width = e.Width;
            this.height = e.Height;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            shader = new ShaderProgram("Default.vert", "Default.frag");
            lightShader = new ShaderProgram("light.vert", "light.frag");
            geometryShader = new ShaderProgram("GeometryPass.vert", "GeometryPass.frag");

            camera = new Camera(width, height, Vector3.Zero);
            physics = new JoltPhysics();

            fbo = new FBO(width, height);

            /*lightVAO = new VAO();
            lightVBO = new VBO(lightVertices);
            lightIBO = new IBO(lightIndices);            
            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            EnableVertexAttribArray(0);
            lightVAO.Unbind();
            lightVBO.Unbind();
            lightIBO.Unbind();*/

            light = new DirectionalLight(shader, camera, lightDirection);

            Mesh cubeMesh = new Mesh(World.Type.Cube);

            Mesh groundMesh = new Mesh("Ground.glb");
            GameObject ground = new GameObject(groundMesh, new Vector3(0f, -10f, -5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, true));
            ground.material.diffuseTex = new Texture("testGrid.png", TextureUnit.Texture0);

            player = new GameObject(cubeMesh, new Vector3(0f, 55f, -5f), Quaternion.Identity, new Material(new Vector3(1f, 0.25f, 0.25f)), new Rigidbody(Rigidbody.BodyType.Box, false));

            Mesh testDummyMesh = new Mesh("test_dummy.glb");
            GameObject testDummy = new GameObject(testDummyMesh, new Vector3(5f, 2f, -5f), Quaternion.Identity, new Material(new Vector3(1f, 0.25f, 0.25f)), new Rigidbody(Rigidbody.BodyType.Box, false));

            cubeBlenderMesh = new Mesh("cube.glb");
            GameObject dice = new GameObject(cubeBlenderMesh, new Vector3(0f, 60f, -5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, false));
            diceDiffuseTex = new Texture("Dice(Diffuse).png", TextureUnit.Texture0);
            dice.material.diffuseTex = diceDiffuseTex;

            Mesh rubixCubeMesh = new Mesh("rubixCube.glb");
            GameObject rubixCube = new GameObject(rubixCubeMesh, new Vector3(0f, 65f, -5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, false));
            rubixCube.material.diffuseTex = new Texture("RubixCube/rubixCube(Diffuse).png", TextureUnit.Texture0);
            rubixCube.material.specularTex = new Texture("RubixCube/rubixCube(Roughness).png", TextureUnit.Texture1);

            for (int i = 0; i < 10; i++)
            {
                GameObject cube = new GameObject(cubeMesh, new Vector3(0f, i * 5f, -5f), Quaternion.Identity, new Material(new Vector3(i / 10f, 0f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, false));
            }

            Enable(EnableCap.DepthTest);
            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            physics.Dispose();
            shader.Delete();
            lightShader.Delete();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fps = MathF.Round(1f / (float)args.Time);
            Console.WriteLine(fps);

            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            geometryShader.Render(camera);
            light.Render();

            GameObject.Render(geometryShader);

            /* LIGHT SHADER
            UseProgram(lightShader.ID);
            lightVAO.Bind();
            Matrix4 lightModel = Matrix4.Identity;
            lightModel = Matrix4.CreateTranslation(new Vector3(0f, 3f, 5f));
            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();
            UniformMatrix4(GetUniformLocation(lightShader.ID, "model"), false, ref lightModel);
            UniformMatrix4(GetUniformLocation(lightShader.ID, "view"), false, ref view);
            UniformMatrix4(GetUniformLocation(lightShader.ID, "projection"), false, ref projection);
            DrawElements(PrimitiveType.Triangles, lightIndices.Count, DrawElementsType.UnsignedInt, 0);
            lightVAO.Unbind();*/

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
                /*float moveSpeed = speed * (float)args.Time;

                if (input.IsKeyDown(Keys.LeftShift)) moveSpeed *= 2.5f;

                if (input.IsKeyDown(Keys.W)) lightObj.position.Z -= moveSpeed;
                if (input.IsKeyDown(Keys.S)) lightObj.position.Z += moveSpeed;
                if (input.IsKeyDown(Keys.A)) lightObj.position.X -= moveSpeed;
                if (input.IsKeyDown(Keys.D)) lightObj.position.X += moveSpeed;
                if (input.IsKeyDown(Keys.Space)) lightObj.position.Y += moveSpeed;
                if (input.IsKeyDown(Keys.X)) lightObj.position.Y -= moveSpeed;*/

                //player.rigidbody.Move(input);

            }

            /*System.Numerics.Vector3 camPos = new System.Numerics.Vector3(camera.position.X, camera.position.Y, camera.position.Z - 2.5f);
            physics.BodyInterface.SetPosition(player.rigidbody.bodyID, camPos,Activation.Activate);
            */

            if (input.IsKeyDown(Keys.J))
            {
                GameObject cubeblender = new GameObject(cubeBlenderMesh, new Vector3(5f, 60f, -5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f), diceDiffuseTex), new Rigidbody(Rigidbody.BodyType.Box, false));
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