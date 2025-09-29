#version 330 core

out vec4 FragColor;
in vec3 fragPos;
in vec3 normal;

struct Material{
    vec3 color; 
};

uniform Material material; 
uniform vec3 viewPos;

void main()
{
    vec3 lightDir = normalize(vec3(0.3f, 0.5f, 0.5f));
    vec3 viewDir = normalize(viewPos - fragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    
    float diff = max(dot(normal, lightDir), 0.0);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    vec3 result = material.color * 0.8f;
    result += vec3(diff) * 0.4f;
    result += vec3(spec) * 0.4f;

    FragColor = vec4(result, 1.0);
}