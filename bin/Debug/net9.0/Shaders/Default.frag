#version 330 core

in vec2 uv;

out vec4 FragColor;

uniform sampler2D diffuseTex;

uniform vec3 objectColor;

uniform bool useTexture;

void main()
{
    if(useTexture)
    {
        FragColor = texture(diffuseTex, uv);
    }
    else
    {
        FragColor = vec4(objectColor, 1.0);
    }
}