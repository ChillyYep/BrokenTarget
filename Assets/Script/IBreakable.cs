using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi2
{
    public interface IBreakable
    {
        List<ModelInfo> ModelInfos { get; set; }
    }
    public class OBBBound
    {
        Vector3[] vertices = new Vector3[8];
        Vector3 center;
        public void ComputeOBBBound(Mesh mesh)
        {
            //目前都是立方体
            for(int i=0;i<8;++i)
            {
                vertices[i] = mesh.vertices[i];
            }
        }
    }
    public class ModelInfo
    {
        public OBBBound bound;
        public Mesh mesh;
    }
    /// <summary>
    /// 可被击碎的网格物体，增加辅助数据结构，加快算法
    /// </summary>
    public class BreakableObj : IBreakable
    {
        public List<ModelInfo> ModelInfos { get; set; }

        public BreakableObj(List<GameObject> cells)
        {
            ModelInfos = new List<ModelInfo>();
            foreach(var modelInfo in cells)
            {
                var meshFilter = modelInfo.GetComponent<MeshFilter>();
                if(meshFilter != null)
                {
                    OBBBound bound = new OBBBound();
                    bound.ComputeOBBBound(meshFilter.mesh);
                    ModelInfos.Add(new ModelInfo()
                    {
                        bound = bound,
                        mesh = meshFilter.mesh
                    });
                }
            }
        }
    }

}
