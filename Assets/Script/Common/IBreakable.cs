using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    /// <summary>
    /// 可破碎对象
    /// </summary>
    public interface IBreakable: IConvertable
    {
        Transform transform { get; }
        void GetMesh(ref Mesh mesh);
        void CreateMesh();
        List<ModelData> ModelInfos { get; set; }
    }
    public class ModelData
    {
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector3> normals;
        public List<Vector2> uvs;
        public void Init()
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();
            normals = new List<Vector3>();
        }
    }
    /// <summary>
    /// 可被击碎的网格物体，增加辅助数据结构，加快算法
    /// </summary>
    public class BreakableObj : IBreakable
    {
        private Mesh oldMesh;
        private Mesh newMesh;
        public Transform transform { get; private set; }

        public List<ModelData> ModelInfos { get; set; }
        public Matrix4x4 World2Object { get; private set; }
        public Matrix4x4 Object2World { get; private set; }

        public BreakableObj(Mesh mesh, Transform transform)
        {
            this.oldMesh = mesh;
            this.transform = transform;
            World2Object = transform.worldToLocalMatrix;
            Object2World = transform.localToWorldMatrix;
            ModelInfos = new List<ModelData>();
            Init();
        }
        protected virtual void Init()
        {
            ModelData info = new ModelData();
            info.triangles = new List<int>(oldMesh.triangles);
            info.vertices = new List<Vector3>(oldMesh.vertices);
            info.normals = new List<Vector3>(oldMesh.normals);
            info.uvs = new List<Vector2>(oldMesh.uv);
            ModelInfos.Add(info);
        }
        public void CreateMesh()
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
                for (int i = 0; i < modelInfo.triangles.Count; ++i)
                {
                    triangles.Add(modelInfo.triangles[i] + Count);
                }
            }
            newMesh.vertices = vertices.ToArray();
            //newMesh.normals = normals.ToArray();
            newMesh.triangles = triangles.ToArray();
            //newMesh.uv = uvs.ToArray();
        }
        public void GetMesh(ref Mesh mesh)
        {
            if (newMesh == null)
            {
                CreateMesh();
            }
            mesh = newMesh;
        }
    }
}