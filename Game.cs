using static OpenTK.Graphics.OpenGL4.GL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using GameEngine.World;
using GameEngine.Physics;
using GameEngine.Graphics;

namespace GameEngine
{
    internal class Game : GameWindow
    {
        private int lights;
        public static JoltPhysics physics = null!;

        // SHADERS
        public ShaderProgram geometryShader = null!;
        public ShaderProgram lightingShader = null!;
        FBO fbo = null!;
        Quad quad = null!;

        public Camera camera = null!;
        public GameObject player = null!;
        public GameObject lightObj = null!;

        Mesh cubeMesh = null!;
        int width;
        int height;
        float fps;
        
        Texture diceDiffuseTex = null!;

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
            physics = new JoltPhysics();

            geometryShader = new ShaderProgram("GeometryPass.vert", "GeometryPass.frag");
            lightingShader = new ShaderProgram("LightingPass.vert", "LightingPass.frag");

            fbo = new FBO(width, height);
            quad = new Quad();

            UseProgram(lightingShader.ID);
            lightingShader.SetInt("gPosition", 0);
            lightingShader.SetInt("gNormal", 1);
            lightingShader.SetInt("gMaterial", 2);
            UseProgram(0);

            cubeMesh = new Mesh(World.Type.Cube);

            Mesh groundMesh = new Mesh("Ground.glb");
            GameObject ground = new GameObject(groundMesh, new Vector3(0f, -10f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, true));

            GameObject wallRight = new GameObject(cubeMesh, new Vector3(20f, 5f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, true), new Vector3(1f, 30f, 40f));
            GameObject wallLeft = new GameObject(cubeMesh, new Vector3(-20f, 5f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, true), new Vector3(1f, 30f, 40f));
            GameObject wallBack = new GameObject(cubeMesh, new Vector3(0f, 5f, -20f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, true), new Vector3(40f, 30f, 1f));
            GameObject wallFront = new GameObject(cubeMesh, new Vector3(0f, 5f, 20f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, true), new Vector3(40f, 30f, 1f));

            player = new GameObject(cubeMesh, new Vector3(0f, 55f, -5f), Quaternion.Identity, new Material(new Vector3(1f, 0.25f, 0.25f)), new Rigidbody(Rigidbody.BodyType.Box, false));

            Mesh testDummyMesh = new Mesh("test_dummy.glb");
            GameObject testDummy = new GameObject(testDummyMesh, new Vector3(5f, 2f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 0.25f, 0.25f)), new Rigidbody(Rigidbody.BodyType.Box, false));

            for (int i = 0; i < 10; i++)
            {
                GameObject cube = new GameObject(cubeMesh, new Vector3(0f, i * 5f, 0f), Quaternion.Identity, new Material(new Vector3(i / 10f, 0f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, false));
            }

            camera = new Camera(width, height, new Vector3(0f, 0f, -3f));
            Enable(EnableCap.DepthTest);
            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fbo.Bind();

            fps = (float)Math.Round(1 / args.Time);

            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            geometryShader.Render(camera);

            GameObject.Render(geometryShader);

            fbo.Unbind();

            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, fbo.gPosition);
            ActiveTexture(TextureUnit.Texture1);
            BindTexture(TextureTarget.Texture2D, fbo.gNormal);
            ActiveTexture(TextureUnit.Texture2);
            BindTexture(TextureTarget.Texture2D, fbo.gMaterial);

            UseProgram(lightingShader.ID);
            int pointLightCount = 0;
            int spotLightCount = 0;
            lightingShader.SetVector3("viewPos", camera.position);
            lightingShader.SetVector3("directionalLight.direction", new Vector3(-0.2f, -1f, -0.5f));
            lightingShader.SetVector3("directionalLight.color", new Vector3(0.5f, 0.5f, 0.5f));

            lightingShader.SetVector3("pointLights[0].position", new Vector3(0f, 5f, 15f));
            lightingShader.SetVector3("pointLights[0].color", new Vector3(0.75f, 0.25f, 0.5f));
            pointLightCount++;
            lightingShader.SetVector3("pointLights[1].position", new Vector3(0f, 5f, -15f));
            lightingShader.SetVector3("pointLights[1].color", new Vector3(0.25f, 0.75f, 0.5f));
            pointLightCount++;

            lightingShader.SetVector3("spotLights[0].position", new Vector3(0f, 0f, 0f));
            lightingShader.SetVector3("spotLights[0].direction", new Vector3(0f, -1f, 0f));
            lightingShader.SetVector3("spotLights[0].color", new Vector3(0f, 0f, 1f));
            lightingShader.SetFloat("spotLights[0].innerCone", MathF.Cos(MathHelper.DegreesToRadians(2f)));
            lightingShader.SetFloat("spotLights[0].outerCone", MathF.Cos(MathHelper.DegreesToRadians(40f)));
            spotLightCount++;
            lightingShader.SetInt("pointLightCount", pointLightCount);
            lightingShader.SetInt("spotLightCount", spotLightCount);

            quad.Render();

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            MouseState mouse = MouseState;
            KeyboardState input = KeyboardState;
            base.OnUpdateFrame(args);

            Console.WriteLine(fps);
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

            if (input.IsKeyDown(Keys.J))
            {
                GameObject cube = new GameObject(cubeMesh, new Vector3(0f, 60f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(Rigidbody.BodyType.Box, false));
            }

            camera.Update(input, mouse, args);

            if (input.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
        }
        
        protected override void OnUnload()
        {
            base.OnUnload();

            physics.Dispose();
            geometryShader.Delete();
            lightingShader.Delete();
            fbo.Delete();
            cubeMesh.buffers.Delete();
            quad.Delete();
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