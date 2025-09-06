using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;
using GameEngine.Physics;
using GameEngine.Graphics;
using OpenTK.Mathematics;
using ImGuiNET;
using JoltPhysicsSharp;

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
        private ImGuiController guiController = null!;
        JoltPhysics physics = null!;
        Quad quad = null!;
        GeometryPass geometryPass = null!;
        LightingPass lightingPass = null!;
        ShadowPass shadowPass = null!;

        ShaderProgram lightShader = null!;

        VAO lightVAO = null!;
        VBO lightVBO = null!;
        IBO lightIBO = null!;

        PointLight pointLight = null!;
        Vector3 lightDir = new Vector3(-0.1f, -1f, -1f).Normalized();
        Mesh cubeMesh = null!;
        Mesh sphere = null!;
        Player player = null!;
        Camera camera = null!;

        float fps;

        int width, height;
        public Test(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width; this.height = height;
            this.CenterWindow(new Vector2i(width, height));
            this.WindowState = WindowState.Fullscreen;
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            width = e.Width; height = e.Height;
            Viewport(0, 0, width, height);

            if (camera != null)
            { camera.screenWidth = width; camera.screenHeight = height; }

            if(guiController != null)
                guiController.WindowResized(width, height);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            guiController = new ImGuiController(width, height);
            camera = new Camera(width, height, new Vector3(0f, 0f, -3f), 10f);
            physics = new JoltPhysics();

            quad = new Quad();

            geometryPass = new GeometryPass("GeometryPass.vert", "GeometryPass.frag", width, height);
            lightingPass = new LightingPass("LightingPass.vert", "LightingPass.frag", quad);
            shadowPass = new ShadowPass("ShadowPass.vert", "ShadowPass.frag", "ShadowPass.geom");
            lightShader = new ShaderProgram("light.vert", "light.frag");

            UseProgram(lightShader.ID);
            lightVAO = new VAO();
            lightVBO = new VBO(lightVertices);
            lightIBO = new IBO(lightIndices);
            lightVAO.Bind();
            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            EnableVertexAttribArray(0);

            lightVAO.Unbind();
            UseProgram(0);

            Light.camera = camera;
            Light.shader = lightingPass.shader;

            DirectionalLight directionalLight = new DirectionalLight(new Vector3(0.075f, 0.085f, 0.085f), lightDir);
            pointLight = new PointLight(new Vector3(0.675f, 0.875f, 1f), new Vector3(0f, 5f, 0f));
            
            cubeMesh = new Mesh(World.Type.Cube);
            sphere = new Mesh(World.Type.Sphere);
            GameObject cube = new(cubeMesh, new(-5f, 0f, 0f), Quaternion.Identity, new(new(1f, 1f, 1f)), new(physics, Rigidbody.BodyType.Box, MotionType.Dynamic));
            GameObject ground = new GameObject(cubeMesh, new Vector3(0f, -5f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(20f, 1f, 20f));
            GameObject top = new GameObject(cubeMesh, new Vector3(0f, 25f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(20f, 1f, 20f));
            
            GameObject wallF1 = new GameObject(cubeMesh, new Vector3(-8.5f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(3f, 30f, 1f));
            GameObject wallF2 = new GameObject(cubeMesh, new Vector3(-4.5f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(2f, 30f, 1f));
            GameObject wallF3 = new GameObject(cubeMesh, new Vector3(0f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(2f, 30f, 1f));
            GameObject wallF4 = new GameObject(cubeMesh, new Vector3(4.5f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(2f, 30f, 1f));
            GameObject wallF5 = new GameObject(cubeMesh, new Vector3(8.5f, 10.5f, 9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(3f, 30f, 1f));

            GameObject wallB = new GameObject(cubeMesh, new Vector3(0f, 10.5f, -9.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(20f, 30f, 1f));
            GameObject wallL = new GameObject(cubeMesh, new Vector3(-9.5f, 10.5f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(1f, 30f, 20f));
            GameObject wallR = new GameObject(cubeMesh, new Vector3(9.5f, 10.5f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(1f, 30f, 20f));
            
            GameObject interior1 = new GameObject(cubeMesh, new Vector3(-7.5f, -3.5f, -8.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(3f, 3f, 2f));
            GameObject interior2 = new GameObject(cubeMesh, new Vector3(0f, -3.5f, -7.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(5f, 5f, 3f));
            GameObject interior3 = new GameObject(cubeMesh, new Vector3(7.5f, -3.5f, -8.5f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static), new Vector3(3f, 3f, 2f));
            
            Mesh dummyMesh = new Mesh("test_dummy.glb");
            GameObject dummy = new GameObject(dummyMesh, new Vector3(5f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Dynamic), new Vector3(0.5f, 0.5f, 0.5f));

            Mesh capsule = new Mesh("capsule.glb");
            GameObject playerObject = new GameObject(capsule, new Vector3(0f, 0f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)));
            player = new(playerObject, new CapsuleShape(new(1.8f, 0.9f)), 60f, camera, physics);

            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            shadowPass.RenderDirLightShadows();

            geometryPass.Render(camera);

            lightingPass.Render(geometryPass, shadowPass);

            // --- LIGHT OBJECT RENDER ---
            lightShader.Render(camera);
            lightVAO.Bind();

            Matrix4 lightModel = Matrix4.Identity * Matrix4.CreateTranslation(pointLight.position);
            lightShader.SetMatrix4("model", lightModel);
            lightShader.SetVector3("inColor", pointLight.color);
            DrawElements(PrimitiveType.Triangles, lightIndices.Count, DrawElementsType.UnsignedInt, 0);

            // --- GUI ---
            guiController.Update(this, (float)args.Time);

            ImGui.SetNextWindowBgAlpha(0.5f);

            ImGui.Begin("FPS Overlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove);
            ImGui.Text($"FPS: {Time.fps}");
            ImGui.End();

            ImGui.SetNextWindowPos(new(0, 500));
            ImGui.Begin("Light Controls", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove);
            ImGui.SetCursorPosX(90f);
            ImGui.Text("Light Controls");
            ImGui.Text("Direction");
            
            ImGui.SliderFloat("lightDirX", ref Light.directionalLight.direction.X, -1f, 1f);
            ImGui.SliderFloat("lightDirY", ref Light.directionalLight.direction.Y, -1f, 1f);
            ImGui.SliderFloat("lightDirZ", ref Light.directionalLight.direction.Z, -1f, 1f);

            ImGui.Text("Color");
            ImGui.SliderFloat("lightColorR", ref pointLight.color.X, 0f, 1f);
            ImGui.SliderFloat("lightColorG", ref pointLight.color.Y, 0f, 1f);
            ImGui.SliderFloat("lightColorB", ref pointLight.color.Z, 0f, 1f);
            ImGui.End();

            guiController.Render();

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
            GameObject.Update();
            camera.Update(this, keyboardInput, mouseInput, args);
            if (camera.mode == Camera.Mode.LookAround)
            {
                player.Update(keyboardInput);
            }

            Console.WriteLine(Time.fps);

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }

            if (keyboardInput.IsKeyDown(Keys.D0))
            {
                GameObject cube = new GameObject(cubeMesh, new Vector3(0f, 5f, 0f), Quaternion.Identity, new Material(new Vector3(1f, 1f, 1f)), new Rigidbody(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Dynamic));
            }

            float moveSpeed = 8f * Time.deltaTime;
            if (keyboardInput.IsKeyDown(Keys.Up)) { pointLight.position.Z -= moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Down)) { pointLight.position.Z += moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Right)) { pointLight.position.X += moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Left)) { pointLight.position.X -= moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.V)) { pointLight.position.Y += moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.B)) { pointLight.position.Y -= moveSpeed; }
            moveSpeed = 0f;
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            
            lightVAO.Delete();
            lightVBO.Delete();
            lightIBO.Delete();
            physics.Dispose();
            quad.Delete();
            ShaderProgram.Delete();
            GameObject.Delete();
            guiController.Dispose();
        }
    }
}