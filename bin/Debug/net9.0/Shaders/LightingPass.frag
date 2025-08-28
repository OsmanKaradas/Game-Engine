#version 330 core

out vec4 FragColor;

in vec2 uv;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gMaterial;
uniform sampler2D gDepth;
uniform sampler2D shadowMap;

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

uniform mat4 lightSpaceMatrix;
uniform vec3 viewPos;

float CalcShadow(vec3 fragPos, vec3 normal)
{
    vec4 fragPosLightSpace = lightSpaceMatrix * vec4(fragPos, 1.0);
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;
    float bias = max(0.005 * (1.0 - dot(normal, normalize(-directionalLight.direction))), 0.0005);
    
    float shadow = 0.0f;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;        
        }    
    }
    shadow /= 9.0;

    if(projCoords.z > 1.0) shadow = 0.0;
    return shadow;
}

vec3 CalcDirectionalLight(vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, float inSpecular)
{
    vec3 lightDir = normalize(-directionalLight.direction);
    vec3 reflectDir = reflect(-lightDir, normal);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * inDiffuse * directionalLight.color;

    float spec = pow(max(dot(inViewDir, reflectDir), 0.0), 32.0);
    vec3 specular = directionalLight.color * (spec * inSpecular);

    float shadow = CalcShadow(fragPos, normal);

    return (diffuse + specular) * (1.0 - shadow);
}

vec3 CalcPointLight(PointLight light, vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, float inSpecular)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(lightDir + inViewDir);

    float distance = length(light.position - fragPos);

    float linear   = 0.045;
    float squared = 0.0075;
    float intensity = 1.0 / (1.0 + linear * distance + squared * (distance * distance));

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * inDiffuse * light.color * intensity;

    float spec = pow(max(dot(inViewDir, halfwayDir), 0.0), 32.0);
    vec3 specular = light.color * (spec * inSpecular) * intensity;

    return diffuse + specular;
}

vec3 CalcSpotLight(SpotLight light, vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, vec3 inSpecular)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(lightDir + inViewDir);

    float diff = max(dot(normal, lightDir), 0.0);

    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.innerCone - light.outerCone;
    float intensity = clamp((theta - light.outerCone) / epsilon, 0.0, 1.0);

    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (1.0 + light.linear * distance + light.squared * (distance * distance));

    vec3 result = (intensity * (inDiffuse * diff + inSpecular * spec) * light.color) * attenuation;

    return result;
}


void main()
{
    vec3 FragPos = texture(gPosition, uv).rgb;
    vec3 Normal  = texture(gNormal, uv).rgb;
    vec3 Diffuse = texture(gMaterial, uv).rgb;
    float Specular = texture(gMaterial, uv).a;

    vec3 viewDir = normalize(viewPos - FragPos);
    
    vec3 lighting = Diffuse * 0.1f;

    lighting += CalcDirectionalLight(FragPos, Normal, viewDir, Diffuse, Specular);
    for(int i = 0; i < pointLightCount; i++)
    {
        lighting += CalcPointLight(pointLights[i], FragPos, Normal, viewDir, Diffuse, Specular);
    }

    for(int i = 0; i < spotLightCount; i++)
    {
        //lighting += CalcSpotLight(spotLights[i], FragPos, Normal, viewDir, Diffuse, vec3(Specular));    
    }

    FragColor = vec4(lighting, 1.0);
}
