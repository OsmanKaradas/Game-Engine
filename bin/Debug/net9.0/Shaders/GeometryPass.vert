#version 330 core
layout(location=0) in vec3 aPos;
layout(location=1) in vec3 aNormal;
layout(location=2) in vec2 aUV;
layout(location=3) in vec3 aTangent;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 pos;
out vec3 normal;
out vec2 uv;

void main(){
    pos = pos.xyz;
    mat3 matrix = mat3(view * model);
    normal = normalize(matrix * aNormal);
    uv = aUV;
    gl_Position = projection * view * model * vec4(aPos, 1.0f);
}
