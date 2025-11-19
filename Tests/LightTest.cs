using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Physics;
using GameEngine.Graphics;
using GameEngine.Animation;
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
        ShaderProgram shadowShaderCubeMap = null!;

        Mesh cubeMesh = null!;
        Camera camera = null!;

        Player player = null!;
        BodyID selectedBody = 0;
        
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
            camera = new(this, width, height, new Vector3(0f, 0f, -3f), 40f);
            physics = new();

            shader = new("test.vert", "test.frag");
            shadowShader = new("ShadowPass/ShadowPass.vert", "ShadowPass/ShadowPass.frag");
            //shadowShaderCubeMap = new("ShadowPassCubeMap/ShadowPassCubeMap.vert", "ShadowPassCubeMap/ShadowPassCubeMap.frag", "ShadowPassCubeMap/ShadowPassCubeMap.geom");

            DirectionalLight directionalLight = new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.3f, 0.6f, -0.7f), true);
            //PointLight pointlight = new(new(1f, 0f, 0f), new(0f, 5f, 0f), false);
            //SpotLight spotLight = new(new(0f, 1f, 0f), new(10f, 10f, 0f), new(0f, -1f, 0f), true);
            //SpotLight spotLight1 = new(new(0f, 1f, 1f), new(-10f, 10f, 0f), new(0f, -1f, 0f), true);
            
            Light.Setup(camera, shader, shadowShader);

            cubeMesh = new(World.Type.Cube);
            
            var dummyImport = SharpGLTF.Schema2.ModelRoot.Load("Models/mixamoAnim.glb");
            var faceImport = SharpGLTF.Schema2.ModelRoot.Load("Models/face.glb");

            GameObject ground = new(cubeMesh, new Vector3(0f, -4f, 0f), Quaternion.Identity, new(100f, 1, 100f), new(new Vector3(1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, MotionType.Static));
            GameObject wall = new(cubeMesh, new Vector3(0f, 1f, 10f), Quaternion.Identity, new(20f, 10f, 1f), new(new Vector3(1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, MotionType.Kinematic));
            GameObject bench = new(cubeMesh, new Vector3(-5f, -3f, -2f), Quaternion.Identity, Vector3.One, new(new(1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Kinematic));

            GameObject dummy = new(new(dummyImport.LogicalMeshes[0]), new(-5f, -0.6f, 0f), Quaternion.Identity, new(0.03f), new(new(1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Kinematic), new(dummyImport.LogicalSkins[0]));
            GameObject face = new(new(faceImport.LogicalMeshes[0]), new(0f, 1.5f, 2f), new(0f, 1f, 0f, 0f), new(1f), new(new(1f)), null, new(faceImport.LogicalSkins[0]));
            
            /*Animator faceAnimator = new(face.armature);
            faceAnimator.AddAnimation(faceImport.LogicalAnimations[0]);
            faceAnimator.animations["test"].loop = true;
            faceAnimator.Play(faceAnimator.animations["test"]);*/

            Animator animator = new(dummy.armature);
            animator.AddAnimation(dummyImport.LogicalAnimations[0]);
            animator.AddAnimation(dummyImport.LogicalAnimations[1]);
            animator.animations["Idle"].loop = true;
            animator.animations["Walk"].loop = true;
            animator.Play(animator.animations["Idle"]);

            player = new(animator, dummy, new CapsuleShape(new(1.675f, 1.2f)), 60f, camera, physics);
            camera.player = player.gameObject;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            //ClearColor(0.85f, 0.85f, 0.9f, 1.0f);
            Light.RenderShadows();

            Enable(EnableCap.CullFace);
            Viewport(0, 0, width, height);
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            shader.Render(camera);
            Light.RenderLights();
            GameObject.Render(shader);
            
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
            Animator.Update();

            if (camera.mode == Camera.Mode.LookAround)
            {
                player.Update(keyboardInput);
            }

            if (keyboardInput.IsKeyDown(Keys.D0))
            {
                GameObject cubeObj = new(cubeMesh, new(0f, 25f, 0f), Quaternion.Identity, Vector3.One, new(new(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Dynamic));
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
            ShaderProgram.Delete();
            GameObject.Delete();
            physics.Dispose();
        }
    }
}