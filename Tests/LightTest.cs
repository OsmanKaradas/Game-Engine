using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Physics;
using GameEngine.Graphics;
using OpenTK.Mathematics;
using JoltPhysicsSharp;
using ImGuiNET;

namespace GameEngine
{
    internal class LightTest : GameWindow
    {
        JoltPhysics physics = null!;
        ShaderProgram shader = null!;
        ShaderProgram shadowShader = null!;

        Mesh cube = null!;
        Camera camera = null!;

        Player player = null!;
        BodyID selectedBody = 0;

        ShadowFBO shadowFBO = null!;
        Quad quad = null!;

        int width; int height;
        public LightTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width; this.height = height;
            Viewport(0, 0, width, height);
            this.CenterWindow(new Vector2i(width, height));
            WindowState = WindowState.Fullscreen;
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

            camera = new(this,width, height, new Vector3(0f, 0f, -3f), 40f);
            physics = new();

            shader = new("test.vert", "test.frag");
            shadowShader = new("ShadowPass/ShadowPass.vert", "ShadowPass/ShadowPass.frag");

            UseProgram(shader.ID);
            shader.SetInt("depthMap", 0);

            Light.shader = shader;
            Light.camera = camera;

            DirectionalLight directionalLight = new(new(0.75f, 0.75f, 0.75f), new(0.45f, -0.625f, 0.75f));

            shadowFBO = new();
            quad = new();

            cube = new(World.Type.Cube);

            GameObject ground = new(cube, new Vector3(0f, -4f, 0f), Quaternion.Identity, new(30f, 1, 30f), new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static));
            GameObject wall = new(cube, new Vector3(0f, 0f, 10f), Quaternion.Identity, new(20f, 10f, 1f), new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Kinematic));
            GameObject playerObject = new(new("capsule.glb"), new(0f, 0f, 0f), Quaternion.Identity, Vector3.One, new(new(0f, 0f, 1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Kinematic));
            GameObject dummy = new(new("dummy2.glb"), new(-5f, 0f, 0f), Quaternion.Identity, new(0.5f, 0.5f, 0.5f), new(new(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Kinematic));
            GameObject bench = new(cube, new Vector3(-5f, -3f, -2f), Quaternion.Identity, Vector3.One, new(new(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Kinematic));

            player = new(dummy, new CapsuleShape(new(1.8f, 0.9f)), 60f, camera, physics);
            camera.player = player.gameObject;
            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            ClearColor(0.85f, 0.85f, 0.9f, 1.0f);
            Viewport(0, 0, shadowFBO.width, shadowFBO.height);            
            shadowFBO.Bind();
            Clear(ClearBufferMask.DepthBufferBit);
            Enable(EnableCap.CullFace);
            CullFace(TriangleFace.Front);

            UseProgram(shadowShader.ID);
            Matrix4 shadowProjection = Matrix4.CreateOrthographic(35f, 35f, 0.1f, 75.0f);
            Matrix4 shadowView = Matrix4.LookAt(35f * -Light.directionalLight.direction, Vector3.Zero, new(0f, 1f, 0f));
            Matrix4 lightProjection = shadowView * shadowProjection;
            shadowShader.SetMatrix4("lightSpaceMatrix", lightProjection);

            GameObject.Render(shadowShader);

            shadowFBO.Unbind();
            CullFace(TriangleFace.Back);
            Disable(EnableCap.CullFace);
            
            Viewport(0, 0, width, height);
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            shader.Render(camera);
            shader.SetVector3("viewPos", camera.position);
            shader.SetFloat("ambientStrength", 0.3f);
            shader.SetMatrix4("lightProjection", lightProjection);
            
            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, shadowFBO.depthMap);

            GameObject.Render(shader);
            Light.RenderLights();

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
            camera.Update(keyboardInput, mouseInput, args);
            GameObject.Update();

            if (camera.mode == Camera.Mode.LookAround)
            {
                player.Update(keyboardInput);
            }

            if (keyboardInput.IsKeyDown(Keys.D0))
            {
                GameObject cubeObj = new(cube, new(0f, 25f, 0f), Quaternion.Identity, Vector3.One, new(new(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Dynamic));
            }

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }

            if (camera.mode == Camera.Mode.Locked)
            {
                if (mouseInput.IsButtonPressed(MouseButton.Left))
                {
                    Vector3 direction;
                    camera.SendRayCastFromScreen(out direction);

                    Ray ray = new(new(camera.position.X, camera.position.Y, camera.position.Z), new System.Numerics.Vector3(direction.X, direction.Y, direction.Z) * 100f);

                    RayCastResult hit;
                    physics.System.NarrowPhaseQuery.CastRay(ray, out hit);
                    if (hit.Fraction > 0.0f)
                    {
                        selectedBody = hit.BodyID;
                    }
                }

                if (physics.BodyInterface.GetMotionType(selectedBody) == MotionType.Static)
                    return;

                System.Numerics.Vector3 force = new(0f, 0f, 0f);

                if (keyboardInput.IsKeyDown(Keys.W)) { force.Z += 1f; }
                if (keyboardInput.IsKeyDown(Keys.A)) { force.X += 1f; }
                if (keyboardInput.IsKeyDown(Keys.S)) { force.Z -= 1f; }
                if (keyboardInput.IsKeyDown(Keys.D)) { force.X -= 1f; }
                if (keyboardInput.IsKeyDown(Keys.Space)) { force.Y += 1f; }
                if (keyboardInput.IsKeyDown(Keys.LeftShift)) { force.Y -= 1f; }


                System.Numerics.Vector3 position = physics.BodyInterface.GetPosition(selectedBody) + force * 5f * Time.deltaTime;
                physics.BodyInterface.SetPosition(selectedBody, position, Activation.Activate);
            }

        }
        protected override void OnUnload()
        {
            base.OnUnload();

            physics.Dispose();
            ShaderProgram.Delete();
            GameObject.Delete();
        }
    }
}