using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpFont.PostScript;

namespace OpenGL.App
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

        public readonly int ShaderProgramHandle;
        public readonly int VertexShaderHandle;
        public readonly int PixelShaderHandle;

        private readonly ShaderUniform[] Uniforms;
        private readonly ShaderAttribute[] Attributes;

        public ShaderProgram(string fileName) 
        {
            this.disposed = false;

            var shader = LoadShader(fileName);

            if (CompileVertexShader(shader.VertexShader, out this.VertexShaderHandle, out string vertexShaderCompileError) == false) //If errored
            {
                throw new ArgumentException(vertexShaderCompileError);
            }

            if (CompilePixelShader(shader.FragmentShader, out this.PixelShaderHandle, out string pixelShaderCompileError) == false) //If errored
            {
                throw new ArgumentException(pixelShaderCompileError);
            }

            this.ShaderProgramHandle = CreateLinkProgram(VertexShaderHandle, PixelShaderHandle);

            this.Uniforms = CreateUniformList(this.ShaderProgramHandle);
            this.Attributes = CreateAttributeList(this.ShaderProgramHandle);
        }

        public void Use()
        {
            GL.UseProgram(ShaderProgramHandle);
        }

        #region OpenGL
        public ShaderUniform[] GetUniformList()
        {
            ShaderUniform[] result = new ShaderUniform[this.Uniforms.Length];
            Array.Copy(this.Uniforms, result, this.Uniforms.Length);
            return result;
        }

        public ShaderAttribute[] GetAttributeList()
        {
            ShaderAttribute[] result = new ShaderAttribute[this.Attributes.Length];
            Array.Copy(this.Attributes, result, this.Attributes.Length);
            return result;
        }


        public void SetUniform(string name, float v1)
        {
            if (GetShaderUniform(name, out ShaderUniform uniform) == false)
            {
                throw new ArgumentException($"Shader uniform name was not found {name}");
            }

            if (uniform.Type != ActiveUniformType.Float)
            {
                throw new ArgumentException("Shader uniform type is not float");
            }

            GL.UseProgram(this.ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.Uniform1(uniform.Location, v1); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        public void SetUniform(string name, float v1, float v2)
        {
            if (GetShaderUniform(name, out ShaderUniform uniform) == false)
            {
                throw new ArgumentException($"Shader uniform name was not found {name}");
            }

            if (uniform.Type != ActiveUniformType.FloatVec2)
            {
                throw new ArgumentException("Shader uniform type is not float vec 2");
            }

            GL.UseProgram(this.ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.Uniform2(uniform.Location, v1, v2); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        public void SetUniform(string name, float v1, float v2, float v3)
        {
            if (GetShaderUniform(name, out ShaderUniform uniform) == false)
            {
                throw new ArgumentException($"Shader uniform name was not found {name}");
            }

            if (uniform.Type != ActiveUniformType.FloatVec3)
            {
                throw new ArgumentException("Shader uniform type is not float vec 2");
            }

            GL.UseProgram(this.ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.Uniform3(uniform.Location, v1, v2, v3); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        public void SetUniform(string name, float v1, float v2, float v3, float v4)
        {
            if (GetShaderUniform(name, out ShaderUniform uniform) == false)
            {
                throw new ArgumentException($"Shader uniform name was not found {name}");
            }

            if (uniform.Type != ActiveUniformType.FloatVec4)
            {
                throw new ArgumentException("Shader uniform type is not float vec 2");
            }

            GL.UseProgram(this.ShaderProgramHandle); //Tell open gl what program we going to send array to
            GL.Uniform4(uniform.Location, v1, v2, v3, v4); //Set the location variable on the shader code with the value
            GL.UseProgram(0); //Clear
        }

        private bool GetShaderUniform(string name, out ShaderUniform shaderUniform)
        {
            shaderUniform = new ShaderUniform();

            for (int i = 0; i < this.Uniforms.Length; i++)
            {
                shaderUniform = this.Uniforms[i];

                if (name == shaderUniform.Name)
                {
                    return true;
                }
            }
            
            return false;
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

        public static ShaderUniform[] CreateUniformList(int shaderProgramHandle)
        {
            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveUniforms, out int uniformCount);

            ShaderUniform[] uniforms = new ShaderUniform[uniformCount];

            for (int i = 0; i < uniformCount; i++)
            {
                GL.GetActiveUniform(shaderProgramHandle, i, 256, out _, out _, out ActiveUniformType type, out string name);
                int locationID = GL.GetUniformLocation(shaderProgramHandle, name);
                uniforms[i] = new ShaderUniform(name, locationID, type);
            }

            return uniforms;
        }

        public static ShaderAttribute[] CreateAttributeList(int shaderProgramHandle)
        {
            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveAttributes, out int attributeCount);

            ShaderAttribute[] attributes = new ShaderAttribute[attributeCount];

            for (int i = 0; i < attributeCount; i++)
            {
                GL.GetActiveAttrib(shaderProgramHandle, i, 256, out _, out _, out ActiveAttribType type, out string name);
                int locationID = GL.GetAttribLocation(shaderProgramHandle, name);
                attributes[i] = new ShaderAttribute(name, locationID, type);
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

                String line;
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
            if (this.disposed) return;

            GL.DeleteShader(this.VertexShaderHandle);
            GL.DeleteShader(this.PixelShaderHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(this.ShaderProgramHandle);

            this.disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
