#version 330 core

out vec4 FragColor;

in vec3 fragPos;
in vec3 normal;
in vec2 uv;
in vec4 fragPosLight_Dir;
in vec4 fragPosLight_Spot[5];

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

    bool useShadow;
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

    bool useShadow;
};

uniform Material material;

uniform DirectionalLight directionalLight;
uniform PointLight pointLights[50];
uniform SpotLight spotLights[50];

uniform int pointLightsCount;
uniform int spotLightsCount;

uniform float ambientStrength;

uniform sampler2D shadowMap_Dir;

uniform samplerCube shadowMap_Point;
uniform samplerCube shadowMap_Point1;
uniform samplerCube shadowMap_Point2;
uniform samplerCube shadowMap_Point3;
uniform samplerCube shadowMap_Point4;

uniform sampler2D shadowMap_Spot;
uniform sampler2D shadowMap_Spot1;
uniform sampler2D shadowMap_Spot2;
uniform sampler2D shadowMap_Spot3;
uniform sampler2D shadowMap_Spot4;

uniform float farPlane;

float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898,78.233))) * 43758.5453);
}

float CalcDirShadow()
{
    vec3 projCoords = fragPosLight_Dir.xyz / fragPosLight_Dir.w;
    projCoords = projCoords * 0.5 + 0.5;

    float currentDepth = projCoords.z;
    if (projCoords.z > 1.0) return 0.0;

    float cosTheta = max(dot(normal, directionalLight.direction), 0.0);
    float bias = max(0.0005 * (1.0 - cosTheta), 0.0005);
      
    vec2 texelSize = 1.0 / vec2(textureSize(shadowMap_Dir, 0));

    float randomAngle = rand(projCoords.xy) * 6.2831853; 
    float shadow = 0.0;
    
    int samples = 12;
    float radius = 2.5;
    
    for (int i = 0; i < samples; i++)
    {
        float r = sqrt(rand(projCoords.xy + float(i))) * radius;
        float a = (float(i) / float(samples)) * 6.2831853 + randomAngle;

        vec2 offset = r * vec2(cos(a), sin(a)) * texelSize;
        float pcfDepth = texture(shadowMap_Dir, projCoords.xy + offset).r;
        shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
    }

    shadow /= float(samples);

    return shadow;
}

float CalcPointShadow(PointLight light, samplerCube shadowCubeMap)
{
    vec3 fragToLight = fragPos - light.position;
    float currentDepth = length(fragToLight);

    vec3 sampleDir = normalize(fragToLight);

    if (currentDepth >= farPlane) return 0.0;

    float cosTheta = max(dot(normal, -normalize(fragToLight)), 0.0);
    float bias = max(0.01 * (1.0 - cosTheta), 0.002);

    float shadow = 0.0;
    int samples = 6;
    float diskRadius = 0.05;

    float randomAngle = rand(fragToLight.xy) * 6.2831853;

    for (int i = 0; i < samples; i++)
    {
        float r = sqrt(rand(fragToLight.xy + float(i))) * diskRadius;
        float a = (float(i) / float(samples)) * 6.2831853 + randomAngle;

        vec3 offset = r * vec3(cos(a), sin(a), 0.0);
        vec3 sampleDirOffset = normalize(fragToLight + offset);

        float closestDepth = texture(shadowCubeMap, sampleDirOffset).r * farPlane;

        shadow += (currentDepth - bias > closestDepth) ? 1.0 : 0.0;
    }

    shadow /= float(samples);

    return shadow;
}

float CalcSpotShadow(SpotLight light, sampler2D shadowMap, vec4 fragPosLightSpace)
{
    float shadow = 0.0;

    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    float currentDepth = projCoords.z;
    if (projCoords.z > 1.0) return 0.0;

    float cosTheta = max(dot(normal, light.direction), 0.0);
    float bias = max(0.0005 * (1.0 - cosTheta), 0.0005);
    
    vec2 texelSize = 1.0 / vec2(textureSize(shadowMap, 0).xy);

    float randomAngle = rand(projCoords.xy) * 6.2831853; 
    
    int samples = 12;
    float radius = 2.5;
    
    for (int i = 0; i < samples; i++)
    {
        float r = sqrt(rand(projCoords.xy + float(i))) * radius;
        float a = (float(i) / float(samples)) * 6.2831853 + randomAngle;

        vec2 offset = r * vec2(cos(a), sin(a)) * texelSize;

        float pcfDepth = texture(shadowMap, projCoords.xy + offset).r;

        shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
    }

    shadow /= float(samples);

    return shadow;
}

