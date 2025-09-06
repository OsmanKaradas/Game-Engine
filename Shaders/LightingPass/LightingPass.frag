#version 330 core

out vec4 FragColor;

in vec2 uv;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gMaterial;
uniform sampler2D gDepth;
uniform sampler2D depthMap;
uniform samplerCube depthCubeMap;

struct DirectionalLight {
    vec3 direction;
    vec3 color;
};

struct PointLight {
    vec3 position;
    vec3 color;

    float linear;
    float quadratic;
};

struct SpotLight{
    vec3 position;
    vec3 direction;
    vec3 color;

    float innerCone;
    float outerCone;

    float linear;
    float quadratic;
};
const int NR_LIGHTS_POINT = 50;
const int NR_LIGHTS_SPOT = 50;

uniform DirectionalLight directionalLight;
uniform PointLight pointLights[NR_LIGHTS_POINT];
uniform SpotLight spotLights[NR_LIGHTS_SPOT];

uniform int pointLightsCount;
uniform int spotLightsCount;

uniform mat4 lightSpaceMatrix;
uniform vec3 viewPos;

uniform float nearPlane;
uniform float farPlane;

float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898,78.233))) * 43758.5453);
}

float CalcShadow(vec3 fragPos, vec3 normal)
{
    vec4 fragPosLightSpace = lightSpaceMatrix * vec4(fragPos, 1.0);
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    float currentDepth = projCoords.z;
    if (projCoords.z > 1.0) return 0.0;

    float bias = clamp(0.005 * tan(acos(dot(normal, directionalLight.direction))), 0.0, 0.01);
    vec2 texelSize = 1.0 / vec2(textureSize(depthMap, 0));

    float randomAngle = rand(projCoords.xy) * 6.2831853; 
    float shadow = 0.0;
    
    int samples = 12;
    float radius = 2.5;
    
    for (int i = 0; i < samples; i++)
    {
        float r = sqrt(rand(projCoords.xy + float(i))) * radius;
        float a = (float(i) / float(samples)) * 6.2831853 + randomAngle;

        vec2 offset = r * vec2(cos(a), sin(a)) * texelSize;
        float pcfDepth = texture(depthMap, projCoords.xy + offset).r;
        shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
    }

    shadow /= float(samples);
    //shadow /= pow((radius * 2 + 1), 2);

    return shadow;
}

float CalcPointShadows(vec3 fragPos, vec3 normal, vec3 lightPos, float farPlane)
{
    vec3 fragToLight = fragPos - lightPos;
    float currentDepth = length(fragToLight);

    float shadow = 0.0;
    float bias = 0.05;
    int samples = 12;

    for (int i = 0; i < samples; ++i)
    {
        float closestDepth = texture(depthCubeMap, normalize(fragToLight)).r;
        closestDepth *= farPlane;

        if (currentDepth - bias > closestDepth)
            shadow += 1.0;
    }

    shadow /= float(samples);

    return shadow;
}

vec3 CalcDirectionalLight(vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, float inSpecular)
{
    vec3 lightDir = normalize(-directionalLight.direction);
    vec3 halfwayDir = normalize(lightDir + inViewDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = inDiffuse * diff * directionalLight.color;

    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = directionalLight.color * spec * inSpecular;

    float shadow = CalcShadow(fragPos, normal);

    return (diffuse + specular) * (1.0 - shadow);
}

vec3 CalcPointLight(PointLight light, vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, float inSpecular)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(lightDir + inViewDir);

    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (1.0 + light.linear * distance + light.quadratic * (distance * distance));

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * inDiffuse * light.color * attenuation;

    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = light.color * (spec * inSpecular) * attenuation;
    
    float shadow = CalcPointShadows(fragPos, normal, light.position, farPlane);

    return (diffuse + specular) * (1.0 - shadow);
}

vec3 CalcSpotLight(SpotLight light, vec3 fragPos, vec3 normal, vec3 inViewDir, vec3 inDiffuse, float inSpecular)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(lightDir + inViewDir);
    
    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.innerCone - light.outerCone;
    float intensity = clamp((theta - light.outerCone) / epsilon, 0.0, 1.0);

    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (1.0 + light.linear * distance + light.quadratic * (distance * distance));
    
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * inDiffuse * light.color * attenuation;

    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = light.color * (spec * inSpecular) * attenuation;

    float shadow = CalcShadow(fragPos, normal);

    return ((diffuse + specular) * intensity) * (1.0 - shadow);
}


void main()
{
    vec3 FragPos = texture(gPosition, uv).rgb;
    vec3 Normal  = texture(gNormal, uv).rgb;
    vec3 Diffuse = texture(gMaterial, uv).rgb;
    float Specular = texture(gMaterial, uv).a;

    vec3 viewDir = normalize(viewPos - FragPos);
    
    // ambient
    vec3 lighting = Diffuse * 0.1f;

    lighting += CalcDirectionalLight(FragPos, Normal, viewDir, Diffuse, Specular);

    for(int i = 0; i < pointLightsCount; i++)
    {
        lighting += CalcPointLight(pointLights[i], FragPos, Normal, viewDir, Diffuse, Specular);
    }
    
    for(int i = 0; i < spotLightsCount; i++)
    {
        lighting += CalcSpotLight(spotLights[i], FragPos, Normal, viewDir, Diffuse, Specular);    
    }

    FragColor = vec4(lighting, 1.0);
}
