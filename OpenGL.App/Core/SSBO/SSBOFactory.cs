using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Core.SSBO
{
    public class SSBOFactory
    {
        public static Dictionary<SSBOIndex, SSBO> SSBOs = new Dictionary<SSBOIndex, SSBO>();

        public enum SSBOIndex
        {
            BindlessTileset
        }

        public SSBOFactory()
        {
            CreateSSBOs();
        }

        private void CreateSSBOs()
        {
            //ProjectionViewMatrix
            //var ubObject = new SSBOObject(Resources.Shaders.BindlessTexture.VertexInfo, 1, "BindlessTileset", SSBOIndex.BindlessTileset, false);
            //var ssbo = new BindlessTileset(ubObject);
            //SSBOs.Add(SSBOIndex.BindlessTileset, ssbo);
        }

        public class SSBO
        {
            public SSBOObject SSBOObject { get; set; }
            public SSBO(SSBOObject uniformBufferObject)
            {
                SSBOObject = uniformBufferObject;
            }

            public virtual void GPU_Use() { }
        }

        public class BindlessTileset : SSBO
        {
            public BindlessTileset(SSBOObject uniformBufferObject) : base(uniformBufferObject)
            {

            }

            public override void GPU_Use()
            {
                //var projectionAndView = new Resources.Shaders.VertexProjectionView(Game._Game.MainCamera.GetProjectionMatrix(Game._Game.ClientSize.X, Game._Game.ClientSize.Y), Game._Game.MainCamera.GetViewMatrix());
                //UniformBufferObject.SetData(projectionAndView);
            }
        }
    }
}
