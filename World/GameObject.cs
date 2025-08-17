using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.Physics;

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
        public Texture diffuseTex = null!;
        public Texture specularTex = null!;
        public Rigidbody? rigidbody;
        public OBB obbBounds;
        public Vector3 worldMin;
        public Vector3 worldMax;
        public string tag = "";

        public Vector3 front = -Vector3.UnitZ;
        public Vector3 right = Vector3.UnitX;
        public Vector3 up = Vector3.UnitY;
        public static List<GameObject> gameObjects = new();

        public GameObject(Mesh mesh, Vector3 position, Material material, Rigidbody? rigidbody = null, Vector3? scale = null)
        {
            this.mesh = mesh;
            this.position = position;
            rotation = Quaternion.Identity;
            this.scale = scale ?? Vector3.One;
            this.material = material;
            this.rigidbody = rigidbody;
            if (rigidbody != null)
            {
                Vector3 colliderSize = this.scale * mesh.size;
                rigidbody.Setup(position, rotation, colliderSize);
            }

            gameObjects.Add(this);
        }

        public Matrix4 GetModelMatrix()
        {
            return
                Matrix4.CreateScale(scale) *
                Matrix4.CreateFromQuaternion(new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W)) *
                Matrix4.CreateTranslation(position);
        }
        
        public static void Render(ShaderProgram shader)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (obj.position.Y < -50f)
                    return;

                Matrix4 model = obj.GetModelMatrix();

                shader.SetVector3("objectColor", obj.color);
                obj.material.Render(shader);
                UniformMatrix4(GetUniformLocation(shader.ID, "model"), true, ref model);

                obj.Update();

                obj.mesh.Render();            
            }
        }

        public static void RenderUnlit(ShaderProgram shader)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (obj.position.Y < -50f)
                    return;

                Matrix4 model = obj.GetModelMatrix();

                obj.material.Render(shader);
                shader.SetVector3("objectColor", obj.color);
                UniformMatrix4(GetUniformLocation(shader.ID, "model"), true, ref model);

                obj.Update();

                obj.mesh.Render();            
            }
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
            if (rigidbody != null)
            {
                rigidbody.Update();
                position = new Vector3(rigidbody.position.X, rigidbody.position.Y, rigidbody.position.Z);
                rotation = new Quaternion(rigidbody.rotation.X, rigidbody.rotation.Y, rigidbody.rotation.Z, rigidbody.rotation.W);
            }
        }
    }
}