using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.World
{
    public class Mesh
    {
        public MeshData meshData;

        public Buffers buffers;

        public Mesh(List<Vector3> vertices, List<Vector2> uv, List<uint> indices)
        {
            meshData = new MeshData(vertices, indices, uv);

            buffers = new Buffers(meshData);
        }

        public Mesh(Type type)
        {
            meshData = type switch
            {
                Type.Plane => Plane(),
                Type.Cube => Cube(),
                Type.Sphere => Sphere(1f, 32, 16),
                Type.Pyramid => Pyramid(),
                _ => throw new ArgumentException("Unknown mesh type")
            };

            buffers = new Buffers(meshData);
        }

        public void Render()
        {
            buffers.vao.Bind();
            buffers.ibo.Bind();

            DrawElements(PrimitiveType.Triangles, meshData.Indices.Count, DrawElementsType.UnsignedInt, 0);

            buffers.vao.Unbind();
            buffers.ibo.Unbind();
        }

        private MeshData Plane()
        {
            MeshData meshData = new MeshData(
                // Vertices
                new List<Vector3>
                {
                    new Vector3(-0.5f, 0f, 0.5f), // front left
                    new Vector3(0.5f, 0f, 0.5f), // front right
                    new Vector3(0.5f, 0f, -0.5f), // back right
                    new Vector3(-0.5f, 0f, -0.5f), // back left 
                },

                // Indices
                new List<uint>
                {
                    0, 1, 2,
                    0, 3, 2
                },

                // UV
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
                // Vertices
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

                // Indices
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

                // UV
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

        public static MeshData Sphere(float radius, int segments, int rings)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var indices = new List<uint>();

            for (int y = 0; y <= rings; y++)
            {
                float v = (float)y / rings;
                float theta1 = v * MathF.PI;

                for (int x = 0; x <= segments; x++)
                {
                    float u = (float)x / segments;
                    float theta2 = u * MathF.PI * 2f;

                    float xPos = radius * MathF.Sin(theta1) * MathF.Cos(theta2);
                    float yPos = radius * MathF.Cos(theta1);
                    float zPos = radius * MathF.Sin(theta1) * MathF.Sin(theta2);

                    vertices.Add(new Vector3(xPos, yPos, zPos));
                    uv.Add(new Vector2(u, v));
                }
            }

            for (int y = 0; y < rings; y++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int first = y * (segments + 1) + x;
                    int second = first + segments + 1;

                    indices.Add((uint)first);
                    indices.Add((uint)second);
                    indices.Add((uint)(first + 1));

                    indices.Add((uint)(first + 1));
                    indices.Add((uint)second);
                    indices.Add((uint)(second + 1));
                }
            }

            return new MeshData(vertices, indices, uv);
        }

        private MeshData Pyramid()
        {
            MeshData meshData = new MeshData(
                // Vertices
                new List<Vector3>
                {
                    new Vector3(0f, 0.5f, 0f), // top
                    new Vector3(-0.5f, 0f, 0.5f), // front left
                    new Vector3(0.5f, 0f, 0.5f), // front right
                    new Vector3(0.5f, 0f, -0.5f), // back right
                    new Vector3(-0.5f, 0f, -0.5f) // back left
                },

                // Indices
                new List<uint>
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 1,
                    1, 2, 3,
                    1, 3, 4
                },

                // UV
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