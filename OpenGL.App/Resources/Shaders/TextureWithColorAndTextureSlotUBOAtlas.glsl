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
    gl_Position =  projection * view * model * vec4(Position, 0.0, 1.0f);
};

#FragmentShader
#version 330 core

in vec2 f_uv;

uniform sampler2DArray u_tex;
uniform float textureUnit;

out vec4 color;

void main() {
    color = texture(u_tex, vec3(f_uv.x, f_uv.y, 3));                   
};