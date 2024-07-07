using OpenGL.App.Core.Shader;
using OpenGL.App.Core.Vertex;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.GameObjects
{
    public class PlaneWithImage : GameObject
    {
        public PlaneWithImage(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, ShaderProgram shaderFile, VertexBuffer[] vertexArray, int[] indices) : base(position, scale, rotation, projectionType, shaderFile, vertexArray, indices)
        {

        }

        public PlaneWithImage(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, string shaderFile, VertexBuffer[] vertexArray, int[] indices) : base(position, scale, rotation, projectionType, shaderFile, vertexArray, indices)
        {

        }

        public float colorFactor = 1f;
        public bool flipColor = false;

        Resources.Shaders.VertexColor[] verticesColor = new Resources.Shaders.VertexColor[]
        {
            new Resources.Shaders.VertexColor(new Color4(1f, 0f, 0f, 1f)),
            new Resources.Shaders.VertexColor(new Color4(0f, 1f, 0f, 1f)),
            new Resources.Shaders.VertexColor(new Color4(0f, 0f, 1f, 1f)),
            new Resources.Shaders.VertexColor(new Color4(1f, 1f, 1f, 1f))
        };

        internal override void GPU_Use_Shader()
        {
            GetShaderProgram().SetUniform("model", ModelView);
            base.GPU_Use_Shader();
        }

        public void Update(FrameEventArgs args)
        {
            if (colorFactor >= 1f)
            {
                flipColor = false;
            }
            else if (colorFactor <= 0f)
            {
                flipColor = true;
            }

            if (flipColor)
            {
                colorFactor += (float)(0.4f * args.Time);
            }
            else
            {
                colorFactor -= (float)(0.4f * args.Time);
            }

            //Console.WriteLine($"ColorFactor:{colorFactor}");

            var copiedVertColor = new Resources.Shaders.VertexColor[verticesColor.Length];
            Array.Copy(verticesColor, copiedVertColor, verticesColor.Length);

            for (int i = 0; i < copiedVertColor.Length; i++)
            {
                copiedVertColor[i].Color.R *= colorFactor;
                copiedVertColor[i].Color.G *= colorFactor;
                copiedVertColor[i].Color.B *= colorFactor;
            }

            //base.VertexArray.VertexBuffer["Color"].SetData(copiedVertColor, copiedVertColor.Length);
        }
    }

}
