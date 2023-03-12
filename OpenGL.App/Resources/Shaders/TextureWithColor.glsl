#VertexShader
#version 330 core

layout (location = 0) in vec2 aPosition; //attribute variables start with 'a'. attribute are per-vertex parameters (typically : positions, normals, colors, UVs, ...) ;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;

out vec2 texCoord;
out vec4 color;

void main()
{
	color = aColor;
	gl_Position = vec4(aPosition.x, aPosition.y, 0, 1f);

	texCoord = aTexCoord;
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