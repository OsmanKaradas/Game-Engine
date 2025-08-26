using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Graphics;
using OpenTK.Mathematics;

namespace GameEngine
{
    internal class Test : GameWindow
    {
        FBO fbo = null!;
        Quad quad = null!;

        Texture diffuseTex = null!;
        ShaderProgram geometryShader = null!;
        ShaderProgram lightingShader = null!;

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

            width = e.Width;
            height = e.Height;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            diffuseTex = new Texture("testGrid.png", TextureUnit.Texture2);

            geometryShader = new ShaderProgram("GeometryPass.vert", "GeometryPass.frag");
            lightingShader = new ShaderProgram("LightingPass.vert", "LightingPass.frag");

            fbo = new FBO(width, height);
            quad = new Quad();
            
            UseProgram(lightingShader.ID);
            lightingShader.SetInt("gPosition", 0);
            lightingShader.SetInt("gNormal", 1);
            lightingShader.SetInt("gMaterial", 2);
            UseProgram(0);

            Mesh cubeMesh = new Mesh(World.Type.Cube);
            GameObject cube = new GameObject(cubeMesh, new Vector3(5f, 0f, 5f), Quaternion.Identity, new Material(new Vector3(1.0f, 0.0f, 0.0f)));
            GameObject ground = new GameObject(cubeMesh, new Vector3(0f, -1f, 0f), Quaternion.Identity, new Material(new Vector3(0.75f, 0.75f, 0.75f)), null, new Vector3(20f, 1f, 20f));

            camera = new Camera(width, height, new Vector3(-4f, 2f, 5f));
            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fbo.Bind();
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            
            Enable(EnableCap.DepthTest);

            geometryShader.Render(camera);

            diffuseTex.Bind();

            Matrix4 pyramidModel = Matrix4.Identity;


            Uniform3(GetUniformLocation(geometryShader.ID, "inColor"), new Vector3(1.0f, 0.25f, 0.5f));
            UniformMatrix4(GetUniformLocation(geometryShader.ID, "model"), false, ref pyramidModel);
            DrawElements(PrimitiveType.Triangles, 18, DrawElementsType.UnsignedInt, 0);

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

            lightingShader.SetVector3("viewPos", camera.position);
            lightingShader.SetVector3("lights[0].Position", new Vector3(0f, 5f, 2f));
            lightingShader.SetVector3("lights[0].Color", new Vector3(1.25f, 1.25f, 1.25f));
            lightingShader.SetVector3("lights[1].position", new Vector3(-4f, 5f, 0f));
            lightingShader.SetVector3("lights[1].color", new Vector3(1f, 1f, 1f));

            quad.Render();
            UseProgram(0);

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            MouseState mouseInput = MouseState;
            KeyboardState keyboardInput = KeyboardState;

            camera.InputController(keyboardInput, mouseInput, args);

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            geometryShader.Delete();
            lightingShader.Delete();
            fbo.Delete();
            quad.Delete();
        }
    }
}