using OpenTK.Mathematics;
using GameEngine.Graphics;

namespace GameEngine.World
{
    public class Material
    {
        public Vector3 color;
        public float ambient = 1f;
        public float diffuse = 1f;
        public float specular = 1f;
        public float shininess;

        public Texture ambientTex = null!;
        public Texture diffuseTex = null!;
        public Texture specularTex = null!;

        public Material(Vector3 color, Texture? ambientTex = null, Texture? diffuseTex = null, Texture? specularTex = null)
        {
            this.color = color;
            if (ambientTex != null)
                this.ambientTex = ambientTex;
            if (diffuseTex != null)
                this.diffuseTex = diffuseTex;
            if (specularTex != null)
                this.specularTex = specularTex;
        }

        public void Render(ShaderProgram shader)
        {
            shader.SetVector3("material.color", color);

            shader.SetFloat("material.ambient", ambient);
            shader.SetFloat("material.diffuse", diffuse);
            shader.SetFloat("material.specular", specular);
            shader.SetFloat("material.shininess", 32f);

            if (ambientTex != null)
            {
                ambientTex.Bind();
                shader.SetBool("useAmbientTex", true);
            }
            else
            {
                shader.SetBool("useAmbientTex", false);
            }

            if (diffuseTex != null)
            {
                diffuseTex.Bind();
                shader.SetBool("useDiffuseTex", true);
            }
            else
            {
                shader.SetBool("useDiffuseTex", false);
            }

            if (specularTex != null)
            {
                specularTex.Bind();
                shader.SetBool("useSpecularTex", true);
            }
            else
            {
                shader.SetBool("useSpecularTex", false);
            }
        }
    }
}