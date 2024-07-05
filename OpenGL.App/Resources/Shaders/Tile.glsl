#VertexShader
#version 330 core

layout( location = 0)in vec2 Position;
layout( location = 1 )in vec2 TexCoord;
out vec2 f_uv;

layout (std140) uniform ProjectionViewMatrix
{
    mat4 projection;
    mat4 view;
};

uniform mat4 model;

void main() {
    f_uv = TexCoord;
    gl_Position =  projection * view * model * vec4(Position, 0.0, 1.0);
};

#FragmentShader
#version 330 core

layout(location = 0) out vec4 color;

in vec2 f_uv;
#BindlessFallbackCreateUniform

void main()
{
    color = #BindlessFallbackCreateTexture
}