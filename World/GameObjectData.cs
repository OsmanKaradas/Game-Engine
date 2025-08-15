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
            // Setup

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

    public class Material
    {
        public bool useTexture = false;

        public Texture? ambient;
        public Texture? diffuse;
        public Texture? specular;
        public Texture? normal;

        public Material(Texture? diffuse = null, Texture? normal = null, Texture? specular = null)
        {
            this.diffuse = diffuse;
            this.normal = normal;
            this.specular = specular;
        }

        public void Render(ShaderProgram shader)
        {
            shader.SetVector3("material.diffuse", new Vector3(0.75f, 0.75f, 0.75f));
            shader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            shader.SetFloat("material.shininess", 32f);

            if (diffuse != null){
                diffuse.Bind();
                shader.SetInt("material.diffuseTex", 0);
                shader.SetBool("useDiffuseTex", true);
            }
            else{
                shader.SetBool("useDiffuseTex", false);
            }

            if (specular != null){
                specular.Bind();
                shader.SetInt("material.specularTex", 1);
                shader.SetBool("useSpecularTex", true);
            }
            else{
                shader.SetBool("useSpecularTex", false);
            }
        }
    }
}