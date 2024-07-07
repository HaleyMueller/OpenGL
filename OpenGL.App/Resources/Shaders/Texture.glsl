#VertexShader
#version 330 core

layout (location = 0) in vec2 aPosition; //attribute variables start with 'a'. attribute are per-vertex parameters (typically : positions, normals, colors, UVs, ...) ;
layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;

void main()
{
	gl_Position = vec4(aPosition.x, aPosition.y, 0, 1.0f);

	texCoord = aTexCoord;
}
#FragmentShader
#version 330 core

in vec2 texCoord;

out vec4 pixelColor;
uniform sampler2D texture0;

void main()
{
	pixelColor = texture(texture0, texCoord);
}