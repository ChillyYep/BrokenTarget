using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi2
{
    public interface IVistor
    {
        void Traversal(IBreakable target);
    }
    public interface IBreakable
    {
        List<ModelInfo> ModelInfos { get; set; }
        void AddExplodeQuery(GameObject gameObject, Vector3 direction);
        void Explode(float power, Vector3 fromDirection);
    }
    public class OBBBound
    {
        Vector3[] vertices = new Vector3[8];
        public Vector3 center { get; set; }
        public void ComputeOBBBound(GameObject gameObject)
        {
            center = gameObject.transform.position;
            //center = mesh.bounds.center;
            //mesh.bounds.size
            ////目前都是立方体
            for (int i = 0; i < 8; ++i)
            {
                //vertices[i] = mesh.vertices[i];
            }
        }
    }
    public class ModelInfo
    {
        public OBBBound bound;
        public GameObject gameObject;
        public Mesh mesh;
    }
    /// <summary>
    /// 可被击碎的网格物体，增加辅助数据结构，加快算法
    /// </summary>
    public class BreakableObj : IBreakable
    {
        public List<ModelInfo> ModelInfos { get; set; }
        private List<GameObject> cells;
        private List<GameObject> explodeQuery = new List<GameObject>();
        private List<Vector3> explodeDirQuery = new List<Vector3>();

        public BreakableObj(List<GameObject> cells)
        {
            this.cells = cells;
            ModelInfos = new List<ModelInfo>();
            foreach (var model in cells)
            {
                var meshFilter = model.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    OBBBound bound = new OBBBound();
                    bound.ComputeOBBBound(model);
                    ModelInfos.Add(new ModelInfo()
                    {
                        bound = bound,
                        gameObject = model,
                        mesh = meshFilter.mesh
                    });
                }
            }
        }
        public void AddExplodeQuery(GameObject gameObject, Vector3 direction)
        {
            explodeQuery.Add(gameObject);
            explodeDirQuery.Add(direction);
        }
        public void Explode(float power, Vector3 fromDirection)
        {
            for (int i = 0; i < explodeQuery.Count; ++i)
            {
                var rgbody = explodeQuery[i].GetComponent<Rigidbody>() == null ? explodeQuery[i].AddComponent<Rigidbody>() : explodeQuery[i].GetComponent<Rigidbody>();
                rgbody.AddForce(Vector3.Reflect(fromDirection,explodeDirQuery[i]).normalized * power);
            }
            explodeQuery.Clear();
            explodeDirQuery.Clear();
        }
    }

}
