using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static OpenTK.Graphics.OpenGL4.GL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GameEngine.Graphics;
namespace GameEngine
{
    internal class ShaderTest : GameWindow
    {
        ShaderProgram shader;
        Quad quad;
        int width, height;
        float timer = 0f;
        public ShaderTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width; this.height = height;
            Viewport(0, 0, width, height);
            this.CenterWindow(new Vector2i(width, height));
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            shader = new("ShaderTest/shaderTest.vert", "ShaderTest/shaderTest.frag");
            quad = new();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            timer += Time.deltaTime;

            UseProgram(shader.ID);
            shader.SetFloat("time", timer);
            shader.SetVector2("resolution", ClientSize);
            Console.WriteLine(timer);
            quad.Render();

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            Time.Update(args.Time);
        }

        protected override void OnUnload()
        {
            quad.Delete();
            base.OnUnload();
        }

    }
} 