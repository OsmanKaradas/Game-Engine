#version 330 core
layout(location=0) out vec3 gAlbedo;
layout(location=1) out vec3 gNormal;
layout(location=2) out vec2 gMaterial; // x=roughness, y=metalness

in vec3 pos;
in vec3 normal;
in vec2 uv;

uniform sampler2D uDiffuseTex;
uniform sampler2D uSpecularTex;
uniform bool UseDiffuseTex;
uniform vec3 color;   // fallback color
uniform float roughness;    // [0..1]
uniform float metalness;    // [0..1]

void main(){
    vec3 albedo = UseDiffuseTex ? texture(uDiffuseTex, uv).rgb : color;
    gAlbedo = albedo;
    gNormal = normalize(normal);
    gMaterial = vec2(clamp(roughness,0.0,1.0), clamp(metalness,0.0,1.0));
}
