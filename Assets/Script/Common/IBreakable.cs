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
        BreakableGroup group { get; }
        Transform transform { get; }
        List<ModelData> ModelInfos { get; set; }
        void BeforeExplode();
        void Explode();
        int InstanceId { get; }
        void AfterExplode();
    }
    public class ModelData
    {
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector3> normals;
        public List<Vector2> uvs;
        public void AllocateStorage()
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
        public BreakableGroup group { get; private set; }
        public GameObject gameObject { get; private set; }
        public Transform transform { get; private set; }

        public List<ModelData> ModelInfos { get; set; }

        public int InstanceId { get; private set; }
        public Vector3 crossPointLocal { get; private set; }

        private Mesh sourceMesh;
        private MeshRenderer sourceMeshRenderer;
        private List<GameObject> peciesObj = new List<GameObject>();
        private ICutStrategy strategy;
        private BreakableObj()
        {
        }
        public BreakableObj(BreakableGroup group, GameObject gameObject)
        {
            this.group = group;
            this.gameObject = gameObject;
            transform = gameObject.transform;
            sourceMesh = transform.GetComponent<MeshFilter>().mesh;
            sourceMeshRenderer = transform.GetComponent<MeshRenderer>();
            strategy = new WholeCutStrategy(this, new GenSmallerPieces(new GenPyramidPieces(group.pieceDepth, group.boxInnerStartUV, group.boxInnerEndUV, group.newVertexRandom), group.areaUnit, group.cutType));
            ModelInfos = new List<ModelData>();
            Init();
            InstanceId = gameObject.GetInstanceID();
        }
        protected void Init()
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
                    if (genPieces != null)
                    {
                        crossPointLocal = transform.worldToLocalMatrix.MultiplyPoint(BrokenManager.Instance.crossPoint);
                        genPieces.SetCrossPoint(crossPointLocal, group.areaEffectByDistance, group.distance, group.limitAreaUnit, group.maxAreaUnit, group.minAreaUnit);
                    }
                    break;
            }
            strategy.Traversal();
        }
        private void ExplodePieces()
        {
            Matrix4x4 local2World = transform.localToWorldMatrix;
            Vector3 hitDirectionLocal = transform.worldToLocalMatrix.MultiplyVector(group.hitDirection);
            hitDirectionLocal.Normalize();
            group.hitDirection.Normalize();
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
                obj.name = string.Format("{0} Pieces {1}", gameObject.name, i);
                obj.transform.parent = group.transform;
                obj.transform.position = transform.position;
                obj.transform.localScale = transform.localScale;
                obj.transform.localRotation = transform.localRotation;
#if UNITY_EDITOR
                if (group.IsTrailRenderOn())
                {
                    var trailRender = obj.AddComponent<TrailRenderer>();
                    trailRender.startWidth = 0.1f;
                    trailRender.endWidth = 0.1f;
                    trailRender.material = new Material(Shader.Find("Standard"));
                    trailRender.material.color = Color.black;
                }
#endif
                peciesObj.Add(obj);
            }
            ApplyForce();
            GameObject.Destroy(gameObject);
        }
        private void ApplyForce()
        {
            for(int i=0;i<peciesObj.Count;++i)
            {
                //转到世界空间下的计算
                Mesh peiceMesh = peciesObj[i].GetComponent<MeshFilter>().mesh;
                Vector3 offset = peiceMesh.bounds.center - sourceMesh.bounds.center;
                offset = transform.localToWorldMatrix.MultiplyVector(offset);
                offset.Normalize();
                Vector3 baseDir = group.hitDirection * group.hitStength + offset;
                baseDir.Normalize();
                Vector3 focusDir = Vector3.Dot(baseDir, group.hitDirection) * group.hitDirection - baseDir;
                var rgBody = peciesObj[i].AddComponent<Rigidbody>();
                rgBody.mass = group.mass;
                rgBody.interpolation = RigidbodyInterpolation.Interpolate;
                if (group.forceEffectedByDistance)
                {
                    float distance = (peiceMesh.bounds.center - crossPointLocal).magnitude;
                    distance = distance > group.forceEffectRangeUnit ? group.forceEffectRangeUnit / distance : 1f;
                    var force = (baseDir + focusDir * group.focusLevel).normalized * group.force * distance;
                    rgBody.AddForce(force, ForceMode.Impulse);
                }
                else
                {
                    var force = (baseDir + focusDir * group.focusLevel).normalized * group.force;
                    rgBody.AddForce(force, ForceMode.Impulse);
                }

                var collider = peciesObj[i].AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = peiceMesh;
            }
        }
        public void Explode()
        {
            GenPeicesDatas();
            ExplodePieces();
        }
    }
}