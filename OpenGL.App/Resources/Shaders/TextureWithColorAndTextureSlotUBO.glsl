#VertexShader
#version 330 core

layout (location = 0) in vec2 Position; //attribute variables start with 'a'. attribute are per-vertex parameters (typically : positions, normals, colors, UVs, ...) ;
layout (location = 1) in vec2 TexCoord;
layout (location = 2) in vec4 Color;

layout (std140) uniform Matrices
{
    mat4 projection;
    mat4 view;
};

uniform mat4 model;

out vec2 texCoord;
out vec4 color;

void main()
{
	color = Color;
	gl_Position = projection * view * model * vec4(Position.x, Position.y, 0, 1f);

	texCoord = TexCoord;
}
#FragmentShader
#version 330 core

in vec2 texCoord;
in vec4 color;

out vec4 pixelColor;
uniform sampler2D texture0;

void main()
{
	pixelColor = texture(texture0, texCoord) * color;
}