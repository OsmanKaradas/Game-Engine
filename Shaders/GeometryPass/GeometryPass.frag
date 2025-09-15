#version 330 core
layout(location=0) out vec3 gPosition;
layout(location=1) out vec3 gNormal;
layout(location=2) out vec3 gAlbedo;
layout(location=3) out vec3 gMaterial;


in vec3 fragPos;
in vec3 normal;
in vec2 uv;

struct Material
{
    vec3 color;

    float roughness;
    float metallic;
    float ao;
    
    sampler2D normalMap;
    sampler2D albedoMap;
    sampler2D roughnessMap;
    sampler2D metallicMap;
    sampler2D aoMap;
};

uniform Material material;

uniform bool useNormalMap;
uniform bool useAlbedoMap;
uniform bool useRoughnessMap;
uniform bool useMetallicMap;
uniform bool useAOMap;

void main(){
    gPosition = fragPos;

    if(useNormalMap)
        gNormal = texture(material.normalMap, uv).rgb * 2.0 - 1.0;
    else
        gNormal = normal;

    if(useAlbedoMap)
        gAlbedo = texture(material.albedoMap, uv).rgb;
    else
        gAlbedo = material.color;

    if(useRoughnessMap)
        gMaterial.r = texture(material.roughnessMap, uv).r;
    else
        gMaterial.r = material.roughness;

    if(useMetallicMap)
        gMaterial.g = texture(material.metallicMap, uv).r;
    else
        gMaterial.g = material.metallic;

    if(useAOMap)
        gMaterial.b = texture(material.aoMap, uv).r;
    else
        gMaterial.b = material.ao;
}
