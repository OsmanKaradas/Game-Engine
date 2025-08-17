using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL4.GL;
using SharpGLTF.Schema2;

namespace GameEngine.World
{

    public class Mesh
    {
        public MeshData meshData;

        public Buffers buffers;

        Vector3 minBounds;
        Vector3 maxBounds;
        public Vector3 size;

        public Mesh(string filePath)
        {
            minBounds = new Vector3(float.MaxValue);
            maxBounds = new Vector3(float.MinValue);

            meshData = LoadGltfModel(filePath);

            buffers = new Buffers(meshData);

            size = maxBounds - minBounds;
        }

        public Mesh(Type type)
        {
            meshData = type switch
            {
                Type.Plane => Plane(),
                Type.Cube => Cube(),
                Type.Sphere => Sphere(0.7f, 32, 16),
                Type.Pyramid => Pyramid(),
                _ => throw new ArgumentException("Unknown mesh type")
            };

            buffers = new Buffers(meshData);

            Vector3 minBounds = new Vector3(float.MaxValue);
            Vector3 maxBounds = new Vector3(float.MinValue);

            foreach (var v in meshData.Vertices)
            {
                minBounds = Vector3.ComponentMin(minBounds, new Vector3(v.X, v.Y, v.Z));
                maxBounds = Vector3.ComponentMax(maxBounds, new Vector3(v.X, v.Y, v.Z));
            }

            size = maxBounds - minBounds;
        }

        public void Render()
        {
            buffers.vao.Bind();
            buffers.ibo.Bind();

            DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, meshData.Indices.Count, DrawElementsType.UnsignedInt, 0);

            buffers.vao.Unbind();
            buffers.ibo.Unbind();
        }
        public static List<Vector3> CalculateSmoothNormals(List<Vector3> vertices, List<uint> indices)
        {
            List<Vector3> normals = new List<Vector3>(new Vector3[vertices.Count]);

            // Loop over triangles
            for (int i = 0; i < indices.Count; i += 3)
            {
                int i0 = (int)indices[i];
                int i1 = (int)indices[i + 1];
                int i2 = (int)indices[i + 2];

                Vector3 v0 = vertices[i0];
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;
                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                normals[i0] += faceNormal;
                normals[i1] += faceNormal;
                normals[i2] += faceNormal;
            }

            // Normalize all vertex normals
            for (int i = 0; i < normals.Count; i++)
            {
                normals[i] = Vector3.Normalize(normals[i]);
            }

            return normals;
        }

        private MeshData Plane()
        {
            // Vertices
            List<Vector3> vertices = new List<Vector3>
            {
                new Vector3(-0.5f, 0f, 0.5f), // front left
                new Vector3(0.5f, 0f, 0.5f), // front right
                new Vector3(0.5f, 0f, -0.5f), // back right                    
                new Vector3(-0.5f, 0f, -0.5f), // back left
            };

            // Indices
            List<uint> indices = new List<uint>
            {
                0, 1, 2,
                0, 3, 2
            };

            // UV
            List<Vector2> uv = new List<Vector2>
            {
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f)
            };

