#VertexShader
#version 330 core
in vec2 Position; // <vec2 pos, vec2 tex>
in vec2 TextureCoords; // <vec2 pos, vec2 tex>
out vec2 TexCoords;

uniform mat4 projection;

void main()
{
    gl_Position = projection * vec4(Position.xy, 0.0, 1.0);
    TexCoords = TextureCoords.xy;
}  
#FragmentShader
#version 330 core
in vec2 TexCoords;
out vec4 color;

uniform sampler2D text;
uniform vec4 textColor;

void main()
{    
    vec4 sampled = vec4(1.0, 1.0, 1.0, texture(text, TexCoords).r);
    color = textColor * sampled;
}  