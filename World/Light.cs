using OpenTK.Mathematics;

namespace GameEngine.World
{
    public class Light
    {
        Type type;
        public ShaderProgram shader;
        public GameObject source;
        public Vector3 direction = new Vector3(-0.5f, -1.0f, -0.3f);
        public Vector3 ambient;
        public Vector3 diffuse;
        public Vector3 specular;
        public Light(ShaderProgram shader, GameObject source)
        {
            this.shader = shader;
            this.source = source;
        }

        public virtual void Render()
        {
            shader.SetVector3("spotLight.position", source.position);
            shader.SetVector3("spotlight.direction", source.front);

            shader.SetVector3("spotLight.ambient", new Vector3(0.2f, 0.2f, 0.2f));
            shader.SetVector3("spotLight.diffuse", new Vector3(0.5f, 0.5f, 0.5f));
            shader.SetVector3("spotlight.specular", new Vector3(0.75f, 0.75f, 0.75f));
        }
    }

    public class PointLight : Light
    {
        public PointLight(ShaderProgram shader, GameObject source) : base(shader, source)
        {
            base.shader = shader;
        }

        public override void Render()
        {
            base.Render();

            shader.SetFloat("pointLight.constant", 1f);
            shader.SetFloat("pointLight.linear", 0.07f);
            shader.SetFloat("pointLight.quadratic", 0.017f);
        }
    }

    public class SpotLight : Light
    {
        public SpotLight(ShaderProgram shader, GameObject source) : base(shader, source)
        {
            base.shader = shader;
        }

        public override void Render()
        {
            base.Render();

            shader.SetFloat("spotLight.cutoff", (float)Math.Cos(MathHelper.DegreesToRadians(12.5)));           
        }
    }
}