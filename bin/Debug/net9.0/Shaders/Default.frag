#version 330 core

in vec2 texCoord;

out vec4 FragColor;

uniform sampler2D texture0;
uniform vec3 objectColor;

void main()
{
    // FragColor = vec4(1.0, 0, 0, 1.0); // Red
    FragColor = vec4(objectColor, 1.0); 
    // FragColor = texture(texture0, texCoord);
}