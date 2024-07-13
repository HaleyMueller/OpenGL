using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL.App.Core.UniformBufferObject;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGL.App.Core.Shader
{
    public readonly struct ShaderUniform
    {
        public readonly string Name;
        public readonly int Location;
        public readonly ActiveUniformType Type;

        public ShaderUniform(string name, int location, ActiveUniformType type)
        {
            Name = name;
            Location = location;
            Type = type;
        }
    }

    public readonly struct ShaderAttribute
    {
        public readonly string Name;
        public readonly int Location;
        public readonly ActiveAttribType Type;

        public ShaderAttribute(string name, int location, ActiveAttribType type)
        {
            Name = name;
            Location = location;
            Type = type;
        }
    }

    public sealed class ShaderProgram : IDisposable
    {
        private bool disposed;

        public List<UniformBufferObjectFactory.UBOIndex> UsedUBOs = new List<UniformBufferObjectFactory.UBOIndex>();

        public readonly int ShaderProgramHandle;
        public readonly int VertexShaderHandle;
        public readonly int PixelShaderHandle;

        private readonly Dictionary<string, ShaderUniform> Uniforms;
        private readonly Dictionary<string, ShaderAttribute> Attributes;

        public ShaderProgram(string fileName)
        {
            disposed = false;

            var shader = LoadShader(fileName);

            if (CompileVertexShader(shader.VertexShader, out VertexShaderHandle, out string vertexShaderCompileError) == false) //If errored
            {
                throw new ArgumentException(vertexShaderCompileError);
            }

            if (CompilePixelShader(shader.FragmentShader, out PixelShaderHandle, out string pixelShaderCompileError) == false) //If errored
            {
                throw new ArgumentException(pixelShaderCompileError);
            }

            ShaderProgramHandle = CreateLinkProgram(VertexShaderHandle, PixelShaderHandle);

            Uniforms = CreateUniformList(ShaderProgramHandle);
            Attributes = CreateAttributeList(ShaderProgramHandle);
        }

        public void Use()
        {
            GL.UseProgram(ShaderProgramHandle);
        }

        internal void GPU_Use_UBOS()
        {
            foreach (var ubo in this.UsedUBOs)
            {
                var shaderBlockIndex = GL.GetUniformBlockIndex(ShaderProgramHandle, ubo.ToString());
                GL.UniformBlockBinding(ShaderProgramHandle, shaderBlockIndex, (int)ubo);
            }
        }

        #region OpenGL

        public ShaderUniform GetUniform(string name)
        {
            if (Uniforms.TryGetValue(name, out var uniform) == false)
                throw new ArgumentException($"Shader uniform name was not found {name}");

            return uniform;
        }

        public void SetUniform(string name, float v1)
        {
            var uniform = GetUniform(name);

            if (uniform.Type != ActiveUniformType.Float)
            {
                throw new ArgumentException("Shader uniform type is not float");
            }

            GL.UseProgram(ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.Uniform1(uniform.Location, v1); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        public void SetUniform(string name, Matrix4 matrix4)
        {
            var uniform = GetUniform(name);

            if (uniform.Type != ActiveUniformType.FloatMat4)
            {
                throw new ArgumentException("Shader uniform type is not Matrix4");
            }

            GL.UseProgram(ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.UniformMatrix4(uniform.Location, false, ref matrix4); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        public void SetUniform(string name, float v1, float v2)
        {
            var uniform = GetUniform(name);

            if (uniform.Type != ActiveUniformType.Float)
            {
                throw new ArgumentException("Shader uniform type is not float");
            }

            GL.UseProgram(ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.Uniform2(uniform.Location, v1, v2); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        public void SetUniform(string name, float v1, float v2, float v3)
        {
            var uniform = GetUniform(name);

            if (uniform.Type != ActiveUniformType.Float)
            {
                throw new ArgumentException("Shader uniform type is not float");
            }

            GL.UseProgram(ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.Uniform3(uniform.Location, v1, v2, v3); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        public void SetUniform(string name, float v1, float v2, float v3, float v4)
        {
            var uniform = GetUniform(name);

            if (uniform.Type != ActiveUniformType.Float)
            {
                throw new ArgumentException("Shader uniform type is not float");
            }

            GL.UseProgram(ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.Uniform4(uniform.Location, v1, v2, v3, v4); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        public static bool CompileVertexShader(string vertexShaderCode, out int vertexShaderHandle, out string errorMessage)
        {
            errorMessage = string.Empty;

            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader); //Grab vertex shader handle from gpu
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode); //Give it the shader code
            GL.CompileShader(vertexShaderHandle); //Compile

            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderHandle);
            if (vertexShaderInfo != string.Empty)
            {
                errorMessage = vertexShaderInfo;
                Console.WriteLine(vertexShaderInfo);
                return false;
            }

            return true;
        }

        public static bool CompilePixelShader(string pixelShaderCode, out int pixelShaderHandle, out string errorMessage)
        {
            errorMessage = string.Empty;

            pixelShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderHandle, pixelShaderCode);
            GL.CompileShader(pixelShaderHandle);

            string pixelShaderInfo = GL.GetShaderInfoLog(pixelShaderHandle);
            if (pixelShaderInfo != string.Empty)
            {
                errorMessage = pixelShaderInfo;
                Console.WriteLine(pixelShaderInfo);
                return false;
            }

            return true;
        }

        public static int CreateLinkProgram(int vertexShaderHandle, int pixelShaderHandle)
        {
            //Combine the shaders into a shader program
            int shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle); //Attach shader to program
            GL.AttachShader(shaderProgramHandle, pixelShaderHandle); //Attach shader to same program

            GL.LinkProgram(shaderProgramHandle); //Link all shaders to program

            //Get rid of shaders in RAM after giving shaders to GPU to keep
            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, pixelShaderHandle);


            return shaderProgramHandle;
        }

        public static Dictionary<string, ShaderUniform> CreateUniformList(int shaderProgramHandle)
        {
            var uniforms = new Dictionary<string, ShaderUniform>();

            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveUniforms, out int uniformCount);

            for (int i = 0; i < uniformCount; i++)
            {
                GL.GetActiveUniform(shaderProgramHandle, i, 256, out _, out _, out ActiveUniformType type, out string name);
                int locationID = GL.GetUniformLocation(shaderProgramHandle, name);
                uniforms.Add(name, new ShaderUniform(name, locationID, type));
            }

            return uniforms;
        }

        public static Dictionary<string, ShaderAttribute> CreateAttributeList(int shaderProgramHandle)
        {
            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveAttributes, out int attributeCount);

            var attributes = new Dictionary<string, ShaderAttribute>();

            for (int i = 0; i < attributeCount; i++)
            {
                GL.GetActiveAttrib(shaderProgramHandle, i, 256, out _, out _, out ActiveAttribType type, out string name);
                int locationID = GL.GetAttribLocation(shaderProgramHandle, name);
                attributes.Add(name, new ShaderAttribute(name, locationID, type));
            }

            return attributes;
        }

        #endregion

        #region FileHandler

        public class Shader
        {
            public readonly string VertexShader;
            public readonly string FragmentShader;

            public Shader(string vertexShader, string fragmentShader)
            {
                VertexShader = vertexShader;
                FragmentShader = fragmentShader;
            }
        }

        public static Shader LoadShader(string fileName)
        {
            ReadFile(fileName, out string vertexShader, out string pixelShader);
            return new Shader(vertexShader, pixelShader);
        }

        private static bool ReadFile(string fileName, out string vertexShader, out string pixelShader)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                bool isVertexShaderMode = true;

                StringBuilder sbVertex = new StringBuilder();
                StringBuilder sbPixel = new StringBuilder();

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Process line
                    if (line.StartsWith("#VertexShader"))
                    {
                        isVertexShaderMode = true;
                        continue;
                    }
                    else if (line.StartsWith("#FragmentShader"))
                    {
                        isVertexShaderMode = false;
                        continue;
                    }

                    if (isVertexShaderMode)
                    {
                        sbVertex.AppendLine(line);
                    }
                    else
                    {
                        sbPixel.AppendLine(line);
                    }
                }

                vertexShader = sbVertex.ToString();

                pixelShader = sbPixel.ToString();

                if (Game._Game.IsBindlessSupported)
                {
                    pixelShader = pixelShader.Replace("#BindlessFallbackCreateUniform", "uniform sampler2D bindlessTexture;");
                    pixelShader = pixelShader.Replace("#BindlessFallbackCreateTexture", "texture(bindlessTexture, f_uv);");

                    pixelShader = pixelShader.Replace("#BindlessExtenstion", "#extension GL_ARB_bindless_texture : require");
                    pixelShader = pixelShader.Replace("#SSBOBindlessTextureArray", @"layout(std430, binding = 0) restrict readonly buffer TextureSSBO {
                                                                                            sampler2D Textures[];
                                                                                        } textureSSBO;");
                    pixelShader = pixelShader.Replace("#TextureIDToColor", @"texture(textureSSBO.Textures[int(textureID)], vec2(f_uv.x, f_uv.y));");
                }
                else
                {
                    pixelShader = pixelShader.Replace("#BindlessFallbackCreateUniform", "uniform sampler2DArray u_tex;\r\nuniform float selectedTexture;");
                    pixelShader = pixelShader.Replace("#BindlessFallbackCreateTexture", " texture(u_tex, vec3(f_uv.x, f_uv.y, selectedTexture));");

                    pixelShader = pixelShader.Replace("#BindlessExtenstion", "");
                    pixelShader = pixelShader.Replace("#SSBOBindlessTextureArray", @"");
                    pixelShader = pixelShader.Replace("#TextureIDToColor", @"texture(u_tex, vec3(f_uv.x, f_uv.y, textureID));");
                }

                if (string.IsNullOrEmpty(vertexShader) || string.IsNullOrEmpty(pixelShader))
                {
                    throw new Exception("Shader file is missing headers");
                }
            }

            return true;
        }

        #endregion

        ~ShaderProgram()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;

            GL.DeleteShader(VertexShaderHandle);
            GL.DeleteShader(PixelShaderHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(ShaderProgramHandle);

            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
