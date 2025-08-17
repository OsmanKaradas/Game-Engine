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
        public Vector3 color;
        public float ambient = 0.2f;
        public Vector3 diffuse;
        public float specular = 0.5f;
        public float shininess;
        public Texture? diffuseTex;
        public Texture? specularTex;

        public Material(Vector3 color, Texture? diffuseTex = null, Texture? specularTex = null)
        {
            this.diffuseTex = diffuseTex;
            this.specularTex = specularTex;
            diffuse = color;
        }

        public void Render(ShaderProgram shader)
        {
            shader.SetFloat("material.ambient", ambient);
            shader.SetVector3("material.diffuse", diffuse);
            shader.SetFloat("material.specular", specular);
            shader.SetFloat("material.shininess", 32f);

            if (diffuseTex != null){
                diffuseTex.Bind();
                shader.SetInt("material.diffuseTex", 0);
                shader.SetBool("useDiffuseTex", true);
            }
            else{
                shader.SetBool("useDiffuseTex", false);
            }

            if (specularTex != null){
                specularTex.Bind();
                shader.SetInt("material.specularTex", 1);
                shader.SetBool("useSpecularTex", true);
            }
            else{
                shader.SetBool("useSpecularTex", false);
            }
        }
    }
}