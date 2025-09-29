#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aUV;
layout(location = 3) in ivec4 aBoneIDs;
layout(location = 4) in vec4 aWeights;

out vec3 fragPos;
out vec3 normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

const int MAX_BONES = 100;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 finalBonesMatrices[MAX_BONES];

uniform bool useArmature;

void main()
{
    vec4 skinnedPos = vec4(0.0);
    vec3 skinnedNormal = vec3(0.0);

    if(useArmature)
    {
        for (int i = 0; i < MAX_BONE_INFLUENCE; i++)
        {
            if(aBoneIDs[i] < 0) continue;
            if(aBoneIDs[i] >= MAX_BONES) break;

            mat4 bone = finalBonesMatrices[aBoneIDs[i]];
            skinnedPos    += (bone * vec4(aPos, 1.0)) * aWeights[i];
            skinnedNormal += (mat3(bone) * aNormal) * aWeights[i];
        }
    }
    else
    {
        skinnedPos = vec4(aPos, 1.0);
        skinnedNormal = aNormal;
    }

    fragPos = vec3(model * skinnedPos);
    normal  = normalize(mat3(model) * skinnedNormal);

    gl_Position = projection * view * model * skinnedPos;
}
