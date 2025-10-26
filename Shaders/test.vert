#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aUV;
layout(location = 3) in ivec4 aBoneIDs;
layout(location = 4) in vec4 aWeights;

out vec3 fragPos;
out vec3 normal;
out vec2 uv;
out vec4 fragPosLight_Dir;
out vec4 fragPosLight_Spot[5];

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 lightSpaceMatrix_Dir;
uniform mat4 lightSpaceMatrix_Spot[5];

uniform bool useArmature;
const int MAX_BONES = 100;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 finalBonesMatrices[MAX_BONES];

void main()
{
    vec4 skinnedPos = vec4(0.0);
    vec3 skinnedNormal = vec3(0.0);

    if(useArmature)
    {
        for (int i = 0; i < MAX_BONE_INFLUENCE; ++i)
        {
            int id = aBoneIDs[i];
            float w = aWeights[i];
            if (id < 0 || w <= 0.0) continue;
            if (id >= MAX_BONES) { skinnedPos = vec4(aPos,1.0); skinnedNormal = aNormal; break; }

            mat4 bone = finalBonesMatrices[id];
            skinnedPos += (bone * vec4(aPos, 1.0)) * w;
            skinnedNormal += (mat3(bone) * aNormal) * w;
        }
    }
    else
    {
        skinnedPos = vec4(aPos, 1.0);
        skinnedNormal = aNormal;
    }

    vec4 worldPos = model * skinnedPos;
    fragPos = worldPos.xyz;

    mat3 normalMatrix = mat3(transpose(inverse(model)));
    normal = normalize(normalMatrix * skinnedNormal);

    uv = aUV;
    fragPosLight_Dir = lightSpaceMatrix_Dir * vec4(fragPos, 1.0);
    
    for(int i = 0; i < 5; i++)
        fragPosLight_Spot[i] = lightSpaceMatrix_Spot[i] * vec4(fragPos, 1.0);

    gl_Position = projection * view * model * skinnedPos;
}
