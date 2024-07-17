#VertexShader
#version 430 core

layout( location = 0)in vec2 Position;
layout( location = 1 )in vec2 TexCoord;
layout (location = 2) in vec2 aOffset;
layout (location = 3) in float aTextureID;
layout (location = 4) in float aIsVisible;

out vec2 f_uv;
out float textureID;
out float isVisible;

layout (std140) uniform ProjectionViewMatrix
{
    mat4 projection;
    mat4 view;
};

uniform mat4 model;

void main() {
    textureID = aTextureID;
    isVisible = aIsVisible;
    f_uv = TexCoord;
    gl_Position =  projection * view * model * vec4(Position + aOffset, 0.0, 1.0);
};

#FragmentShader
#version 430
#BindlessExtenstion

layout(location = 0) out vec4 color;
#SSBOBindlessTextureArray

in vec2 f_uv;
in float textureID;
in float isVisible;
uniform sampler2DArray u_tex;
uniform float selectedTexture;

void main()
{
    if (isVisible < 1.0) {
        color = vec4(0.0, 0.0, 0.0, 0.0);
    }else{
    color = #TextureIDToColor
    }
}