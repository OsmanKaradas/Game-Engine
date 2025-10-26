#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 3) in ivec4 aBoneIDs;
layout(location = 4) in vec4 aWeights;

uniform mat4 model;
uniform mat4 lightSpaceMatrix;

uniform bool useArmature;
const int MAX_BONES = 100;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 finalBonesMatrices[MAX_BONES];

void main()
{
    vec4 skinnedPos = vec4(0.0);

    if(useArmature)
    {
        for (int i = 0; i < MAX_BONE_INFLUENCE; ++i)
        {
            int id = aBoneIDs[i];
            float w = aWeights[i];
            if (id < 0 || w <= 0.0) continue;
            if (id >= MAX_BONES) { skinnedPos = vec4(aPos,1.0); break; }

            mat4 bone = finalBonesMatrices[id];
            skinnedPos += (bone * vec4(aPos, 1.0)) * w;
        }
    }
    else
    {
        skinnedPos = vec4(aPos, 1.0);
    }
    
    gl_Position =  lightSpaceMatrix * model * skinnedPos;
}
