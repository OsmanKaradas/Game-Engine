using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Physics;
using GameEngine.Graphics;
using OpenTK.Mathematics;

namespace GameEngine
{
    internal class Test : GameWindow
    {
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

        JoltPhysics physics = null!;
        FBO fbo = null!;
        Quad quad = null!;
        ShadowFBO shadowFBO = null!;
        ShaderProgram geometryShader = null!;
        ShaderProgram lightingShader = null!;
        ShaderProgram shadowShader = null!;
        ShaderProgram lightShader = null!;

        VAO lightVAO = null!;
        VBO lightVBO = null!;
        IBO lightIBO = null!;

        Matrix4 lightSpaceMatrix;
        Vector3 lightPos = new Vector3(0f, 5f, 0f);
        Vector3 lightDir = new Vector3(-0.2f, -1f, -0.5f);
        Mesh cubeMesh = null!;
        Camera camera = null!;
        int width;
        int height;
        public Test(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;
            this.CenterWindow(new Vector2i(width, height));
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            Viewport(0, 0, e.Width, e.Height);

            fbo?.Delete();
            fbo = new FBO(e.Width, e.Height);

            if (camera != null)
            { camera.SCREENWIDTH = e.Width; camera.SCREENHEIGHT = e.Height; }

            width = e.Width;
            height = e.Height;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            physics = new JoltPhysics();

            geometryShader = new ShaderProgram("GeometryPass.vert", "GeometryPass.frag");
            lightingShader = new ShaderProgram("LightingPass.vert", "LightingPass.frag");
            shadowShader = new ShaderProgram("ShadowPass.vert", "ShadowPass.frag");
            lightShader = new ShaderProgram("light.vert", "light.frag");

            fbo = new FBO(width, height);
            quad = new Quad();

            shadowFBO = new ShadowFBO();

            UseProgram(lightingShader.ID);
            lightingShader.SetInt("gPosition", 0);
            lightingShader.SetInt("gNormal", 1);
            lightingShader.SetInt("gMaterial", 2);
            lightingShader.SetInt("gDepth", 3);
            lightingShader.SetInt("shadowMap", 4);

            UseProgram(0);
            UseProgram(lightShader.ID);
            lightVAO = new VAO();
            lightVBO = new VBO(lightVertices);
            lightIBO = new IBO(lightIndices);
            lightVAO.Bind();
            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            EnableVertexAttribArray(0);

            lightVAO.Unbind();
            UseProgram(0);

            cubeMesh = new Mesh(World.Type.Cube);
            GameObject ground = new GameObject(cubeMesh, new Vector3(0f, -5f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, true), new Vector3(20f, 1f, 20f));
            GameObject player = new GameObject(cubeMesh, new Vector3(0f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, false));

            GameObject wall1 = new GameObject(cubeMesh, new Vector3(-8.5f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, true), new Vector3(3f, 30f, 1f));
            GameObject wall2 = new GameObject(cubeMesh, new Vector3(-4.5f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, true), new Vector3(3f, 30f, 1f));
            GameObject wall3 = new GameObject(cubeMesh, new Vector3(0f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, true), new Vector3(3f, 30f, 1f));
            GameObject wall4 = new GameObject(cubeMesh, new Vector3(4.5f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, true), new Vector3(3f, 30f, 1f));
            GameObject wall5 = new GameObject(cubeMesh, new Vector3(8.5f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, true), new Vector3(3f, 30f, 1f));
            Mesh dummyMesh = new Mesh("test_dummy.glb");
            GameObject dummy = new GameObject(dummyMesh, new Vector3(5f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, false), new Vector3(0.5f, 0.5f, 0.5f));

            camera = new Camera(width, height, new Vector3(0f, 0f, -3f));
            CursorState = CursorState.Grabbed;
            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // --- SHADOW RENDER ---
            Viewport(0, 0, shadowFBO.width, shadowFBO.height);
            shadowFBO.Bind();
            Clear(ClearBufferMask.DepthBufferBit);
            Enable(EnableCap.CullFace);
            CullFace(TriangleFace.Front);
            UseProgram(shadowShader.ID);
            float near = 1f, far = 20f;
            Matrix4 lightProjection = Matrix4.CreateOrthographic(20f, 20f, near, far);
            Matrix4 lightView = Matrix4.LookAt(-lightDir * 10f, Vector3.Zero, Vector3.UnitY);
            lightSpaceMatrix = lightProjection * lightView;
            UniformMatrix4(GetUniformLocation(shadowShader.ID, "lightSpaceMatrix"), false, ref lightSpaceMatrix);

            GameObject.Render(shadowShader);
            shadowFBO.Unbind();
            CullFace(TriangleFace.Back);
            
            // --- GEOMETRY RENDER ---
            fbo.Bind();
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            Viewport(0, 0, width, height);

            geometryShader.Render(camera);

            GameObject.Render(geometryShader);
    
            fbo.Unbind();

            // --- LIGHT RENDER ---
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, fbo.gPosition);
            ActiveTexture(TextureUnit.Texture1);
            BindTexture(TextureTarget.Texture2D, fbo.gNormal);
            ActiveTexture(TextureUnit.Texture2);
            BindTexture(TextureTarget.Texture2D, fbo.gMaterial);
            ActiveTexture(TextureUnit.Texture3);
            BindTexture(TextureTarget.Texture2D, fbo.gDepth);
            ActiveTexture(TextureUnit.Texture4);
            BindTexture(TextureTarget.Texture2D, shadowFBO.shadowMap);