vec3 CalcDirectionalLight(vec3 viewDir)
{
    vec3 lightDir = normalize(directionalLight.direction);
    vec3 halfwayDir = normalize(viewDir + lightDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = material.color * diff * directionalLight.color;

    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    if(diff == 0.0f)
        spec = 0.0f;
        
    vec3 specular = directionalLight.color * spec * 0.5f;

    float shadow = CalcDirShadow();

    return (diffuse + specular) * (1.0 - shadow);
}

vec3 CalcPointLight(PointLight light, vec3 viewDir, int index)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = (material.color * 0.75f) * diff * light.color;
    
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = spec * 0.5f * light.color;
    
    float distance = length(light.position - fragPos);
    float intensity = 1.0 / (1.0 + light.linear * distance + light.quadratic * (distance * distance));

    float shadow = 0.0f;
    
    if(light.useShadow)
    {
        shadow = CalcPointShadow(light, shadowMap_Point);
        /*
        if(index == 0) shadow = CalcPointShadow(light, shadowMap_Point);
        //else if(index == 1) shadow = CalcPointShadow(light, shadowMap_Point1);
        //else if(index == 2) shadow = CalcPointShadow(light, shadowMap_Point2);
        //else if(index == 3) shadow = CalcPointShadow(light, shadowMap_Point3);
        //else if(index == 4) shadow = CalcPointShadow(light, shadowMap_Point4);*/
    }

    return ((diffuse + specular) * intensity) * (1.0 - shadow);
}

vec3 CalcSpotLight(SpotLight light, vec3 viewDir, int index)
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

    float shadow = 0.0f;
    
    if(light.useShadow)
    {
        shadow = CalcSpotShadow(light, shadowMap_Spot, fragPosLight_Spot[0]);
        /*
        if(index == 0)  shadow = CalcSpotShadow(light, shadowMap_Spot, fragPosLight_Spot[0]);
        else if(index == 1)  shadow = CalcSpotShadow(light, shadowMap_Spot1, fragPosLight_Spot[1]);
        else if(index == 2)  shadow = CalcSpotShadow(light, shadowMap_Spot2, fragPosLight_Spot[2]);
        else if(index == 3)  shadow = CalcSpotShadow(light, shadowMap_Spot3, fragPosLight_Spot[3]);
        else if(index == 4)  shadow = CalcSpotShadow(light, shadowMap_Spot4, fragPosLight_Spot[4]);
        */
    }

    return ((diffuse + specular) * intensity) * (1.0 - shadow);
}

void main()
{  
    vec3 lighting = material.color * ambientStrength;
    vec3 viewDir = normalize(viewPos - fragPos);

    lighting += CalcDirectionalLight(viewDir);
    for(int i = 0; i < pointLightsCount; i++)
        lighting += CalcPointLight(pointLights[i], viewDir, i);
    for(int i = 0; i < spotLightsCount; i++)
        lighting += CalcSpotLight(spotLights[i], viewDir, i);
    
    FragColor = vec4(lighting, 1.0f);

    // GAMMA CORRECTION
    float gamma = 1.5;
    FragColor = vec4(pow(lighting, vec3(1.0 / gamma)), 1.0f);
    
    /* FOG EFFECT
    float near = 0.1f; float far = 75.0f;
    float linearDepth = (2.0 * near * far) / (far + near - (gl_FragCoord.z * 2.0 - 1.0) * (far - near));

    float steepness = 0.5f; float offset = 8.0f;
    float depth = 1 / (1 + exp(-steepness * (linearDepth - offset)));
    FragColor = vec4(lighting * (1.0f - depth) + (depth * vec3(0.85, 0.85, 0.9)), 1.0f);*/

}
