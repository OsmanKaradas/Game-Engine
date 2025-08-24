#version 330 core
out vec4 FragColor;

in vec2 uv; // from FSQ vertex shader

uniform sampler2D gAlbedo;
uniform sampler2D gNormal;
uniform sampler2D gMaterial;
uniform sampler2D gDepth; // read from default depth attachment of GBuffer

uniform mat4 uInvProj; // inverse of projection matrix
uniform vec3 viewPos;

struct DirectionalLight {
    vec3 color;
    vec3 direction;
};
uniform DirectionalLight directionalLight;

struct PointLight {
    vec3 color;
    vec3 position;
    float radius;
};
uniform int pointCount;
uniform PointLight pointLights[16];

vec3 ReconstructPosition(vec2 uv, float depth)
{
    // NDC
    vec4 ndc = vec4(uv*2.0-1.0, depth*2.0-1.0, 1.0);
    // unproject to view space
    vec4 position = uInvProj * ndc;
    position /= position.w;
    return position.xyz;
}

void main(){
    vec3 albedo = texture(gAlbedo, uv).rgb;
    vec3 normal = normalize(texture(gNormal, uv).rgb * 2.0 - 1.0); // if you encoded [-1..1], otherwise just normalize()
    vec2 matRM = texture(gMaterial, uv).rg;
    float rough = clamp(matRM.r, 0.02, 1.0);
    float metal = clamp(matRM.g, 0.0, 1.0);

    float depth = texture(gDepth, uv).r;
    if(depth == 1.0) { // background
        discard;
    }
    vec3 position = ReconstructPosition(uv, depth);
    vec3 V = normalize(-position);

    // Simple Blinn-Phong-ish for demo
    vec3 norm = normalize(normal);
    vec3 finalColor = vec3(0.0);

    // Directional
    {
        vec3 dirLight = normalize(-directionalLight.direction);
        vec3 H = normalize(dirLight + norm);

        float diff = max(dot(norm, dirLight), 0.0);
        float spec = pow(max(dot(norm, H), 0.0), mix(8.0, 64.0, 1.0 - rough));

        vec3 diffuse = albedo * diff * directionalLight.color;
        vec3 specular = spec * directionalLight.color * 0.5;
        finalColor += diffuse + specular;
    }

    // Points
    for(int i = 0; i < pointCount; i++){

        vec3 lengthVec = pointLights[i].position - position;
        float distance = length(lengthVec);
        
        if(distance > pointLights[i].radius) continue;

        vec3 L = lengthVec / distance;
        vec3 H = normalize(L + V);

        float intensity = 1.0 - (distance / pointLights[i].radius);

        float diff = max(dot(norm, L), 0.0);
        float spec = pow(max(dot(norm, H), 0.0), mix(8.0, 64.0, 1.0 - rough));

        vec3 diffuse = albedo * diff * pointLights[i].color * intensity;
        vec3 specular = spec * pointLights[i].color * 0.5 * intensity;

        finalColor += diffuse + specular;
    }

    FragColor = vec4(finalColor, 1.0);
}
