using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    /// <summary>
    /// 可破碎对象
    /// </summary>
    public interface IBreakable
    {
        BreakableObjBehaviour behaviour { get; }
        Transform transform { get; }
        List<ModelData> ModelInfos { get; set; }
        void BeforeExplode();
        void Explode();
        void AfterExplode();
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
        private Mesh sourceMesh;
        private MeshRenderer sourceMeshRenderer;
        public BreakableObjBehaviour behaviour { get; private set; }
        public Transform transform { get; private set; }

        public List<ModelData> ModelInfos { get; set; }

        private List<GameObject> peciesObj = new List<GameObject>();
        private ICutStrategy strategy;
        public BreakableObj(BreakableObjBehaviour behaviour)
        {
            this.behaviour = behaviour;
            this.transform = behaviour.transform;
            this.sourceMesh = behaviour.transform.GetComponent<MeshFilter>().mesh;
            this.sourceMeshRenderer = behaviour.transform.GetComponent<MeshRenderer>();
            this.transform = transform;
            strategy = new WholeCutStrategy(this, new GenSmallerPieces(new GenPyramidPieces(behaviour.pieceDepth, behaviour.boxInnerStartUV, behaviour.boxInnerEndUV), behaviour.areaUnit));
            ModelInfos = new List<ModelData>();
            Init();
        }
        protected virtual void Init()
        {
            ModelData info = new ModelData();
            info.triangles = new List<int>(sourceMesh.triangles);
            info.vertices = new List<Vector3>(sourceMesh.vertices);
            info.normals = new List<Vector3>(sourceMesh.normals);
            info.uvs = new List<Vector2>(sourceMesh.uv);
            ModelInfos.Add(info);
        }
        public void BeforeExplode()
        {
            //爆炸前的一些处理
        }
        public void AfterExplode()
        {
            //爆炸后的一些处理
        }
        private void GenPeicesDatas()
        {
            switch (strategy.genPieces.GetEnumType())
            {
                case GenPiecesType.GenSmallerPieces:
                    var genPieces = strategy.genPieces as GenSmallerPieces;
                    if (behaviour.areaEffectByDistance)
                    {
                        if (genPieces != null)
                        {
                            genPieces.SetCrossPoint(behaviour.crossPointLocal, true, behaviour.distance);
                        }
                    }
                    break;
            }
            strategy.Traversal();
        }
        private void ExplodePieces()
        {
            Matrix4x4 local2World = transform.localToWorldMatrix;
            Vector3 hitDirectionLocal = transform.worldToLocalMatrix.MultiplyVector(behaviour.hitDirection);
            hitDirectionLocal.Normalize();
            behaviour.hitDirection.Normalize();
            List<Mesh> peciesMesh = new List<Mesh>();
            peciesObj.Clear();
            foreach (var item in strategy.pecies)
            {
                peciesMesh.Add(new Mesh()
                {
                    vertices = item.vertices.ToArray(),
                    triangles = item.triangles.ToArray(),
                    normals = item.normals.ToArray(),
                    uv = item.uvs.ToArray()
                });
            }
            for (int i = 0; i < peciesMesh.Count; ++i)
            {
                GameObject obj = new GameObject();
                obj.AddComponent<MeshFilter>().mesh = peciesMesh[i];
                obj.AddComponent<MeshRenderer>().material = new Material(sourceMeshRenderer.material);
                obj.name = string.Format("{0} Pieces {1}", behaviour.gameObject.name, i);
                obj.transform.parent = behaviour.transform.parent;
                obj.transform.position = transform.position;
                obj.transform.localScale = transform.localScale;
                obj.transform.localRotation = transform.localRotation;
#if UNITY_EDITOR
                if (behaviour.IsTrailRenderOn())
                {
                    var trailRender = obj.AddComponent<TrailRenderer>();
                    trailRender.startWidth = 0.1f;
                    trailRender.endWidth = 0.1f;
                    trailRender.material = new Material(Shader.Find("Standard"));
                    trailRender.material.color = Color.black;
                }
#endif
                //物体空间下的计算
                Vector3 offset = peciesMesh[i].bounds.center - sourceMesh.bounds.center;
                offset.Normalize();
                Vector3 baseDir = hitDirectionLocal * behaviour.hitStength + offset * (1 - behaviour.hitStength);
                //转到世界空间下的计算
                baseDir = local2World.MultiplyVector(baseDir);
                var rgBody = obj.AddComponent<Rigidbody>();
                rgBody.mass = behaviour.mass;
                if (behaviour.forceEffectedByDistance)
                {
                    float distance = (peciesMesh[i].bounds.center - behaviour.crossPointLocal).magnitude;
                    distance = distance > behaviour.forceEffectRangeUnit ? behaviour.forceEffectRangeUnit / distance : 1f;
                    rgBody.AddForce((baseDir + Vector3.Dot(baseDir, behaviour.hitDirection) * behaviour.hitDirection * behaviour.focusLevel).normalized * behaviour.force * distance, ForceMode.Impulse);
                }
                else
                {
                    rgBody.AddForce((baseDir + Vector3.Dot(baseDir, behaviour.hitDirection) * behaviour.hitDirection * behaviour.focusLevel).normalized * behaviour.force, ForceMode.Impulse);
                }
                var collider = obj.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = peciesMesh[i];
                peciesObj.Add(obj);
            }
            GameObject.Destroy(behaviour.gameObject);
        }
        public void Explode()
        {
            GenPeicesDatas();
            ExplodePieces();
        }
        //public void CreateMesh()
        //{
        //    newMesh = new Mesh();
        //    List<Vector3> normals = new List<Vector3>();
        //    List<Vector3> vertices = new List<Vector3>();
        //    List<int> triangles = new List<int>();
        //    List<Vector2> uvs = new List<Vector2>();
        //    foreach (var modelInfo in ModelInfos)
        //    {
        //        int Count = vertices.Count;
        //        vertices.AddRange(modelInfo.vertices);
        //        normals.AddRange(modelInfo.normals);
        //        uvs.AddRange(modelInfo.uvs);
        //        for (int i = 0; i < modelInfo.triangles.Count; ++i)
        //        {
        //            triangles.Add(modelInfo.triangles[i] + Count);
        //        }
        //    }
        //    newMesh.vertices = vertices.ToArray();
        //    //newMesh.normals = normals.ToArray();
        //    newMesh.triangles = triangles.ToArray();
        //    //newMesh.uv = uvs.ToArray();
        //}
    }
}