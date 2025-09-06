using OpenTK.Mathematics;
using GameEngine.Graphics;

namespace GameEngine.World
{
    public class Light
    {
        public static ShaderProgram shader = null!;
        public static Camera camera = null!;

        public static DirectionalLight directionalLight = null!;
        public static List<PointLight> pointLights = new List<PointLight>();
        public static List<SpotLight> spotLights = new List<SpotLight>();

        public int ID;
        public Vector3 color;
        public float intensity = 1f;

        public Light(Vector3 color)
        {
            this.color = color;
        }

        public static void RenderLights()
        {
            shader.SetVector3("viewPos", camera.position);

            if (directionalLight != null)
                directionalLight.Render();

            foreach (PointLight pointLight in pointLights)
                pointLight.Render();
            
            foreach (SpotLight spotLight in spotLights)
            {
                spotLight.Render();
            }
        }
    }

    public class DirectionalLight : Light
    {
        public Vector3 direction;
        public DirectionalLight(Vector3 color, Vector3 direction) : base(color)
        {
            this.direction = direction;

            directionalLight = this;
        }

        public void Render()
        {
            shader.SetVector3("directionalLight.direction", direction);
            shader.SetVector3("directionalLight.color", color * intensity);
        }
    }
    public class PointLight : Light
    {
        public Vector3 position;
        public float linear = 0.045f, quadratic = 0.0075f;

        public PointLight(Vector3 color, Vector3 position) : base(color)
        {
            this.position = position;

            ID = pointLights.Count;
            pointLights.Add(this);
        }

        public void Render()
        {
            shader.SetInt("pointLightsCount", pointLights.Count);

            shader.SetVector3($"pointLights[{ID}].position", position);
            shader.SetVector3($"pointLights[{ID}].color", color * intensity);

            shader.SetFloat($"pointLights[{ID}].linear", linear);
            shader.SetFloat($"pointLights[{ID}].quadratic", quadratic);
        }
    }

    public class SpotLight : Light
    {
        public Vector3 position;
        public Vector3 direction;
        public float innerCone = 0.95f, outerCone = 0.93f;
        public float linear = 0.045f, quadratic = 0.0075f;

        public SpotLight(Vector3 color, Vector3 position, Vector3 direction) : base(color)
        {
            this.position = position;
            this.direction = direction.Normalized();

            ID = spotLights.Count;
            spotLights.Add(this);
        }

        public void Render()
        {
            shader.SetInt("spotLightsCount", spotLights.Count);

            shader.SetVector3($"spotLights[{ID}].position", position);
            shader.SetVector3($"spotLights[{ID}].direction", direction);

            shader.SetVector3($"spotLights[{ID}].color", color * intensity);

            shader.SetFloat($"spotLights[{ID}].innerCone", innerCone);
            shader.SetFloat($"spotLights[{ID}].outerCone", outerCone);
            shader.SetFloat($"spotLights[{ID}].linear", linear);
            shader.SetFloat($"spotLights[{ID}].quadratic", quadratic);
        }
    }
}