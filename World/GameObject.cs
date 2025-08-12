using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.World
{
    public class GameObject
    {
        public Mesh mesh;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public Vector4 color;
        private Texture? texture;
        public Rigidbody rigidbody;
        public OBB obbBounds;
        public Vector3 worldMin;
        public Vector3 worldMax;
        public string tag = "";

        public static List<GameObject> gameObjects = new();

        public GameObject(Mesh mesh, Vector3 position, Vector4 color, bool isStatic, string texturePath = null)
        {
            this.mesh = mesh;
            this.position = position;
            this.color = color;
            rotation = Vector3.Zero;
            scale = Vector3.One;

            if (!string.IsNullOrEmpty(texturePath))
            {
                try
                {
                    this.texture = new Texture(texturePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to load texture: " + e.Message);
                }
            }

            rigidbody = new Rigidbody(this, isStatic);

            obbBounds = new OBB(position, scale / 2);
            UpdateBounds();

            gameObjects.Add(this);
        }

        public Matrix4 GetModelMatrix()
        {
            return
                Matrix4.CreateScale(scale) *
                Matrix4.CreateRotationX(rotation.X) *
                Matrix4.CreateRotationY(rotation.Y) *
                Matrix4.CreateRotationZ(rotation.Z) *
                Matrix4.CreateTranslation(position);
        }

        public void Render(ShaderProgram shader)
        {
            Matrix4 model = GetModelMatrix();

            /*shader.SetBool("useTexture", texture != null);
            shader.SetVector4("objectColor", color);*/

            int colorLocation = GetUniformLocation(shader.ID, "objectColor");
            Uniform4(colorLocation, ref color);

            /*if (texture != null)
            {
                texture.Bind();
            }*/

            UniformMatrix4(GetUniformLocation(shader.ID, "model"), true, ref model);

            mesh.Render();
        }

        public void UpdateBounds()
        {
            worldMin = position - obbBounds.halfSize;
            worldMax = position + obbBounds.halfSize;

            // update OBB for SAT:
            obbBounds.center = position;
            obbBounds.halfSize = scale * 0.5f;

            // compute axes from rotation quaternion
            var q = Quaternion.FromEulerAngles(rotation);
            obbBounds.axes = new Vector3[3];
            obbBounds.axes[0] = Vector3.Transform(Vector3.UnitX, q).Normalized();
            obbBounds.axes[1] = Vector3.Transform(Vector3.UnitY, q).Normalized();
            obbBounds.axes[2] = Vector3.Transform(Vector3.UnitZ, q).Normalized();
        }

        /*public void UpdateBounds()
        {
            // Local-space min/max of your cube mesh
            Vector3 localMin = new Vector3(-0.5f, -0.5f, -0.5f);
            Vector3 localMax = new Vector3(0.5f, 0.5f, 0.5f);

            // Apply scaling
            Vector3 scaledMin = localMin * scale;
            Vector3 scaledMax = localMax * scale;

            // If you have rotation, you need to transform the 8 corners
            // to find the new world-space min/max
            Vector3[] corners = new Vector3[8]
            {
                new Vector3(scaledMin.X, scaledMin.Y, scaledMin.Z),
                new Vector3(scaledMax.X, scaledMin.Y, scaledMin.Z),
                new Vector3(scaledMin.X, scaledMax.Y, scaledMin.Z),
                new Vector3(scaledMax.X, scaledMax.Y, scaledMin.Z),
                new Vector3(scaledMin.X, scaledMin.Y, scaledMax.Z),
                new Vector3(scaledMax.X, scaledMin.Y, scaledMax.Z),
                new Vector3(scaledMin.X, scaledMax.Y, scaledMax.Z),
                new Vector3(scaledMax.X, scaledMax.Y, scaledMax.Z)
            };

            // Apply rotation & translation
            Matrix4 transform = Matrix4.CreateRotationX(rotation.X) *
                                 Matrix4.CreateRotationY(rotation.Y) *
                                 Matrix4.CreateRotationZ(rotation.Z) *
                                 Matrix4.CreateTranslation(position);

            Vector3 worldMin = new Vector3(float.MaxValue);
            Vector3 worldMax = new Vector3(float.MinValue);

            foreach (var c in corners)
            {
                Vector3 transformed = Vector3.TransformPosition(c, transform);
                worldMin = Vector3.ComponentMin(worldMin, transformed);
                worldMax = Vector3.ComponentMax(worldMax, transformed);
            }

            // Update the bounding box
            bounds.Min = worldMin;
            bounds.Max = worldMax;
        }*/

    }
}