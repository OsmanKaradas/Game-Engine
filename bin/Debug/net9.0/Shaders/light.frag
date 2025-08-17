#version 330 core

out vec4 FragColor;

struct Material
{
    float ambient;
    vec3 diffuse;
    float specular;

    sampler2D ambientTex;
    sampler2D diffuseTex;
    sampler2D specularTex;
    sampler2D normalTex;

    float shininess;
};

struct DirectionalLight
{
    vec3 position;
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct PointLight
{
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

struct SpotLight
{
    vec3 position;
    vec3 direction;
    float cutoff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform Material material;

uniform DirectionalLight directionalLight;
uniform PointLight pointLight;
uniform SpotLight spotLight;

uniform vec3 objectColor;

uniform vec3 viewPos;

uniform bool useDiffuseTex;
uniform bool useSpecularTex;

in vec3 normal;
in vec2 uv;
in vec3 FragPos;

void main()
{
    vec3 result;

    vec3 lightDir = normalize(spotLight.position - FragPos);
    float theta = dot(lightDir, normalize(-spotLight.direction));

    if(theta > spotLight.cutoff)
    {
        // ambient
        vec3 ambient = spotLight.ambient * material.ambient;

        // diffuse
        vec3 norm = normalize(normal);
        float diff = max(dot(norm, lightDir), 0.0f);
        vec3 diffuse = spotLight.diffuse * (diff * material.diffuse);

        // specular
        vec3 viewDir = normalize(viewPos - FragPos);
        vec3 reflectDir = reflect(-lightDir, norm);

        vec3 halfwayVec = normalize(viewDir + lightDir);

        float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
        vec3 specular = spotLight.specular * (spec * material.specular);

        if(useDiffuseTex){
            ambient = spotLight.ambient * vec3(texture(material.diffuseTex, uv));
            diffuse = spotLight.diffuse * diff * vec3(texture(material.diffuseTex, uv));
        }

        if(useSpecularTex)
            specular = spotLight.specular * spec * vec3(texture(material.specularTex, uv));

        result = ambient + diffuse + specular;
    }
    else
    {
        if(useDiffuseTex)
        {
            result = spotLight.ambient * vec3(texture(material.diffuseTex, uv));
        }
        else
        {
            result = spotLight.ambient * material.diffuse;
        }
    }

    /*
    float distance = length(pointLight.position - FragPos);
    float attenuation = 1.0 / (pointLight.constant + pointLight.linear * distance + pointLight.quadratic * (distance * distance));
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;*/
    

    FragColor = vec4(result, 1.0);
}