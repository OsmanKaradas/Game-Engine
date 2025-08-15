#version 330 core

out vec4 FragColor;

struct Material
{
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    sampler2D ambientTex;
    sampler2D diffuseTex;
    sampler2D specularTex;

    float shininess;
};

struct Light
{
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform Material material;
uniform Light light;

uniform vec3 objectColor;

uniform vec3 viewPos;

uniform sampler2D texture0;
uniform bool useTexture;

in vec3 normal;
in vec2 uv;
in vec3 FragPos;

void main()
{
    // ambient
    vec3 ambient = light.ambient * material.ambient;

    // diffuse
    vec3 norm = normalize(normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(norm, lightDir), 0.0f);
    vec3 diffuse = light.diffuse * diff * material.diffuse;

    // specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * (spec * material.specular);

    vec3 result = (ambient + diffuse + specular) * objectColor;

    if(useTexture)
    {
        ambient = light.ambient * vec3(texture(material.ambientTex, uv));
        diffuse = light.diffuse * diff * vec3(texture(material.diffuseTex, uv));
        specular = light.specular * spec * vec3(texture(material.specularTex, uv));
        result = ambient + diffuse + specular;
    }

    FragColor = vec4(result, 1.0);
}