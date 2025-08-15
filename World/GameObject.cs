using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using BepuPhysics;
using GameEngine.Physics;
using BepuPhysics.CollisionDetection;

namespace GameEngine.World
{
    public class GameObject
    {
        public Mesh mesh;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Vector3 color;
        public Material material;
        public JoltRigidbody joltRigidbody;
        public OBB obbBounds;
        public Vector3 worldMin;
        public Vector3 worldMax;
        public string tag = "";

        public static List<GameObject> gameObjects = new();

        public GameObject(Mesh mesh, Vector3 position, Vector3 color, bool isStatic, Vector3? scale = null)
        {
            this.mesh = mesh;
            this.position = position;
            this.color = color;
            rotation = Quaternion.Identity;
            this.scale = scale ?? Vector3.One;

            material = new Material();
            
            this.joltRigidbody = new JoltRigidbody(this, isStatic);
            gameObjects.Add(this);
        }

        public Matrix4 GetModelMatrix()
        {
            return
                Matrix4.CreateScale(scale) *
                Matrix4.CreateFromQuaternion(new Quaternion(rotation.X, rotation.Y, rotation.Z)) *
                Matrix4.CreateTranslation(position);
        }
        
        public void Render(ShaderProgram shader)
        {
            Matrix4 model = GetModelMatrix();

            material.BindMaterial();
            material.Render(shader, color);

            shader.SetBool("useTexture", material.useTexture);
            shader.SetVector3("objectColor", color);

            UniformMatrix4(GetUniformLocation(shader.ID, "model"), true, ref model);

            mesh.Render();
        }

        public void RenderUnlit(ShaderProgram shader)
        {
            Matrix4 model = GetModelMatrix();

            material.BindMaterial();

            shader.SetBool("useTexture", material.useTexture);
            shader.SetVector3("objectColor", color);

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
            obbBounds.axes = new Vector3[3];
            obbBounds.axes[0] = Vector3.Transform(Vector3.UnitX, rotation).Normalized();
            obbBounds.axes[1] = Vector3.Transform(Vector3.UnitY, rotation).Normalized();
            obbBounds.axes[2] = Vector3.Transform(Vector3.UnitZ, rotation).Normalized();

        }

        public void Update()
        {
            joltRigidbody.UpdateTransform();
        }
    }
}