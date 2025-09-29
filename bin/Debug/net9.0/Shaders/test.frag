#version 330 core

out vec4 FragColor;

in vec3 fragPos;
in vec3 normal;
in vec2 uv;
in vec4 fragPosLight;

uniform vec3 viewPos;

struct Material
{
    vec3 color;
};

struct DirectionalLight
{
    vec3 color;
    vec3 direction;
};

struct PointLight
{
    vec3 color;
    vec3 position;

    float linear;
    float quadratic;
};

struct SpotLight
{
    vec3 color;
    vec3 position;
    vec3 direction;

    float linear;
    float quadratic;

    float innerCone;
    float outerCone;
};

uniform Material material;

uniform DirectionalLight directionalLight;
uniform PointLight pointLights[50];
uniform SpotLight spotLights[50];

uniform int pointLightsCount;
uniform int spotLightsCount;

uniform float ambientStrength;

uniform sampler2D depthMap;

float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898,78.233))) * 43758.5453);
}

float CalcShadow()
{
    vec3 projCoords = fragPosLight.xyz / fragPosLight.w;
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

    return shadow;
}

vec3 CalcDirectionalLight(vec3 viewDir)
{
    vec3 lightDir = normalize(-directionalLight.direction);
    vec3 halfwayDir = normalize(lightDir + viewDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = (material.color * 0.75f) * diff * directionalLight.color;
    
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular =  0.5f * spec * directionalLight.color;

    return(diffuse + specular) * (1.0 - CalcShadow());
}

vec3 CalcPointLight(PointLight light, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = (material.color * 0.75f) * diff * light.color;
    
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = spec * 0.5f * light.color;
    
    float distance = length(light.position - fragPos);
    float intensity = 1.0 / (1.0 + light.linear * distance + light.quadratic * (distance * distance));
    
    return (diffuse + specular) * intensity;
}

vec3 CalcSpotLight(SpotLight light, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = (material.color * 0.75f) * diff * light.color;
    
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = spec * 0.5f * light.color;
    
    float distance = length(light.position - fragPos);
    float intensity = 1.0 / (1.0 + light.linear * distance + light.quadratic * (distance * distance));
    
    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.innerCone - light.outerCone;
    float intensitySpot = smoothstep(0.0, 1.0, (theta - light.outerCone) / epsilon);
    diffuse *= intensitySpot;
    specular *= intensitySpot;
    
    return (diffuse + specular) * intensity;
}

void main()
{  
    vec3 lighting = material.color * ambientStrength;
    vec3 viewDir = normalize(viewPos - fragPos);

    lighting += CalcDirectionalLight(viewDir);
    for(int i = 0; i < pointLightsCount; i++)
        lighting += CalcPointLight(pointLights[i], viewDir);
    for(int i = 0; i < spotLightsCount; i++)
        lighting += CalcSpotLight(spotLights[i], viewDir);
    
    FragColor = vec4(lighting, 1.0f);

    /* GAMMA CORRECTION
    float gamma = 1.8;
    FragColor = vec4(pow(lighting, vec3(1.0 / gamma)), 1.0f);*/
    
    /* FOG EFFECT
    float near = 0.1f; float far = 75.0f;
    float linearDepth = (2.0 * near * far) / (far + near - (gl_FragCoord.z * 2.0 - 1.0) * (far - near));

    float steepness = 0.5f; float offset = 8.0f;
    float depth = 1 / (1 + exp(-steepness * (linearDepth - offset)));
    FragColor = vec4(lighting * (1.0f - depth) + (depth * vec3(0.85, 0.85, 0.9)), 1.0f);*/

}
