using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.World
{
    public class Mesh
    {
        public MeshData meshData;

        public VAO vao;
        public VBO vbo;
        public VBO vboUV;
        public IBO ibo;

        public Mesh(List<Vector3> vertices, List<Vector2> uv, List<uint> indices)
        {
            meshData = new MeshData(vertices, indices, uv);

            SetupBuffers();
        }

        public Mesh(Type type)
        {
            meshData = type switch
            {
                Type.Plane => Plane(),
                Type.Cube => Cube(),
                Type.Pyramid => Pyramid(),
                _ => throw new ArgumentException("Unknown mesh type")
            };

            SetupBuffers();
        }

        private void SetupBuffers()
        {
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

        public void Render()
        {
            vao.Bind();
            ibo.Bind();

            DrawElements(PrimitiveType.Triangles, meshData.Indices.Count, DrawElementsType.UnsignedInt, 0);

            vao.Unbind();
            ibo.Unbind();
        }

        private MeshData Plane()
        {
            MeshData meshData = new MeshData(
                //Vertices
                new List<Vector3>
                {
                    new Vector3(-0.5f, 0f, 0.5f), // front left
                    new Vector3(0.5f, 0f, 0.5f), // front right
                    new Vector3(0.5f, 0f, -0.5f), // back right
                    new Vector3(-0.5f, 0f, -0.5f), // back left 
                },

                //Indices
                new List<uint>
                {
                    0, 1, 2,
                    0, 3, 2
                },

                //UV
                new List<Vector2>
                {
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 0f)
                }
            );

            return meshData;
        }

        private MeshData Cube()
        {
            MeshData meshData = new MeshData(
                //Vertices
                new List<Vector3>
                {
                    // front face
                    //          x,     y,     z
                    new Vector3(-0.5f, 0.5f, 0.5f), // top left - 0
                    new Vector3(0.5f, 0.5f, 0.5f), // top right
                    new Vector3(0.5f, -0.5f, 0.5f), // bottom right
                    new Vector3(-0.5f, -0.5f, 0.5f), // bottom left        

                    // right face
                    new Vector3(0.5f, 0.5f, 0.5f), // top left - 4
                    new Vector3(0.5f, 0.5f, -0.5f), // top right
                    new Vector3(0.5f, -0.5f, -0.5f), // bottom right
                    new Vector3(0.5f, -0.5f, 0.5f), // bottom left

                    // back face
                    new Vector3(0.5f, 0.5f, -0.5f), // top left - 8
                    new Vector3(-0.5f, 0.5f, -0.5f), // top right
                    new Vector3(-0.5f, -0.5f, -0.5f), // bottom right
                    new Vector3(0.5f, -0.5f, -0.5f), // bottom left            

                    // left face
                    new Vector3(-0.5f, 0.5f, -0.5f), // top left - 12
                    new Vector3(-0.5f, 0.5f, 0.5f), // top right
                    new Vector3(-0.5f, -0.5f, 0.5f), // bottom right
                    new Vector3(-0.5f, -0.5f, -0.5f), // bottom left            

                    // top face
                    new Vector3(-0.5f, 0.5f, -0.5f), // top left - 16
                    new Vector3(0.5f, 0.5f, -0.5f), // top right
                    new Vector3(0.5f, 0.5f, 0.5f), // bottom right
                    new Vector3(-0.5f, 0.5f, 0.5f), // bottom left

                    // bottom face
                    new Vector3(-0.5f, -0.5f, 0.5f), // top left - 20
                    new Vector3(0.5f, -0.5f, 0.5f), // top right
                    new Vector3(0.5f, -0.5f, -0.5f), // bottom right
                    new Vector3(-0.5f, -0.5f, -0.5f) // bottom left
                },

                //Indices
                new List<uint>
                {
                    0, 1, 2,
                    2, 3, 0,

                    4, 5, 6,
                    6, 7, 4,

                    8, 9, 10,
                    10, 11, 8,

                    12, 13, 14,
                    14, 15, 12,

                    16, 17, 18,
                    18, 19, 16,

                    20, 21, 22,
                    22, 23, 20
                },

                //UV
                new List<Vector2>
                {
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 0f),

                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 0f),

                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 0f),

                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 0f),

                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 0f),

                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 0f)
                }
            );

            return meshData;
        }

        private MeshData Pyramid()
        {
            MeshData meshData = new MeshData(
                //Vertices
                new List<Vector3>
                {
                    new Vector3(0f, 0.5f, 0f), // top
                    new Vector3(-0.5f, 0f, 0.5f), // front left
                    new Vector3(0.5f, 0f, 0.5f), // front right
                    new Vector3(0.5f, 0f, -0.5f), // back right
                    new Vector3(-0.5f, 0f, -0.5f) // back left
                },

                //Indices
                new List<uint>
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 1,
                    1, 2, 3,
                    1, 3, 4
                },

                //UV
                new List<Vector2>
                {
                    new Vector2(0.5f, 1f),  // top - 0
                    new Vector2(0f, 0f),    // front left - 1
                    new Vector2(1f, 0f),    // front right - 2
                    new Vector2(1f, 1f),    // back right - 3
                    new Vector2(0f, 1f)     // back right - 4
                }
            );

            return meshData;
        }
    }
}