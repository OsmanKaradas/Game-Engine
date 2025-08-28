#version 330 core

out vec4 FragColor;

//in vec3 fragPos;
//in vec3 normal;
//in vec2 uv;

//uniform vec3 viewPos;

uniform vec3 inColor;

/*vec4 CalcDirectionalLight()
{
    float light_ambient = 0.2f;
    float light_diffuse = 0.5f;
    float light_specular = 0.5f;

    vec3 lightDir = normalize(vec3(1.0f, 1.0f, 1.0f));
    vec3 viewDir = normalize(viewPos - fragPos);
    vec3 reflectDir = reflect(-lightDir, normal);

    float ambient = light_ambient;

    float diff = max(dot(normal, lightDir), 0.0f);
    float diffuse = diff * light_diffuse;

    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 16.0f);
    float specular = spec * light_specular;

    return vec4(inColor, 1.0f) * (ambient + diffuse + specular);
}*/

void main()
{  
    FragColor = vec4(inColor, 1.0f);
}
