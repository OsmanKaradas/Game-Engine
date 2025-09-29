using OpenTK.Mathematics;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.Graphics;

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
        public VBO armatureVBO = null!;
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

                if (meshData.Weights.Count > 0)
                {
                    // Weights
                    vertexData.Add(meshData.Weights[i].X);
                    vertexData.Add(meshData.Weights[i].Y);
                    vertexData.Add(meshData.Weights[i].Z);
                    vertexData.Add(meshData.Weights[i].W);
                }
            }

            vbo = new VBO(vertexData);

            int stride = 8 * sizeof(float);
            
            if (meshData.Weights.Count > 0)
            {
                stride = 12 * sizeof(float);
            }

            // POSITION
            EnableVertexAttribArray(0);
            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // NORMAL
            EnableVertexAttribArray(1);
            VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            // UV
            EnableVertexAttribArray(2);
            VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));


            if(meshData.Weights.Count > 0)
            {
                // WEIGHTS
                EnableVertexAttribArray(4);
                VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, stride, 8 * sizeof(float));                    
            }
            
            if (meshData.BoneIDs.Count > 0)
            {
                // BONES
                List<int> intData = new();
                for (int i = 0; i < meshData.BoneIDs.Count; i++)
                {
                    intData.Add(meshData.BoneIDs[i].X);
                    intData.Add(meshData.BoneIDs[i].Y);
                    intData.Add(meshData.BoneIDs[i].Z);
                    intData.Add(meshData.BoneIDs[i].W);
                }

                armatureVBO = new(intData);

                EnableVertexAttribArray(3);
                VertexAttribIPointer(3, 4, VertexAttribIntegerType.Int, 4 * sizeof(int), IntPtr.Zero);
            }
            
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
        public List<Vector4i> BoneIDs;
        public List<Vector4> Weights;
        public MeshData(List<Vector3> Vertices, List<uint> Indices, List<Vector2> UV, List<Vector3> Normals, List<Vector4i>? BoneIDs = null, List<Vector4>? Weights = null)
        {
            this.Vertices = Vertices;
            this.Indices = Indices;
            this.UV = UV;
            this.Normals = Normals;
            this.BoneIDs = BoneIDs ?? new List<Vector4i>();
            this.Weights = Weights ?? new List<Vector4>();
        }
    }
}