            UseProgram(lightingShader.ID);

            UniformMatrix4(GetUniformLocation(lightingShader.ID, "lightSpaceMatrix"), false, ref lightSpaceMatrix);

            Vector3 lightColor = new Vector3(0.5f, 0f, 0.5f);
            lightingShader.SetVector3("viewPos", camera.position);
            lightingShader.SetVector3("directionalLight.direction", lightDir);
            lightingShader.SetVector3("directionalLight.color", new Vector3(0.5f, 0.5f, 0.5f));
            lightingShader.SetVector3("pointLights[0].position",lightPos);
            lightingShader.SetVector3("pointLights[0].color", lightColor);
            lightingShader.SetInt("pointLightCount", 1);

            quad.Render();

            // --- LIGHT OBJECT RENDER ---
            lightShader.Render(camera);
            lightVAO.Bind();
            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(lightPos);
            UniformMatrix4(GetUniformLocation(lightShader.ID, "model"), false, ref model);
            lightShader.SetVector3("color", lightColor);
            DrawElements(PrimitiveType.Triangles, lightIndices.Count, DrawElementsType.UnsignedInt, 0);

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

            camera.InputController(keyboardInput, mouseInput, args);

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }

            if (keyboardInput.IsKeyDown(Keys.D0))
            {
                GameObject cube = new GameObject(cubeMesh, new Vector3(0f, 50f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, false));
            }

            float moveSpeed = 8f * Time.deltaTime;
            if (keyboardInput.IsKeyDown(Keys.Up)) { lightPos.Z -= moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Down)) { lightPos.Z += moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Right)) { lightPos.X += moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Left)) { lightPos.X -= moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.V)) { lightPos.Y += moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.B)) { lightPos.Y -= moveSpeed; }

            if (keyboardInput.IsKeyDown(Keys.R)) { lightDir.X += moveSpeed * 0.1f; }
            if (keyboardInput.IsKeyDown(Keys.T)) { lightDir.X -= moveSpeed * 0.1f; }
            
            moveSpeed = 0f;
        }
        protected override void OnUnload()
        {
            base.OnUnload();

            physics.Dispose();
            geometryShader.Delete();
            lightingShader.Delete();
            fbo.Delete();
            quad.Delete();
            GameObject.Delete();
        }
    }
}