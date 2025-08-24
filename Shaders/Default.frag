#version 330 core

out vec4 FragColor;

struct Material
{
    vec3 color;

    float ambient;
    float diffuse;
    float specular;

    float shininess;

    sampler2D ambientTex;
    sampler2D diffuseTex;
    sampler2D specularTex;
};

struct DirectionalLight
{
    vec4 color;
    vec3 direction;

    float ambient;
    float diffuse;
    float specular;
};

struct PointLight
{
    vec4 color;
    vec3 position;

    float ambient;
    float diffuse;
    float specular;

    float constant;
    float linear;
    float quadratic;
};

struct SpotLight
{
    vec4 color;
    vec3 position;
    vec3 direction;

    float innerCutoff;
    float outerCutoff;

    float ambient;
    float diffuse;
    float specular;

    float constant;
    float linear;
    float quadratic;
};

uniform Material material;

uniform DirectionalLight directionalLight;
uniform PointLight pointLight;
uniform SpotLight spotLight;

uniform vec3 viewPos;

uniform bool useAmbientTex;
uniform bool useDiffuseTex;
uniform bool useSpecularTex;


in vec3 normal;
in vec2 uv;
in vec3 FragPos;


vec4 CalcDirectionalLight(DirectionalLight light)
{
    vec3 lightDir = normalize(-light.direction);
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, normal);

    float diff = max(dot(normal, lightDir), 0.0);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    vec3 ambient = vec3(1.0f, 1.0f, 1.0f) * light.ambient * material.ambient;
    vec3 diffuse = vec3(1.0f, 1.0f, 1.0f) * light.diffuse * material.diffuse * diff;
    vec3 specular = vec3(1.0f, 1.0f, 1.0f) * light.specular * material.specular * spec;

    if(useAmbientTex){
        ambient = vec3(texture(material.ambientTex, uv)) * light.ambient;
    }    
    if(useDiffuseTex){
        diffuse = vec3(texture(material.diffuseTex, uv)) * light.diffuse;
    }    
    if(useSpecularTex){
        specular = vec3(texture(material.specularTex, uv)) * light.specular;
    }
    
    return vec4(material.color * (ambient + diffuse + specular), 1.0f);
    //return vec4(1.0f, 1.0f, 1.0f, 1.0f) * (ambient + diffuse + specular);
}

/*vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    // attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    // combine results
    if(useDiffuseTex)
    {
        ambient  = light.ambient  * vec3(texture(material.diffuseTex, uv));
        diffuse  = light.diffuse  * diff * vec3(texture(material.diffuseTex, uv));
    }
    else
    {
        ambient = light.ambient * material.ambient;
        diffuse = light.diffuse * diff * material.diffuse;
    }

    if(useSpecularTex)
    {
        specular = light.specular * spec * vec3(texture(material.specularTex, uv));
    }
    else
    {
        specular = light.specular * spec * material.specular;
    }
    
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

vec4 CalcSpotLight(SpotLight light)
{
    vec3 lightDir = normalize(light.position - FragPos);
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, normal);

    float ambient = light.ambient * material.ambient;

    float diff = max(dot(normal, lightDir), 0.0f);
    float diffuse = diff * light.diffuse * material.diffuse;
 
    float spec = pow(max(dot(viewDir, reflectDir), 0.0f), 16.0f);
    float specular = spec * light.specular * material.specular;
    
    float angle = dot(spotLight.direction, -lightDir);
    float intensity = clamp((angle - light.innerCutoff) / (light.innerCutoff - light.outerCutoff), 0.0f, 1.0f);
    vec4 result = (ambient + diffuse + specular) * light.color;

    return vec4(1.0f, 1.0f, 1.0f, 1.0f) * specular;
}
*/
void main()
{
    FragColor = CalcDirectionalLight(directionalLight);
}