#version 330

in vec2 Uv;
out vec4 FragColor;
uniform sampler2D Side;

void main()
{
    FragColor = texture(Side, Uv);
}
