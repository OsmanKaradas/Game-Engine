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
        public Vector3 front = new Vector3(0f, 0f, 1f);
        public Vector3 right = new Vector3(1f, 0f, 0f);

        public Material material;
        public Rigidbody rigidbody = null!;

        public Vector3 worldMin;
        public Vector3 worldMax;
        public string tag = "";

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
            if (gameObjects.Count == 0)
                Console.WriteLine("There are no gameobjects to render!");
            foreach (GameObject obj in gameObjects)
            {
                if (obj.position.Y < -50f)
                    continue;

                shader.SetMatrix4("model", obj.GetModelMatrix());

                shader.SetVector3("inColor", obj.material.color);
                obj.material.Render(shader);
                obj.mesh.Render();
            }
        }

        public static void Update()
        {
            foreach(GameObject obj in gameObjects)
                obj.UpdateTransform();
        }

        public void UpdateTransform()
        {
            if (rigidbody == null)
                return;

            var transform = rigidbody.body.GetTransformedShape();

            position = new Vector3(transform.ShapePositionCOM.X, transform.ShapePositionCOM.Y, transform.ShapePositionCOM.Z);
            rotation = new Quaternion(transform.ShapeRotation.X, transform.ShapeRotation.Y, transform.ShapeRotation.Z, transform.ShapeRotation.W);
        }

        public static void Delete()
        {
            foreach (GameObject obj in gameObjects)
            {
                obj.mesh.buffers.vao.Delete();
                obj.mesh.buffers.vbo.Delete();
                obj.mesh.buffers.ibo.Delete();
            }
        }
    }
}