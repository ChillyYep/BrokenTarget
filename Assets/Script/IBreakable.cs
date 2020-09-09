using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi2
{
    public interface IBreakable
    {
        void GetNew(ref Mesh mesh);
        void CreateNew();
        List<ModelInfo> ModelInfos { get; set; }
    }
    public class OBBBound
    {
        Vector3[] vertices = new Vector3[8];
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
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector3> normals;
        public List<Vector2> uvs;
    }
    /// <summary>
    /// 可被击碎的网格物体，增加辅助数据结构，加快算法
    /// </summary>
    public class BreakableObj : IBreakable
    {
        private Mesh oldMesh;
        private Mesh newMesh;
        public List<ModelInfo> ModelInfos { get; set; }

        public BreakableObj(Mesh mesh)
        {
            this.oldMesh = mesh;
            ModelInfos = new List<ModelInfo>();
            Init();
        }
        protected virtual void Init()
        {
            ModelInfo info = new ModelInfo();
            info.triangles = new List<int>(oldMesh.triangles);
            info.vertices = new List<Vector3>(oldMesh.vertices);
            info.normals = new List<Vector3>(oldMesh.normals);
            info.uvs = new List<Vector2>(oldMesh.uv);
            ModelInfos.Add(info);
        }
        public void CreateNew()
        {
            newMesh = new Mesh();
            List<Vector3> normals = new List<Vector3>();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            foreach (var modelInfo in ModelInfos)
            {
                vertices.AddRange(modelInfo.vertices);
                normals.AddRange(modelInfo.normals);
                triangles.AddRange(modelInfo.triangles);
                uvs.AddRange(modelInfo.uvs);
            }
            newMesh.vertices = vertices.ToArray();
            newMesh.normals = normals.ToArray();
            newMesh.triangles = triangles.ToArray();
            newMesh.uv = uvs.ToArray();
        }
        public void GetNew(ref Mesh mesh)
        {
            if (newMesh == null)
            {
                CreateNew();
            }
            mesh = newMesh;
        }
    }

}
