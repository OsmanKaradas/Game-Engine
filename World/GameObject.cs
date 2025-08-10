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

        public Vector3 color;

        public static List<GameObject> gameObjects = new();

        public GameObject(Mesh mesh, Vector3 position, Vector3 color)
        {
            this.mesh = mesh;
            this.position = position;
            this.color = color;
            rotation = Vector3.Zero;
            scale = Vector3.One;

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
            int colorLocation = GetUniformLocation(shader.ID, "objectColor");
            Uniform3(colorLocation, color);

            UniformMatrix4(GetUniformLocation(shader.ID, "model"), true, ref model);

            mesh.Render();
        }

        public void Parent(GameObject parent)
        {
            rotation = parent.rotation;  
        }
    }
}