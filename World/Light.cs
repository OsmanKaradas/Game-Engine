using OpenTK.Mathematics;
using GameEngine.Graphics;
using static OpenTK.Graphics.OpenGL4.GL;
using OpenTK.Graphics.OpenGL4;
namespace GameEngine.World
{
    public class Light
    {
        public static ShaderProgram shader = null!;
        public static ShaderProgram shadowShader = null!;
        public static ShaderProgram shadowCubeMapShader = null!;
        
        public static Camera camera = null!;

        public static DirectionalLight directionalLight = null!;
        public static List<PointLight> pointLights = new List<PointLight>();
        public static List<SpotLight> spotLights = new List<SpotLight>();

        public int ID;
        public Vector3 color;
        public float intensity = 1f;
        public static float ambientStrength = 0.3f;

        public bool useShadow = false;
        public Light(Vector3 color, bool useShadow)
        {
            this.color = color;
            this.useShadow = useShadow;
        }

        public static void Setup(Camera camera, ShaderProgram shader, ShaderProgram? shadowShader = null, ShaderProgram? shadowCubeMapShader = null)
        {
            Light.camera = camera;
            Light.shader = shader;
            if(shadowShader != null)
                Light.shadowShader = shadowShader;
            if (shadowCubeMapShader != null)
                Light.shadowCubeMapShader = shadowCubeMapShader;

            UseProgram(shader.ID);
            if (directionalLight != null && directionalLight.useShadow == true)
                shader.SetInt("shadowMap_Dir", 0);

            shader.SetInt("shadowMap_Point", 1);
            shader.SetInt("shadowMap_Spot", 6);

            /*foreach (PointLight light in pointLights)
            {
                if (light.useShadow)
                {
                    if (light.ID == 0) shader.SetInt("shadowMap_Point", light.ID + 1);
                    if (light.ID == 1) shader.SetInt("shadowMap_Point1", light.ID + 1);
                    if (light.ID == 2) shader.SetInt("shadowMap_Point2", light.ID + 1);
                    if (light.ID == 3) shader.SetInt("shadowMap_Point3", light.ID + 1);
                    if (light.ID == 4) shader.SetInt("shadowMap_Point4", light.ID + 1);
                }
            }*/
            
            /*foreach(SpotLight light in spotLights)
            {
                if(light.useShadow)
                {
                    if(light.ID == 0) shader.SetInt("shadowMap_Spot", light.ID + 6);
                    if(light.ID == 1) shader.SetInt("shadowMap_Spot1", light.ID + 6);
                    if(light.ID == 2) shader.SetInt("shadowMap_Spot2", light.ID + 6);
                    if(light.ID == 3) shader.SetInt("shadowMap_Spot3", light.ID + 6);
                    if(light.ID == 4) shader.SetInt("shadowMap_Spot4", light.ID + 6);
                }
            }*/
        }
        
        public static void RenderLights()
        {
            shader.SetVector3("viewPos", camera.position);
            shader.SetFloat("farPlane", 75f);
            shader.SetFloat("ambientStrength", ambientStrength);

            if (directionalLight != null)
                directionalLight.RenderLight();

            foreach (PointLight pointLight in pointLights)
            {
                pointLight.RenderLight();
            }
            
            foreach (SpotLight spotLight in spotLights)
            {
                spotLight.RenderLight();
            }
        }

        public static void RenderShadows()
        {
            if(shadowShader != null)
            {
                UseProgram(shadowShader.ID);
                if (directionalLight != null && directionalLight.useShadow)
                    directionalLight.RenderShadow();
                
                UseProgram(shadowShader.ID);
                foreach (SpotLight light in spotLights)
                {
                    light.RenderShadow();
                }
            }
            
            if(shadowCubeMapShader != null)
            {
                UseProgram(shadowCubeMapShader.ID);
                foreach (PointLight light in pointLights)
                {
                    light.RenderShadow();
                }  
            }
        }
    }

    public class DirectionalLight : Light
    {
        public Vector3 direction;
        private ShadowFBO shadowFBO = null!;
        private Matrix4 lightSpaceMatrix = Matrix4.Identity;

        public DirectionalLight(Vector3 color, Vector3 direction, bool useShadow) : base(color, useShadow)
        {
            this.direction = direction.Normalized();
            
            if (useShadow)
                shadowFBO = new();
                
            Light.directionalLight = this;
        }

        public void RenderShadow()
        {
            if (!useShadow)
                return;
            Enable(EnableCap.DepthTest);
            Viewport(0, 0, shadowFBO.width, shadowFBO.height);
            shadowFBO.Bind();
            Clear(ClearBufferMask.DepthBufferBit);
            Disable(EnableCap.CullFace);
            Matrix4 shadowProjection = Matrix4.CreateOrthographic(35f, 35f, 0.1f, 75f);
            Matrix4 shadowView = Matrix4.LookAt(20f * direction, Vector3.Zero, new(0f, 1f, 0f));
            lightSpaceMatrix = shadowView * shadowProjection;
            shadowShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            GameObject.Render(shadowShader);
            shadowFBO.Unbind();
        }
        
