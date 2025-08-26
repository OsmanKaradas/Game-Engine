using OpenTK.Mathematics;
using GameEngine.Graphics;

namespace GameEngine.World
{
    public class Light
    {
        public int ID = 0;
        private static int count;
        public ShaderProgram shader;
        public GameObject source = null!;
        public Camera camera = null!;
        public float ambient = 0.2f;
        public float diffuse = 0.5f;
        public float specular = 0.75f;

        public Light(ShaderProgram shader, Camera camera, GameObject? source = null)
        {
            this.shader = shader;
            this.camera = camera;
            if (source != null)
                this.source = source;
            ID = count;
            count++;
        }

        public virtual void Render()
        {
            shader.SetVector3("viewPos", camera.position);
        }
    }

    public class DirectionalLight : Light
    {
        Vector3 direction;
        public DirectionalLight(ShaderProgram shader, Camera camera, Vector3 direction) : base(shader, camera)
        {
            this.direction = direction;
        }

        public override void Render()
        {
            base.Render();
            shader.SetVector3("directionalLight.direction", direction);

            shader.SetFloat("directionalLight.ambient", ambient);
            shader.SetFloat("directionalLight.diffuse", diffuse);
            shader.SetFloat("directionalLight.specular", specular);
        }
    }
    public class PointLight : Light
    {
        public float constant = 1f;
        public float linear = 0.045f;
        public float quadratic = 0.0075f;

        public PointLight(ShaderProgram shader, Camera camera, GameObject source) : base(shader, camera, source)
        {

        }

        public override void Render()
        {
            base.Render();
            shader.SetVector3("pointLight.position", source.position);

            shader.SetFloat($"pointLight.ambient", ambient);
            shader.SetFloat($"pointLight.diffuse", diffuse);
            shader.SetFloat($"pointLight.specular", specular);

            shader.SetFloat($"pointLight.constant", constant);
            shader.SetFloat($"pointLight.linear", linear);
            shader.SetFloat($"pointLight.quadratic", quadratic);
        }
    }

    public class SpotLight : Light
    {
        public SpotLight(ShaderProgram shader, Camera camera, GameObject source) : base(shader, camera, source)
        {
            
        }

        public override void Render()
        {
            base.Render();

            shader.SetVector3("spotLight.position", camera.position);
            shader.SetVector3("spotLight.direction", camera.front);
            
            shader.SetFloat("spotLight.innerCutoff", 0.95f);
            shader.SetFloat("spotLight.outerCutoff", 0.9f);

            shader.SetFloat($"spotLight.ambient", ambient);
            shader.SetFloat($"spotLight.diffuse", diffuse);
            shader.SetFloat($"spotLight.specular", specular);

            shader.SetFloat($"spotLight.constant", 1f);
            shader.SetFloat($"spotLight.linear", 0.045f);
            shader.SetFloat($"spotLight.quadratic", 0.0075f);
        }
    }
}