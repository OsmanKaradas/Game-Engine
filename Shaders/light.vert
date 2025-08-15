#version 330 core

layout (location = 0) in vec3 aPosition; // vertex coordinates
layout (location = 1) in vec3 aNormal; // normal coordinates
layout (location = 2) in vec2 aUV; // uv coordinates

out vec3 normal;
out vec2 uv;
out vec3 FragPos;

// uniform variables
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = vec4(aPosition, 1.0f) * model * view * projection; // coordinates
    FragPos = vec3(model * vec4(aPosition, 1.0));
    normal =  aNormal * mat3(transpose(inverse(model)));
    uv = aUV;
}