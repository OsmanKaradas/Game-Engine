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
using StbImageSharp;
using JoltPhysicsSharp;


namespace GameEngine
{
    internal class SkyboxTest : GameWindow
    {
        float[] skyboxVertices =
        {
            // positions          
            -1f,  1f, -1f,
            -1f, -1f, -1f,
            1f, -1f, -1f,
            1f, -1f, -1f,
            1f,  1f, -1f,
            -1f,  1f, -1f,

            -1f, -1f,  1f,
            -1f, -1f, -1f,
            -1f,  1f, -1f,
            -1f,  1f, -1f,
            -1f,  1f,  1f,
            -1f, -1f,  1f,

            1f, -1f, -1f,
            1f, -1f,  1f,
            1f,  1f,  1f,
            1f,  1f,  1f,
            1f,  1f, -1f,
            1f, -1f, -1f,

            -1f, -1f,  1f,
            -1f,  1f,  1f,
            1f,  1f,  1f,
            1f,  1f,  1f,
            1f, -1f,  1f,
            -1f, -1f,  1f,

            -1f,  1f, -1f,
            1f,  1f, -1f,
            1f,  1f,  1f,
            1f,  1f,  1f,
            -1f,  1f,  1f,
            -1f,  1f, -1f,

            -1f, -1f, -1f,
            -1f, -1f,  1f,
            1f, -1f, -1f,
            1f, -1f, -1f,
            -1f, -1f,  1f,
            1f, -1f,  1f
        };
        int skyboxVAO, skyboxVBO;
        int cubemap;

        JoltPhysics physics = null!;
        ShaderProgram shader = null!;
        ShaderProgram shadowShader = null!;
        ShaderProgram shadowShaderCubemap = null!;
        ShaderProgram skyboxShader = null!;

        Camera camera = null!;

        Player player = null!;

        int width; int height;
        public SkyboxTest(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width; this.height = height;
            Viewport(0, 0, width, height);
            this.CenterWindow(new Vector2i(width, height));
            WindowState = WindowState.Fullscreen;
        }

        public static int LoadCubemap(string[] faces)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            for (int i = 0; i < faces.Length; i++)
            {
                using var stream = File.OpenRead(faces[i]);
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                GL.TexImage2D(
                    TextureTarget.TextureCubeMapPositiveX + i,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    image.Data
                );
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            return textureID;
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

            physics = new();
            camera = new(this, width, height, new Vector3(0f, 0f, -3f), 40f);

            shader = new("test.vert", "test.frag");
            shadowShader = new("ShadowPass/ShadowPass.vert", "ShadowPass/ShadowPass.frag");
            skyboxShader = new("Skybox/skybox.vert", "Skybox/skybox.frag");

            DirectionalLight directionalLight = new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.3f, 0.6f, -0.7f), true);
            Light.Setup(camera, shader, shadowShader, shadowShaderCubemap);

            string[] faces =
            {
                "Textures/Skybox2/right.png",
                "Textures/Skybox2/left.png",
                "Textures/Skybox2/top.png",
                "Textures/Skybox2/bottom.png",
                "Textures/Skybox2/front.png",
                "Textures/Skybox2/back.png"
            };
            cubemap = LoadCubemap(faces);

            skyboxVAO = GL.GenVertexArray();
            skyboxVBO = GL.GenBuffer();
            GL.BindVertexArray(skyboxVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            
            Mesh cubeMesh = new(World.Type.Cube);

            GameObject ground = new(cubeMesh, new Vector3(0f, -4f, 0f), Quaternion.Identity, new(100f, 1, 100f), new(new Vector3(1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static));
            GameObject wall = new(cubeMesh, new Vector3(0f, 1f, 10f), Quaternion.Identity, new(20f, 10f, 1f), new(new Vector3(1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Static));
            GameObject bench = new(cubeMesh, new Vector3(-5f, -3f, -2f), Quaternion.Identity, Vector3.One, new(new(1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Dynamic));
        
            var faceImport = SharpGLTF.Schema2.ModelRoot.Load("Models/face.glb");
            GameObject face = new(new(faceImport.LogicalMeshes[0]), new(0f, 2f, 0f), new(0f, 1f, 0f ,0f), new(1f), new(new(1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Kinematic));
            
            var dummyImport = SharpGLTF.Schema2.ModelRoot.Load("Models/mixamoAnim.glb");
            GameObject dummy = new(new(dummyImport.LogicalMeshes[0]), new(5f, 0f, 0f), Quaternion.Identity, new(0.03f), new(new(1f)), new(physics, Rigidbody.BodyType.Box, JoltPhysicsSharp.MotionType.Kinematic), new(dummyImport.LogicalSkins[0]));

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
            Light.RenderShadows();

            Enable(EnableCap.CullFace);
            Viewport(0, 0, width, height);
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            GL.DepthFunc(DepthFunction.Lequal);

            UseProgram(skyboxShader.ID);

            Matrix4 viewNoTranslate = new Matrix4(new Matrix3(camera.GetViewMatrix()));
            skyboxShader.SetMatrix4("view", viewNoTranslate);
            skyboxShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            GL.BindVertexArray(skyboxVAO);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemap);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.DepthFunc(DepthFunction.Less);

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

            if(camera.mode == Camera.Mode.LookAround)
                player.Update(keyboardInput);

            GameObject.Update();
            Animator.Update();

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
            physics.Dispose();
        }
    }
}