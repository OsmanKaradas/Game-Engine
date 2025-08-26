using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GameEngine.Physics;
using GameEngine.Graphics;

namespace GameEngine.World
{
    public class GameObject
    {
        public Mesh mesh;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Material material;
        public Texture diffuseTex = null!;
        public Texture specularTex = null!;
        public Rigidbody rigidbody = null!;
        public OBB obbBounds;
        public Vector3 worldMin;
        public Vector3 worldMax;
        public string tag = "";

        public Vector3 front = -Vector3.UnitZ;
        public Vector3 right = Vector3.UnitX;
        public Vector3 up = Vector3.UnitY;
        public static List<GameObject> gameObjects = new();

        public GameObject(Mesh mesh, Vector3 position, Quaternion rotation, Material material, Rigidbody? rigidbody = null, Vector3? scale = null)
        {
            this.mesh = mesh;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale ?? Vector3.One;
            this.material = material;

            if (rigidbody != null)
            {
                this.rigidbody = rigidbody;
                Vector3 colliderSize = this.scale * mesh.size;
                rigidbody.Initialize(position, rotation, colliderSize);
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
                    continue;

                Matrix4 model = obj.GetModelMatrix();
                UniformMatrix4(GetUniformLocation(shader.ID, "model"), false, ref model);
                shader.SetVector3("inColor", obj.material.color);
                obj.material.Render(shader);

                obj.UpdateTransform();

                obj.mesh.buffers.vao.Bind();
                obj.mesh.buffers.ibo.Bind();
                
                DrawElements(PrimitiveType.Triangles, obj.mesh.meshData.Indices.Count, DrawElementsType.UnsignedInt, 0);
                obj.mesh.buffers.vao.Unbind();
            }
        }

        public void UpdateBounds()
        {

            worldMin = position - obbBounds.halfSize;
            worldMax = position + obbBounds.halfSize;

            obbBounds.center = position;
            obbBounds.halfSize = scale * 0.5f;

            obbBounds.axes = new Vector3[3];
            obbBounds.axes[0] = Vector3.Transform(Vector3.UnitX, rotation).Normalized();
            obbBounds.axes[1] = Vector3.Transform(Vector3.UnitY, rotation).Normalized();
            obbBounds.axes[2] = Vector3.Transform(Vector3.UnitZ, rotation).Normalized();

        }

        public void UpdateTransform()
        {
            if (rigidbody != null)
            {
                rigidbody.UpdateTransform();
                position = new Vector3(rigidbody.position.X, rigidbody.position.Y, rigidbody.position.Z);
                rotation = new Quaternion(rigidbody.rotation.X, rigidbody.rotation.Y, rigidbody.rotation.Z, rigidbody.rotation.W);
            }
        }
    }
}