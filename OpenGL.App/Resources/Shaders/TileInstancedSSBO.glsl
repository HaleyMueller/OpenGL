#VertexShader
#version 330 core

layout( location = 0)in vec2 Position;
layout( location = 1 )in vec2 TexCoord;
layout (location = 2) in vec2 aOffset;
layout (location = 3) in float aTextureID;

out vec2 f_uv;
out float textureID;

layout (std140) uniform ProjectionViewMatrix
{
    mat4 projection;
    mat4 view;
};



uniform mat4 model;

void main() {
    textureID = aTextureID;
    f_uv = TexCoord;
    gl_Position =  projection * view * model * vec4(Position + aOffset, 0.0, 1.0);
};

#FragmentShader
#version 330 core
#extension GL_ARB_bindless_texture : require

layout(location = 0) out vec4 color;

layout(std430, binding = 0) restrict readonly buffer TextureSSBO {
    sampler2D TextureID[];
} textureSSBO;

in vec2 f_uv;
in float textureID;
uniform sampler2DArray u_tex;
uniform float selectedTexture;

void main()
{
    sampler2D myTexture = textureSSBO.Textures[0];
    color = texture(myTexture, vec3(f_uv.x, f_uv.y, textureID));
}