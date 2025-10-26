#version 330 core
#define BIAS 0.001

in vec4 FragPos;

void main()
{
    gl_FragDepth = gl_FragCoord.z;
    gl_FragDepth += gl_FrontFacing ? BIAS : 0.0;
} 