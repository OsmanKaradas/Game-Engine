using OpenTK.Mathematics;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

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
        public IBO ibo;

        public Buffers(MeshData meshData)
        {
            vao = new VAO();

            List<float> vertexData = new List<float>();
            for (int i = 0; i < meshData.Vertices.Count; i++)
            {
                // Position
                vertexData.Add(meshData.Vertices[i].X);
                vertexData.Add(meshData.Vertices[i].Y);
                vertexData.Add(meshData.Vertices[i].Z);

                // Normal
                vertexData.Add(meshData.Normals[i].X);
                vertexData.Add(meshData.Normals[i].Y);
                vertexData.Add(meshData.Normals[i].Z);

                // UV
                vertexData.Add(meshData.UV[i].X);
                vertexData.Add(meshData.UV[i].Y);
            }

            vbo = new VBO(vertexData);
            vao.LinkToVAO(vbo);

            ibo = new IBO(meshData.Indices);

            vao.Unbind();
        }
    }

    public class MeshData
    {
        public List<Vector3> Vertices;
        public List<uint> Indices;
        public List<Vector2> UV;
        public List<Vector3> Normals;
        public MeshData(List<Vector3> Vertices, List<uint> Indices, List<Vector2> UV, List<Vector3> Normals)
        {
            this.Vertices = Vertices;
            this.Indices = Indices;
            this.UV = UV;
            this.Normals = Normals;
        }
    }
}