using OpenTK.Mathematics;
using GameEngine.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace GameEngine.World
{
    public class Material
    {
        public Vector3 color;
        public float roughness = 0.5f;
        public float metallic = 0.5f;
        public float ao = 0.5f;
        public float shininess = 32f;

        public Texture normalMap = null!;
        public Texture albedoMap = null!;
        public Texture roughnessMap = null!;
        public Texture metallicMap = null!;
        public Texture aoMap = null!;

        public Material(Vector3 color, Texture? normalMap = null, Texture? albedoMap = null, Texture? roughnessMap = null, Texture? metallicMap = null, Texture? aoMap = null)
        {
            this.color = color;
            if (normalMap != null)
                this.normalMap = normalMap;

            if (albedoMap != null)
                this.albedoMap = albedoMap;

            if (roughnessMap != null)
                this.roughnessMap = roughnessMap;

            if (metallicMap != null)
                this.metallicMap = metallicMap;
                
            if (aoMap != null)
                this.aoMap = aoMap;
        }

        public void Render(ShaderProgram shader)
        {
            shader.SetVector3("material.color", color);
            shader.SetFloat("material.roughness", roughness);
            shader.SetFloat("material.metallic", metallic);
            shader.SetFloat("material.ao", ao);

            if (normalMap != null)
            {
                normalMap.Bind();
                shader.SetBool("useNormalMap", true);
            }
            else
            {
                shader.SetBool("useNormalMap", false);
            }

            if (albedoMap != null)
            {
                albedoMap.Bind();
                shader.SetInt("material.albedoMap", (int)albedoMap.unit - (int)TextureUnit.Texture0);
                shader.SetBool("useAlbedoMap", true);
            }
            else
            {
                shader.SetBool("useAlbedoMap", false);
            }

            if (roughnessMap != null)
            {
                roughnessMap.Bind();
                shader.SetBool("useRoughnessMap", true);
            }
            else
            {
                shader.SetBool("useRoughnessMap", false);
            }


            if (metallicMap != null)
            {
                metallicMap.Bind();
                shader.SetBool("useMetallicMap", true);
            }
            else
            {
                shader.SetBool("useMetallicMap", false);
            }


            if (aoMap != null)
            {
                aoMap.Bind();
                shader.SetBool("useAOMap", true);
            }
            else
            {
                shader.SetBool("useAOMAP", false);
            }
        }
    }
}