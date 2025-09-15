using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Graphics;
using GameEngine.Physics;
using OpenTK.Mathematics;
using ImGuiNET;
using JoltPhysicsSharp;

namespace GameEngine
{
    internal class ShadowTest : GameWindow
    {
        JoltPhysics physics = null!;
        GeometryPass geometryPass = null!;
        ShadowPass shadowPass = null!;
        LightingPass lightingPass = null!;

        ImGuiController guiController = null!;
        Camera camera = null!;
        Mesh cube = null!;

        BodyID selectedBody = 0;

        GameObject lightObj = null!;

        public ShadowTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.CenterWindow(new Vector2i(width, height));
            this.WindowState = WindowState.Fullscreen;
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            Viewport(0, 0, e.Width, e.Height);

            if (camera != null)
            { camera.screenWidth = e.Width; camera.screenHeight = e.Height; }

            if (guiController != null)
                guiController.WindowResized(e.Width, e.Height);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            camera = new(ClientSize.X, ClientSize.Y, new Vector3(0f, 0f, -3f), 40f);
            physics = new JoltPhysics();

            geometryPass = new("GeometryPass/GeometryPass.vert", "GeometryPass/GeometryPass.frag", ClientSize.X, ClientSize.Y);
            shadowPass = new("ShadowPass/ShadowPass.vert", "ShadowPass/ShadowPass.frag", "ShadowPass/ShadowPass.geom");
            lightingPass = new("LightingPass/LightingPass.vert", "LightingPass/LightingPass.frag", new());

            Light.shader = lightingPass.shader;
            Light.camera = camera;
            DirectionalLight directionalLight = new(new(0.75f, 0.75f, 0.75f), new Vector3(-0.45f, -0.625f, -0.75f));
            //PointLight pointLight = new(new(1f, 0.5f, 0.2f), new(0f, 5f, 2f));
            SpotLight spotLight = new(new(1f, 1f, 1f), new(0f, 5f, 0f), new(0f, -1f, 0f));

            guiController = new(ClientSize.X, ClientSize.Y);

            cube = new(World.Type.Cube);
            Mesh sphere = new(World.Type.Sphere);
            Mesh dummyMesh = new("dummy2.glb");
            Mesh spiderMesh = new("spider.glb");
            Mesh creatureMesh = new("creature.glb");
        
            GameObject ground = new(cube, new Vector3(0f, -3.4f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(20f, 1f, 20f));
            GameObject wall = new(cube, new Vector3(0f, 0f, -4f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Kinematic), new Vector3(20f, 10f, 1f));
            GameObject cubeObj = new(cube, new Vector3(0f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Dynamic));
            GameObject dummy = new(dummyMesh, new Vector3(5f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Kinematic), new(0.5f, 0.5f, 0.5f));
            GameObject spider = new(spiderMesh, new Vector3(-5f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Kinematic), new(0.85f, 0.85f, 0.85f));
            GameObject creature = new(creatureMesh, new Vector3(-5f, 0f, 10f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f), null, new("Skin/Skin(Diffuse).png", TextureUnit.Texture1)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Kinematic), new(1.5f, 1.5f, 1.5f));
            lightObj = new(sphere, new(0f, 5f, 2f), Quaternion.Identity, new(new(0.5f, 0f, 0f)), new(physics, Rigidbody.BodyType.Sphere, MotionType.Kinematic), new(0.25f, 0.25f, 0.25f));
            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            //shadowPass.RenderPointLightShadows(Light.spotLights[0].position);
            shadowPass.RenderDirLightShadows();

            geometryPass.Render(camera);

            lightingPass.Render(geometryPass, shadowPass);

            // --- GUI ---
            guiController.Update(this, (float)args.Time);

            ImGui.SetNextWindowBgAlpha(0.75f);
            ImGui.Begin("FPS Overlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.Text($"FPS: {Time.fps}");
            ImGui.SliderFloat("lightDirX", ref Light.directionalLight.direction.X, -0.75f, 0.75f);
            ImGui.SliderFloat("lightDirY", ref Light.directionalLight.direction.Y, -1f, 0f);
            ImGui.SliderFloat("lightDirZ", ref Light.directionalLight.direction.Z, -1f, 0f);
            /*
            ImGui.SliderFloat("pointLightDirX", ref Light.spotLights[0].position.X, -10f, 10f);
            ImGui.SliderFloat("pointLightDirY", ref Light.spotLights[0].position.Y, -10f, 10f);
            ImGui.SliderFloat("pointLightDirZ", ref Light.spotLights[0].position.Z, -10f, 10f);
            */
            ImGui.End();
            
            guiController.Render();

            Light.directionalLight.direction.Normalize();
            Light.spotLights[0].position = lightObj.position;

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
            camera.Update(this, keyboardInput, mouseInput, args);
            GameObject.Update();

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }

            if (keyboardInput.IsKeyDown(Keys.D0))
            {
                GameObject cubeObj = new(cube, new Vector3(0f, 50f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Dynamic));
            }

            if (camera.mode == Camera.Mode.Locked)
            {
                if (mouseInput.IsButtonPressed(MouseButton.Left))
                {
                    Vector3 direction;
                    camera.SendRayCastFromScreen(this, out direction);

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

                if (keyboardInput.IsKeyDown(Keys.W)) { force.Z -= 1f; }
                if (keyboardInput.IsKeyDown(Keys.A)) { force.X -= 1f; }
                if (keyboardInput.IsKeyDown(Keys.S)) { force.Z += 1f; }
                if (keyboardInput.IsKeyDown(Keys.D)) { force.X += 1f; }
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
        }
    }
}