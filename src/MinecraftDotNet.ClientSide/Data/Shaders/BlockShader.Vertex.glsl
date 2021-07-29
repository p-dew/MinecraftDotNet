#version 330

in vec3 InVertex;
in vec2 InUv;
out vec2 Uv;

uniform mat4 MvpMatrix;

void main()
{
    Uv = InUv;
    gl_Position = MvpMatrix * vec4(InVertex, 1);
}