        public void RenderLight()
        {
            shader.SetVector3("directionalLight.direction", direction);
            shader.SetVector3("directionalLight.color", color * intensity);
            shader.SetMatrix4("lightSpaceMatrix_Dir", lightSpaceMatrix);

            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, shadowFBO.depthMap);
        }
    }
    public class PointLight : Light
    {
        public Vector3 position;
        public float linear = 0.045f, quadratic = 0.0075f;
        private ShadowFBOCubeMap shadowFBO = null!;

        public PointLight(Vector3 color, Vector3 position, bool useShadow) : base(color, useShadow)
        {
            this.position = position;

            if (useShadow)
                shadowFBO = new();

            ID = pointLights.Count;
            pointLights.Add(this);
        }

        public void RenderShadow()
        {
            if (!useShadow)
                return;
            Viewport(0, 0, shadowFBO.width, shadowFBO.height);
            shadowFBO.Bind();
            Clear(ClearBufferMask.DepthBufferBit);

            Matrix4 shadowProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1f, 0.1f, 75f);
            Matrix4[] shadowTransforms = {
                Matrix4.LookAt(position, position + new Vector3(1f, 0f, 0f), new(0f, -1f, 0f)) * shadowProjection,
                Matrix4.LookAt(position, position + new Vector3(-1f, 0f, 0f), new(0f, -1f, 0f)) * shadowProjection,
                Matrix4.LookAt(position, position + new Vector3(0f, 1f, 0f), new(0f, 0f, 1f)) * shadowProjection,
                Matrix4.LookAt(position, position + new Vector3(0f, -1f, 0f), new(0f, 0f, -1f)) * shadowProjection,
                Matrix4.LookAt(position, position + new Vector3(0f, 0f, 1f), new(0f, -1f, 0f)) * shadowProjection,
                Matrix4.LookAt(position, position + new Vector3(0f, 0f, -1f), new(0f, -1f, 0f)) * shadowProjection,
            };

            shadowCubeMapShader.SetVector3("lightPos", position);
            shadowCubeMapShader.SetMatrix4($"shadowMatrices[{0}]", shadowTransforms[0]);
            shadowCubeMapShader.SetMatrix4($"shadowMatrices[{1}]", shadowTransforms[1]);
            shadowCubeMapShader.SetMatrix4($"shadowMatrices[{2}]", shadowTransforms[2]);
            shadowCubeMapShader.SetMatrix4($"shadowMatrices[{3}]", shadowTransforms[3]);
            shadowCubeMapShader.SetMatrix4($"shadowMatrices[{4}]", shadowTransforms[4]);
            shadowCubeMapShader.SetMatrix4($"shadowMatrices[{5}]", shadowTransforms[5]);

            GameObject.Render(shadowCubeMapShader);
            shadowFBO.Unbind();
        }
        
        public void RenderLight()
        {
            shader.SetInt("pointLightsCount", pointLights.Count);

            shader.SetVector3($"pointLights[{ID}].position", position);
            shader.SetVector3($"pointLights[{ID}].color", color * intensity);

            shader.SetFloat($"pointLights[{ID}].linear", linear);
            shader.SetFloat($"pointLights[{ID}].quadratic", quadratic);

            shader.SetBool($"pointLights[{ID}].useShadow", useShadow);
            if(useShadow)
            {
                ActiveTexture(TextureUnit.Texture1 + ID);
                BindTexture(TextureTarget.TextureCubeMap, shadowFBO.depthCubeMap);
            }
        }
    }

    public class SpotLight : Light
    {
        public Vector3 position;
        public Vector3 direction;
        public float innerCone = MathF.Cos(MathHelper.DegreesToRadians(12.5f)), outerCone = MathF.Cos(MathHelper.DegreesToRadians(17.5f));
        public float linear = 0.045f, quadratic = 0.0075f;

        private ShadowFBO shadowFBO = null!;
        private Matrix4 lightSpaceMatrix = Matrix4.Identity;

        public SpotLight(Vector3 color, Vector3 position, Vector3 direction, bool useShadow) : base(color, useShadow)
        {
            this.position = position;
            this.direction = direction.Normalized();

            if (useShadow)
                shadowFBO = new();

            ID = spotLights.Count;
            spotLights.Add(this);
        }

        public void RenderShadow()
        {
            if (!useShadow)
                return;
            Viewport(0, 0, shadowFBO.width, shadowFBO.height);
            shadowFBO.Bind();
            Clear(ClearBufferMask.DepthBufferBit);
            Matrix4 shadowProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1.0f, 1.0f, 75.0f);
            Matrix4 shadowView = Matrix4.LookAt(position, position + direction, new(0f, 0f, 1f));
            lightSpaceMatrix = shadowView * shadowProjection;
            shadowShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            GameObject.Render(shadowShader);
            shadowFBO.Unbind();
        }
        
        public void RenderLight()
        {
            shader.SetInt("spotLightsCount", spotLights.Count);

            shader.SetVector3($"spotLights[{ID}].position", position);
            shader.SetVector3($"spotLights[{ID}].direction", direction);

            shader.SetVector3($"spotLights[{ID}].color", color * intensity);

            shader.SetFloat($"spotLights[{ID}].innerCone", innerCone);
            shader.SetFloat($"spotLights[{ID}].outerCone", outerCone);
            shader.SetFloat($"spotLights[{ID}].linear", linear);
            shader.SetFloat($"spotLights[{ID}].quadratic", quadratic);

            shader.SetBool($"spotLights[{ID}].useShadow", useShadow);

            if (!useShadow)
                return;

            shader.SetMatrix4($"lightSpaceMatrix[{ID}]", lightSpaceMatrix);
            ActiveTexture(TextureUnit.Texture6 + ID);
            BindTexture(TextureTarget.Texture2D, shadowFBO.depthMap);
        }
    }
}