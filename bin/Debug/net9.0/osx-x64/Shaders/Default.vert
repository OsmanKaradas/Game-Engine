#version 330 core

layout (location = 0) in vec3 aPosition; // vertex coordinates
layout (location = 1) in vec2 aUV; // texture coordinates

out vec2 uv;

// uniform variables
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection; // coordinates
    uv = aUV;
}