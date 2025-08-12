#version 330 core

in vec2 texCoord;

out vec4 FragColor;

uniform sampler2D texture0;
uniform vec4 objectColor;

uniform bool useTexture;

void main()
{
    // FragColor = vec4(1.0, 0, 0, 1.0); // Red
    /*if(useTexture)
    {
        FragColor = texture(texture0, texCoord);
    }
    else
    {
        FragColor = vec4(objectColor);
    }*/
    FragColor = objectColor;
}