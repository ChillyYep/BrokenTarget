using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    public interface IVistor
    {
        void Traversal(IBreakable target);
    }
    public interface IBreakable
    {
        void GetNew(ref Mesh mesh);
        void CreateNew();
        List<ModelInfo> ModelInfos { get; set; }
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
                int Count = vertices.Count;
                vertices.AddRange(modelInfo.vertices);
                normals.AddRange(modelInfo.normals);
                uvs.AddRange(modelInfo.uvs);
                //有问题
                for(int i=0;i<modelInfo.triangles.Count;++i)
                {
                    triangles.Add(modelInfo.triangles[i] + Count);
                }
            }
            newMesh.vertices = vertices.ToArray();
            //newMesh.normals = normals.ToArray();
            newMesh.triangles = triangles.ToArray();
            //newMesh.uv = uvs.ToArray();
        }
        public void GetNew(ref Mesh mesh)
        {
            if (newMesh == null)
            {
                CreateNew();
            }
            mesh = newMesh;
        }
        public void ComputeInterpolation(int triangleBeginIndex, Vector3 curPos)
        {
            Vector3 p1 = newMesh.vertices[newMesh.triangles[triangleBeginIndex]];
            Vector3 p2 = newMesh.vertices[newMesh.triangles[triangleBeginIndex + 1]];
            Vector3 p3 = newMesh.vertices[newMesh.triangles[triangleBeginIndex + 2]];
            float p2Factor = Vector3.Dot((p2 - p1).normalized, curPos - p1);
            for (int i = 0; i < 3; ++i)
            {

            }
        }
    }
}