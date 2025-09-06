#version 330 core

out vec4 FragColor;

in vec3 fragPos;
in vec3 normal;

uniform vec3 viewPos;

uniform vec3 inColor;

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

uniform DirectionalLight directionalLight;
uniform PointLight pointLight;

vec3 CalcDirectionalLight()
{
    vec3 lightDir = normalize(-directionalLight.direction);
    vec3 viewDir = normalize(viewPos - fragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = (inColor * 0.75f) * diff * directionalLight.color;
    
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular =  0.5f * spec * directionalLight.color;

    return(diffuse + specular);
}

vec3 CalcPointLight(PointLight light)
{
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 viewDir = normalize(viewPos - fragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = (inColor * 0.75f) * diff * light.color;
    
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = spec * 0.5f * light.color;
    
    float distance = length(light.position - fragPos);
    float intensity = 1.0 / (1.0 + light.linear * distance + light.quadratic * (distance * distance));
    
    return (diffuse + specular) * intensity;
}

void main()
{  
    vec3 lighting = inColor * 0.4f;

    lighting += CalcDirectionalLight();
    lighting += CalcPointLight(pointLight);

    FragColor = vec4(lighting, 1.0f);
}
