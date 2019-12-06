-- Version
#version 330


-- Vertex
#include BlockShader.Version

in vec3 InVertex;
in vec2 InUv;
out vec2 Uv;

uniform mat4 MvpMatrix;
uniform vec3 BlockPosition;

void main()
{
    Uv = InUv;
    gl_Position = MvpMatrix * vec4(InVertex + BlockPosition, 1);
}


-- Fragment
#include BlockShader.Version

in vec2 Uv;
out vec4 FragColor;
uniform sampler2D Side;

void main()
{
    FragColor = texture(Side, Uv);
}