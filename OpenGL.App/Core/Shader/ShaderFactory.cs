using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Core.Shader
{
    public class ShaderFactory : IDisposable
    {
        public static Dictionary<string, ShaderProgram> ShaderPrograms = new Dictionary<string, ShaderProgram>();

        public const string ShaderPath = "Resources/Shaders/";

        public ShaderFactory() 
        {
            Console.WriteLine("Loading shaders...");
            LoadAllShaders(ShaderPath);
        }

        private void LoadAllShaders(string shaderFolder)
        {
            var files = Directory.GetFiles(shaderFolder);

            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Extension == ".glsl")
                    LoadShader(fileInfo);
            }
        }

        private void LoadShader(FileInfo fileInfo)
        {
            //Create shader program and link to public dict
            var shaderProgram = new ShaderProgram(fileInfo.FullName);

            ShaderPrograms.Add(fileInfo.Name, shaderProgram);

            Console.WriteLine($"Loaded shader: {fileInfo.Name}");
        }

        public void Dispose()
        {
            for (int i = 0; i < ShaderPrograms.Count; i++) 
            {
                var shaderProgram = ShaderPrograms.Values.ElementAt(i);
                shaderProgram?.Dispose();
            }
        }
    }
}
