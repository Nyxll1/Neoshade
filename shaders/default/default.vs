#version 330 core
layout (location = 0) in vec3 vPos; 
layout (location = 1) in vec2 vTex; 

out vec2 texCoords;

void main()
{
    texCoords = vTex;
    gl_Position = vec4(vPos, 1.0); 
}