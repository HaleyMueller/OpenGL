using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Core.UniformBufferObject
{
    public class UniformBufferObjectFactory
    {
        public static Dictionary<UBOIndex, UBO> UniformBufferObjects = new Dictionary<UBOIndex, UBO>();

        public enum UBOIndex
        {
            ProjectionViewMatrix
        }

        public UniformBufferObjectFactory()
        {
            CreateUBOs();
        }

        private void CreateUBOs()
        {
            //ProjectionViewMatrix
            var ubObject = new UniformBufferObject(Resources.Shaders.VertexProjectionView.VertexInfo, 1, "ProjectionAndView", UBOIndex.ProjectionViewMatrix, false);
            var ubo = new ProjectionViewMatrix(ubObject);
            UniformBufferObjects.Add(UBOIndex.ProjectionViewMatrix, ubo);
        }

        public class UBO
        {
            public UniformBufferObject UniformBufferObject { get; set; }
            public UBO(UniformBufferObject uniformBufferObject)
            {
                UniformBufferObject = uniformBufferObject;
            }

            public virtual void GPU_Use() { }
        }

        public class ProjectionViewMatrix : UBO
        {
            public ProjectionViewMatrix(UniformBufferObject uniformBufferObject) : base(uniformBufferObject)
            {

            }

            public override void GPU_Use()
            {
                var projectionAndView = new Resources.Shaders.VertexProjectionView(Game._Game.MainCamera.GetProjectionMatrix(Game._Game.ClientSize.X, Game._Game.ClientSize.Y), Game._Game.MainCamera.GetViewMatrix());
                UniformBufferObject.SetData(projectionAndView);
            }
        }
    }
}
