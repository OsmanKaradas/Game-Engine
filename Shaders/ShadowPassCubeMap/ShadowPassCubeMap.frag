#version 330 core
#define BIAS 0.001

in vec4 FragPos;

uniform vec3 lightPos;
uniform float farPlane;

void main()
{
    gl_FragDepth = length(FragPos.xyz - lightPos) / farPlane;
    gl_FragDepth += gl_FrontFacing ? BIAS : 0.0;
} 