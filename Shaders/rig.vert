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
            if(aBoneIDs[i] == -1)
            {
                continue;
            }

            if(aBoneIDs[i] >= MAX_BONES) 
            {
                skinnedPos = vec4(aPos, 1.0f);
                skinnedNormal = mat3(transpose(inverse(model))) * aNormal;
                break;
            }

            vec4 localPos = finalBonesMatrices[aBoneIDs[i]] * vec4(aPos, 1.0);
            vec3 localNormal = mat3(finalBonesMatrices[aBoneIDs[i]]) * aNormal;
            skinnedPos += localPos * aWeights[i];
            skinnedNormal += localNormal * aWeights[i];
        }
    }
    else
    {
        skinnedPos = vec4(aPos, 1.0f);
        skinnedNormal = mat3(transpose(inverse(model))) * aNormal;
    }

    fragPos = vec3(skinnedPos);
    normal = normalize(skinnedNormal);

    gl_Position = projection * view * model * skinnedPos;
}
