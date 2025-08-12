using OpenTK.Mathematics;

namespace GameEngine.World
{
    public enum Type
    {
        Plane,
        Cube,
        Sphere,
        Pyramid
    }
    public class Buffers
    {
        public VAO vao;
        public VBO vbo;
        public VBO vboUV;
        public IBO ibo;

        public Buffers(MeshData meshData)
        {
            // Setup

            vao = new VAO();

            // Vertex VBO
            vbo = new VBO(meshData.Vertices);
            vao.LinkToVAO(0, 3, vbo);

            // UV VBO
            vboUV = new VBO(meshData.UV);
            vao.LinkToVAO(1, 2, vboUV);

            ibo = new IBO(meshData.Indices);

            vao.Unbind();
        }
    }

    public class MeshData
    {
        public List<Vector3> Vertices;
        public List<Vector2> UV;
        public List<uint> Indices;

        public MeshData(List<Vector3> Vertices, List<uint> Indices, List<Vector2> UV)
        {
            this.Vertices = Vertices;
            this.Indices = Indices;
            this.UV = UV;
        }
    }
}