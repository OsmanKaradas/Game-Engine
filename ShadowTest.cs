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
    internal class ShadowTest : GameWindow
    {
        List<float> vertices = new List<float>
        {
            // front
            -0.5f, -0.5f,  0.5f,
            0.5f, -0.5f,  0.5f,
            0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            // back
            0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            0.5f,  0.5f, -0.5f,
            // left
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f,
            // right
            0.5f, -0.5f,  0.5f,
            0.5f, -0.5f, -0.5f,
            0.5f,  0.5f, -0.5f,
            0.5f,  0.5f,  0.5f,
            // top
            -0.5f,  0.5f,  0.5f,
            0.5f,  0.5f,  0.5f,
            0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            // bottom
            -0.5f, -0.5f, -0.5f,
            0.5f, -0.5f, -0.5f,
            0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f
        };

        List<uint> indices = new List<uint>
        {
            0,1,2,     0,2,3,      // front
            4,5,6,     4,6,7,      // back
            8,9,10,    8,10,11,    // left
            12,13,14,  12,14,15,  // right
            16,17,18,  16,18,19,  // top
            20,21,22,  20,22,23   // bottom
        };

        ShaderProgram shader = null!;
        ShaderProgram shadowShader = null!;
        ShaderProgram lightShader = null!;
        Camera camera = null!;

        ShadowFBO shadowFBO = null!;
        Quad quad = null!;

        Vector3 lightPos = new Vector3(-2f, 4f, -1f);
        int width;
        int height;

        VAO vao;
        VBO vbo;
        IBO ibo;

        public ShadowTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;
            this.CenterWindow(new Vector2i(width, height));
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            Viewport(0, 0, e.Width, e.Height);

            if (camera != null)
            { camera.SCREENWIDTH = e.Width; camera.SCREENHEIGHT = e.Height; }

            width = e.Width;
            height = e.Height;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            shader = new ShaderProgram("test.vert", "test.frag");
            shadowShader = new ShaderProgram("ShadowPass.vert", "ShadowPass.frag");
            lightShader = new ShaderProgram("LightingPassTest.vert", "LightingPassTest.frag");

            shadowFBO = new ShadowFBO();
            quad = new Quad();

            UseProgram(lightShader.ID);
            lightShader.SetInt("depthMap", 0);
            UseProgram(0);

            /*Mesh cubeMesh = new Mesh(World.Type.Cube);
            GameObject ground = new GameObject(cubeMesh, new Vector3(0f, -2f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), null, new Vector3(20f, 1f, 20f));
            GameObject cube = new GameObject(cubeMesh, new Vector3(0f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(.75f, .75f, .75f)));
            */
            vao = new VAO();
            vbo = new VBO(vertices);
            ibo = new IBO(indices);
            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            EnableVertexAttribArray(0);
            vao.Unbind();

            camera = new Camera(width, height, new Vector3(-2f, 2f, -5f));
            CursorState = CursorState.Grabbed;
            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            float near = 1.0f, far = 7.5f;
            Matrix4 lightProjection = Matrix4.CreateOrthographic(10.0f, 10.0f, near, far);
            Matrix4 lightView = Matrix4.LookAt(lightPos, Vector3.Zero, new Vector3(0f, 1f, 0f));
            Matrix4 lightSpaceMatrix = lightProjection * lightView;

            UseProgram(shadowShader.ID);
            shadowShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            Viewport(0, 0, shadowFBO.width, shadowFBO.height);
            shadowFBO.Bind();
            Clear(ClearBufferMask.DepthBufferBit);
            
            vao.Bind();
            // CUBE
            Matrix4 model = Matrix4.Identity;
            shadowShader.SetMatrix4("model", model);
            DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);

            // GROUND
            model = Matrix4.CreateTranslation(new Vector3(0f, -5f, 0f))  * Matrix4.CreateScale(20f, 1f, 20f);
            shadowShader.SetMatrix4("model", model);
            DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
            vao.Unbind();

            shadowFBO.Unbind();
            Viewport(0, 0, width, height);
            
            UseProgram(0);
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, shadowFBO.shadowMap);

            UseProgram(lightShader.ID);
            lightShader.SetFloat("near_plane", near);
            lightShader.SetFloat("far_plane", far);
            quad.Render();

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            MouseState mouseInput = MouseState;
            KeyboardState keyboardInput = KeyboardState;

            Time.Update(args.Time);

            camera.InputController(keyboardInput, mouseInput, args);

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            shadowFBO.Delete();
            quad.Delete();
            shadowShader.Delete();
            lightShader.Delete();
            GameObject.Delete();
        }
    }
}