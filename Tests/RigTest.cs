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
        GameObject dummy = null!;
        GameObject bowl = null!;
        GameObject rigTest = null!;

        AnimationClip animation = null!;

        int idxCount = 0;
        VAO vao = null!;
        VBO vbo = null!;
        IBO ibo = null!;

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

            Mesh dummyMesh = new("dummyRig.glb");
            Mesh bowlMesh = new("bowl.glb");
            Mesh rigTestMesh = new("rigTest.glb");

            Mesh cubeMesh = new(World.Type.Cube);

            MeshData sphere = Sphere(0.07f, 32, 16);

            dummy = new(dummyMesh, new(0f, 0f, 0f), Quaternion.Identity, Vector3.One, new(new(1f, 0f, 0f)), null, new("dummyRig.glb"));
            //bowl = new(bowlMesh, new(-12f, 0f, 0f), Quaternion.Identity, Vector3.One, new(new(1f, 0f, 0f)), null, new("bowl.glb"));
            rigTest = new(rigTestMesh, new(14f, 0f, 0f), Quaternion.Identity, Vector3.One, new(new(1f, 0f, 0f)), null, new("rigTest.glb"));

            foreach (var bone in rigTest.armature.bones)
            {
                Console.WriteLine("INVERSE: " + bone.Value.name + ": " + bone.Value.offset);
                Console.WriteLine("LOCAL: " + bone.Value.name + ": " + bone.Value.GetLocalMatrix());
                Console.WriteLine("FINAL: " + bone.Value.name + ": " + bone.Value.finalMatrix);

            }

            foreach (var bone in rigTest.armature.bones)
            {
            }
            GameObject cube = new(cubeMesh, new(-10f, 0f, 0f), Quaternion.Identity, Vector3.One, new(new(1f, 0f, 0f)));

            vao = new();
            vbo = new(sphere.Vertices.SelectMany(v => new float[]{ v.X, v.Y, v.Z }).ToList());

            EnableVertexAttribArray(0);
            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            ibo = new(sphere.Indices);
            idxCount = sphere.Indices.Count;

            vbo.Unbind();
            vao.Unbind();
            
            animation = new(2f, new KeyFrame[5]);
            Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Viewport(0, 0, width, height);
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            shader.Render(camera);
            shader.SetVector3("viewPos", camera.position);
            GameObject.Render(shader);

            UseProgram(debugShader.ID);
            debugShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            debugShader.SetMatrix4("view", camera.GetViewMatrix());
            debugShader.SetVector3("inColor", new(0f, 0f, 1f));

            vao.Bind();

            foreach (var bone in dummy.armature.bones)
            {
                Vector3 position = bone.Value.finalMatrix.ExtractTranslation();
                Matrix4 model = Matrix4.CreateTranslation(position);
                debugShader.SetMatrix4("model", model);
                DrawElements(PrimitiveType.Triangles, idxCount, DrawElementsType.UnsignedInt, 0);
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
            animation.UpdateAnimation();

            if (keyboardInput.IsKeyPressed(Keys.Escape))
            {
                Close();
            }

            float angle = 1.2f * Time.deltaTime;
            string boneName = "High";
            
            if (keyboardInput.IsKeyDown(Keys.G))
            {
                var x = Quaternion.FromAxisAngle(Vector3.UnitX, angle);
                rigTest.armature.bones[boneName].rotation = Quaternion.Normalize(x * rigTest.armature.bones[boneName].rotation);

            }
            if (keyboardInput.IsKeyDown(Keys.H))
            {
                var mX = Quaternion.FromAxisAngle(-Vector3.UnitX, angle);
                rigTest.armature.bones[boneName].rotation = Quaternion.Normalize(mX * rigTest.armature.bones[boneName].rotation);
            }

            if (keyboardInput.IsKeyDown(Keys.J))
            {
                var z = Quaternion.FromAxisAngle(Vector3.UnitZ, angle);
                rigTest.armature.bones[boneName].rotation = Quaternion.Normalize(z * rigTest.armature.bones[boneName].rotation);
            }
            if (keyboardInput.IsKeyDown(Keys.K))
            {
                var mZ = Quaternion.FromAxisAngle(-Vector3.UnitZ, angle);
                rigTest.armature.bones[boneName].rotation = Quaternion.Normalize(mZ * rigTest.armature.bones[boneName].rotation);
            }
        }
        protected override void OnUnload()
        {
            base.OnUnload();

            ShaderProgram.Delete();
            GameObject.Delete();
        }

        private MeshData Sphere(float radius, int segments, int rings)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uv = new List<Vector2>();
            var indices = new List<uint>();

            // Generate vertices
            for (int y = 0; y <= rings; y++)
            {
                float v = (float)y / rings;
                float theta1 = v * MathF.PI;

                for (int x = 0; x <= segments; x++)
                {
                    float u = (float)x / segments;
                    float theta2 = u * MathF.PI * 2f;

                    float xPos = radius * MathF.Sin(theta1) * MathF.Cos(theta2);
                    float yPos = radius * MathF.Cos(theta1);
                    float zPos = radius * MathF.Sin(theta1) * MathF.Sin(theta2);

                    var pos = new Vector3(xPos, yPos, zPos);
                    vertices.Add(pos);

                    // Normal = position normalized
                    normals.Add(Vector3.Normalize(pos));

                    // UVs
                    uv.Add(new Vector2(u, v));
                }
            }

            // Generate indices
            for (int y = 0; y < rings; y++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int first = y * (segments + 1) + x;
                    int second = first + segments + 1;

                    indices.Add((uint)first);
                    indices.Add((uint)second);
                    indices.Add((uint)(first + 1));

                    indices.Add((uint)(first + 1));
                    indices.Add((uint)second);
                    indices.Add((uint)(second + 1));
                }
            }

            return new MeshData(vertices, indices, uv, normals);
        }
    }
}