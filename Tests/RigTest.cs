using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Graphics;
using OpenTK.Mathematics;
using GameEngine.Animation;

namespace GameEngine
{
    internal class RigTest : GameWindow
    {
        ShaderProgram shader = null!;
        ShaderProgram debugShader = null!;
        
        Camera camera = null!;
        World.Mesh mesh = null!;
        GameObject dummy = null!;
        Armature armature = null!;
        Animator animator = null!;

        int debugVao, debugVbo;

        int width; int height;
        public RigTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width; this.height = height;
            Viewport(0, 0, width, height);
            this.CenterWindow(new Vector2i(width, height));
            //WindowState = WindowState.Fullscreen;
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            width = e.Width; height = e.Height;

            Viewport(0, 0, width, height);

            if (camera != null)
            { camera.screenWidth = width; camera.screenHeight = height; }
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            camera = new(this, width, height, new Vector3(0f, 0f, -3f), 40f);
            shader = new("rig.vert", "rig.frag");
            debugShader = new("Debug/debug.vert", "Debug/debug.frag");

            GL.GenVertexArrays(1, out debugVao);
            GL.GenBuffers(1, out debugVbo);

            GL.BindVertexArray(debugVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, debugVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 2048 * Vector3.SizeInBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.BindVertexArray(0);

            var scene = SharpGLTF.Schema2.ModelRoot.Load("Models/mixamoAnim.glb");

            World.Mesh dummyMesh = new(scene.LogicalMeshes[0]);
            dummy = new(dummyMesh, new(0f, 0f, 0f), Quaternion.Identity, new(0.1f), new(new(0.3f, 0.3f, 0.3f)), null, new(scene.LogicalSkins[0]));

            animator = new(dummy.armature);
            animator.AddAnimation(scene.LogicalAnimations[0]);
            animator.AddAnimation(scene.LogicalAnimations[1]);
            animator.animations["Idle"].loop = true;
            animator.animations["Walk"].loop = true;
            
            foreach (var anim in animator.animations)
                Console.WriteLine(anim.Key);
            animator.Play(animator.animations["Walk"]);

            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Viewport(0, 0, width, height);
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            shader.SetVector3("viewPos", camera.position);
            shader.Render(camera);
            GameObject.Render(shader);
            
            UseProgram(debugShader.ID);
            debugShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            debugShader.SetMatrix4("view", camera.GetViewMatrix());
            debugShader.SetMatrix4("model", Matrix4.Identity);
            debugShader.SetVector3("inColor", new(1, 0, 0));

            // Get bone lines
            List<Vector3> lines = dummy.armature.GetBoneDebugLines();
            if (lines.Count > 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, debugVbo);
                GL.BufferData(BufferTarget.ArrayBuffer, lines.Count * Vector3.SizeInBytes, lines.ToArray(), BufferUsageHint.DynamicDraw);

                GL.BindVertexArray(debugVao);
                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, 0, lines.Count);
                GL.BindVertexArray(0);
            }

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            MouseState mouseInput = MouseState;
            KeyboardState keyboardInput = KeyboardState;

            Time.Update(args.Time);
            camera.Update(keyboardInput, mouseInput, args);
            GameObject.Update();
            Animator.Update();

            if (keyboardInput.IsKeyDown(Keys.P))
            {
                animator.Play(animator.animations.Values.First());
            }

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
        }
        protected override void OnUnload()
        {
            base.OnUnload();

            ShaderProgram.Delete();
            GameObject.Delete();
        }


    }
}