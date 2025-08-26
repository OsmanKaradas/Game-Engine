#version 330 core
layout(location=0) out vec3 gPosition;
layout(location=1) out vec3 gNormal;
layout(location=2) out vec4 gMaterial; // RGBA

in vec3 fragPos;
in vec3 normal;
in vec2 uv;

uniform sampler2D diffuseTex;
uniform sampler2D specularTex;
uniform vec3 inColor;

void main(){
    gPosition = fragPos;
    gNormal = normal;
    gMaterial.rgb = inColor;
    gMaterial.a = 0.5f;
}