            return new MeshData(vertices, indices, uv, CalculateSmoothNormals(vertices, indices));
        }

        private MeshData Cube()
        {
            // Vertices
            List<Vector3> vertices = new List<Vector3>
            {
                new Vector3(0.5f, 0.5f, -0.5f),

                new Vector3(0.5f, 0.5f, -0.5f),

                new Vector3(0.5f, 0.5f, -0.5f),

                new Vector3(0.5f, -0.5f, -0.5f),

                new Vector3(0.5f, -0.5f, -0.5f),

                new Vector3(0.5f, -0.5f, -0.5f),

                new Vector3(0.5f, 0.5f, 0.5f),

                new Vector3(0.5f, 0.5f, 0.5f),

                new Vector3(0.5f, 0.5f, 0.5f),

                new Vector3(0.5f, -0.5f, 0.5f),

                new Vector3(0.5f, -0.5f, 0.5f),

                new Vector3(0.5f, -0.5f, 0.5f),

                new Vector3(-0.5f, 0.5f, -0.5f),

                new Vector3(-0.5f, 0.5f, -0.5f),

                new Vector3(-0.5f, 0.5f, -0.5f),

                new Vector3(-0.5f, -0.5f, -0.5f),

                new Vector3(-0.5f, -0.5f, -0.5f),

                new Vector3(-0.5f, -0.5f, -0.5f),

                new Vector3(-0.5f, 0.5f, 0.5f),

                new Vector3(-0.5f, 0.5f, 0.5f),

                new Vector3(-0.5f, 0.5f, 0.5f),

                new Vector3(-0.5f, -0.5f, 0.5f),

                new Vector3(-0.5f, -0.5f, 0.5f),

                new Vector3(-0.5f, -0.5f, 0.5f)
            };

            // Indices
            List<uint> indices = new List<uint>
            {
                1, 13, 19,
                1, 19, 7,

                9, 6, 18,
                9, 18, 21,

                23, 20, 14,
                23, 14, 17,

                16, 4, 10,
                16, 10, 22,

                5, 2, 8,
                5, 8, 11,

                15, 12, 0,
                15, 0, 3
            };

            // UV
            List<Vector2> uv = new List<Vector2>
            {
                new Vector2(0.625f, 0.5f),
                new Vector2(0.625f, 0.5f),
                new Vector2(0.625f, 0.5f),
                new Vector2(0.375f, 0.5f),

                new Vector2(0.375f, 0.5f),
                new Vector2(0.375f, 0.5f),
                new Vector2(0.625f, 0.25f),
                new Vector2(0.625f, 0.25f),

                new Vector2(0.625f, 0.25f),
                new Vector2(0.375f, 0.25f),
                new Vector2(0.375f, 0.25f),
                new Vector2(0.375f, 0.25f),

                new Vector2(0.625f, 0.75f),
                new Vector2(0.875f, 0.5f),
                new Vector2(0.625f, 0.75f),
                new Vector2(0.375f, 0.75f),

                new Vector2(0.125f, 0.5f),
                new Vector2(0.375f, 0.75f),
                new Vector2(0.625f, 0f),
                new Vector2(0.875f, 0.25f),

                new Vector2(0.625f, 1f),
                new Vector2(0.375f, 0f),
                new Vector2(0.125f, 0.25f),
                new Vector2(0.375f, 1f)
            };

            return new MeshData(vertices, indices, uv, CalculateSmoothNormals(vertices, indices));
        }

        private MeshData Sphere(float radius, int segments, int rings)
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

            return new MeshData(vertices, indices, uv, CalculateSmoothNormals(vertices, indices));
        }

        private MeshData Pyramid()
        {
            // Vertices
            List<Vector3> vertices = new List<Vector3>
            {
                new Vector3(0f, 1f, 0f), // top
                new Vector3(-0.5f, 0f, -0.5f), // front left
                new Vector3(0.5f, 0f, -0.5f), // front right
                new Vector3(0.5f, 0f, 0.5f), // back right
                new Vector3(-0.5f, 0f, 0.5f) // back left
            };

            // Indices
            List<uint> indices = new List<uint>
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 4,
                0, 4, 1,
                1, 2, 3,
                3, 4, 1
            };

            // UV
            List<Vector2> uv = new List<Vector2>
            {
                new Vector2(0.5f, 1f),  // top - 0
                new Vector2(0f, 0f),    // front left - 1
                new Vector2(1f, 0f),    // front right - 2
                new Vector2(1f, 1f),    // back right - 3
                new Vector2(0f, 1f)     // back right - 4
            };

            return new MeshData(vertices, indices, uv, CalculateSmoothNormals(vertices, indices));
        }

        public MeshData LoadGltfModel(string filePath)
        {
            // Load the model
            var model = ModelRoot.Load("Models/" + filePath);

            var vertices = new List<Vector3>();
            var indices = new List<uint>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            // Usually a model has scenes -> nodes -> meshes -> primitives
            // We'll grab the first mesh of the first node for simplicity
            var mesh = model.LogicalMeshes[0];  // or loop if you want all

            foreach (var prim in mesh.Primitives)
            {
                var positionAccessor = prim.GetVertexAccessor("POSITION");
                var normalAccesor = prim.GetVertexAccessor("NORMAL");
                var uvAccesor = prim.GetVertexAccessor("TEXCOORD_0");

                // Extract vertices
                foreach (var pos in positionAccessor.AsVector3Array())
                {
                    vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                    minBounds = Vector3.ComponentMin(minBounds, new Vector3(pos.X, pos.Y, pos.Z));
                    maxBounds = Vector3.ComponentMax(maxBounds, new Vector3(pos.X, pos.Y, pos.Z));
                }

                // Extract indices
                foreach (var idx in prim.GetIndices())
                {
                    indices.Add((uint)idx);
                }

                //Extract Normals
                if (normalAccesor != null)
                {
                    foreach (var norm in normalAccesor.AsVector3Array())
                    {
                        normals.Add(new Vector3(norm.X, norm.Y, norm.Z));
                    }
                }

                // Extract UVs
                if (uvAccesor != null)
                {
                    foreach (var uv in uvAccesor.AsVector2Array())
                    {
                        uvs.Add(new Vector2(uv.X, uv.Y));
                    }
                }

                while (uvs.Count < vertices.Count) uvs.Add(Vector2.Zero);

            }

            return new MeshData(vertices, indices, uvs, normals);
        }
    }
}