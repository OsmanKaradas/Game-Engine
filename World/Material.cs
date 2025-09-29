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

        public Material(Vector3 color, string? normalMapFilePath = null, string? albedoMapFilePath = null, string? roughnessMapFilePath = null, string? metallicMapFilePath = null, string? aoMapFilePath = null)
        {
            this.color = color;
            if (normalMapFilePath != null)
                this.normalMap = new(normalMapFilePath, TextureUnit.Texture0);

            if (albedoMapFilePath != null)
                this.albedoMap = new(albedoMapFilePath, TextureUnit.Texture1);

            if (roughnessMapFilePath != null)
                this.roughnessMap = new(roughnessMapFilePath, TextureUnit.Texture2);

            if (metallicMapFilePath != null)
                this.metallicMap = new(metallicMapFilePath, TextureUnit.Texture3);
                
            if (aoMapFilePath != null)
                this.aoMap = new(aoMapFilePath, TextureUnit.Texture4);
        }

        public void Render(ShaderProgram shader)
        {
            shader.SetVector3("material.color", color);
            shader.SetFloat("material.roughness", roughness);
            shader.SetFloat("material.metallic", metallic);
            shader.SetFloat("material.ao", ao);

            // Normal map
            if (normalMap != null)
            {
                normalMap.Bind();
                shader.SetInt("material.normalMap", (int)normalMap.unit - (int)TextureUnit.Texture0);
                shader.SetBool("useNormalMap", true);
            }
            else shader.SetBool("useNormalMap", false);

            // Albedo map
            if (albedoMap != null)
            {
                albedoMap.Bind();
                shader.SetInt("material.albedoMap", (int)albedoMap.unit - (int)TextureUnit.Texture0);
                shader.SetBool("useAlbedoMap", true);
            }
            else shader.SetBool("useAlbedoMap", false);

            // Roughness map
            if (roughnessMap != null)
            {
                roughnessMap.Bind();
                shader.SetInt("material.roughnessMap", (int)roughnessMap.unit - (int)TextureUnit.Texture0);
                shader.SetBool("useRoughnessMap", true);
            }
            else shader.SetBool("useRoughnessMap", false);

            // Metallic map
            if (metallicMap != null)
            {
                metallicMap.Bind();
                shader.SetInt("material.metallicMap", (int)metallicMap.unit - (int)TextureUnit.Texture0);
                shader.SetBool("useMetallicMap", true);
            }
            else shader.SetBool("useMetallicMap", false);

            // AO map (fixed boolean name)
            if (aoMap != null)
            {
                aoMap.Bind();
                shader.SetInt("material.aoMap", (int)aoMap.unit - (int)TextureUnit.Texture0);
                shader.SetBool("useAOMap", true);
            }
            else shader.SetBool("useAOMap", false);
}

    }
}