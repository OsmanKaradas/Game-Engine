#version 330 core
out vec4 FragColor;

in vec2 uv;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gMaterial;

struct DirectionalLight {
    vec3 direction;
    vec3 color;
};

struct PointLight {
    vec3 position;
    vec3 color;

    float linear;
    float squared;
};

struct SpotLight{
    vec3 position;
    vec3 direction;
    vec3 color;

    float innerCone;
    float outerCone;

    float linear;
    float squared;
};
const int NR_LIGHTS = 100;

uniform DirectionalLight directionalLight;
uniform PointLight pointLights[NR_LIGHTS];
uniform SpotLight spotLights[NR_LIGHTS];

uniform int pointLightCount;
uniform int spotLightCount;
uniform vec3 viewPos;

vec3 CalcDirectionalLight(vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, float inSpecular)
{
    vec3 lightDir = normalize(-directionalLight.direction);
    vec3 reflectDir = reflect(-lightDir, normal);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * inDiffuse * directionalLight.color;

    float spec = pow(max(dot(inViewDir, reflectDir), 0.0), 32.0);
    vec3 specular = directionalLight.color * (spec * inSpecular);

    return (diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, float inSpecular)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 reflectDir = reflect(-lightDir, normal);

    float distance = length(light.position - fragPos);

    float linear   = 0.045;
    float squared = 0.0075;
    float intensity = 1.0 / (1.0 + linear * distance + squared * (distance * distance));

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * inDiffuse * light.color * intensity;

    float spec = pow(max(dot(inViewDir, reflectDir), 0.0), 32.0);
    vec3 specular = light.color * (spec * inSpecular) * intensity;

    return diffuse + specular;
}

vec3 CalcSpotLight(SpotLight light, vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, float inSpecular)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 reflectDir = reflect(-lightDir, normal);

    float distance = length(light.position - fragPos);
    float linear   = 0.045;
    float squared= 0.0075;
    float intensity = 1.0 / (1.0 + linear * distance + squared * (distance * distance));

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * inDiffuse * light.color * intensity;

    
    float spec = pow(max(dot(inViewDir, reflectDir), 0.0), 32.0);
    vec3 specular = light.color * (spec * inSpecular) * intensity;

    float angle = dot(-lightDir, light.direction);
    float inten = clamp((angle - light.outerCone) / (light.innerCone - light.outerCone), 0.0f, 1.0f);

    return (diffuse + specular) * inten;
}

void main()
{
    vec3 FragPos = texture(gPosition, uv).rgb;
    vec3 Normal  = texture(gNormal, uv).rgb;
    vec3 Diffuse = texture(gMaterial, uv).rgb;
    float Specular = texture(gMaterial, uv).a;

    vec3 viewDir = normalize(viewPos - FragPos);
    
    vec3 lighting = Diffuse * 0.2f;

    lighting += CalcDirectionalLight(FragPos, Normal, viewDir, Diffuse, Specular);
    for(int i = 0; i < pointLightCount; i++)
    {
        lighting += CalcPointLight(pointLights[i], FragPos, Normal, viewDir, Diffuse, Specular);
    }

    /*for(int i = 0; i < spotLightCount; i++)
    {
        lighting += CalcSpotLight(spotLights[i], FragPos, Normal, viewDir, Diffuse, Specular);    
    }*/
    //lighting += CalcSpotLight(spotLights[0], FragPos, Normal, viewDir, Diffuse, Specular);

    FragColor = vec4(lighting, 1.0);
}
