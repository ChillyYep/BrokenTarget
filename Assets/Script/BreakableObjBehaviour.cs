using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    /// <summary>
    /// 持有可破碎物体的数据
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class BreakableObjBehaviour : MonoBehaviour
    {
        public Transform parent;
        public float pieceDepth = 0.5f;
        public float areaUnit = 0.25f;
        [Range(0f, 1f)]
        public float hitStength = 0.5f;
        [Tooltip("飞行轨迹向击打方向集中的程度")]
        [Range(0f, 20f)]
        public float focusLevel = 0f;
        public float force = 10f;
        public float mass = 1f;

        [HideInInspector]
        public bool areaEffectByDistance = true;
        [HideInInspector]
        public float distance = 0.5f;
        [HideInInspector]
        public float forceEffectRangeUnit = 1f;
        [HideInInspector]
        public bool forceEffectedByDistance = true;
        [HideInInspector]
        public Vector2 boxInnerStartUV = Vector2.zero;
        [HideInInspector]
        public Vector2 boxInnerEndUV = Vector2.one;

        public Vector3 crossPointLocal
        {
            get
            {
                if (!crossPointAssigned)
                {
                    //转到物体空间
                    _crossPointLocal = transform.worldToLocalMatrix.MultiplyPoint(BrokenManager.Instance.crossPoint);
                    //排除scale影响
                    Vector3 scale = transform.localScale;
                    scale.x = scale.x == 0f ? 0f : 1f / scale.x;
                    scale.y = scale.y == 0f ? 0f : 1f / scale.y;
                    scale.z = scale.z == 0f ? 0f : 1f / scale.z;
                    _crossPointLocal.Scale(scale);
                    crossPointAssigned = true;
                }
                return _crossPointLocal;
            }
        }
        public Vector3 hitDirection
        {
            get
            {
                return BrokenManager.Instance.hitDirection;
            }
        }

        private bool crossPointAssigned = false;
        private Vector3 _crossPointLocal;
        private IBreakable breakable;
        private ICutStrategy strategy;
        private MeshFilter meshFilter;
        private MeshRenderer meshRender;
        private List<GameObject> peciesObj = new List<GameObject>();
#if UNITY_EDITOR
        [Tooltip("仅在编辑器模式下生效")]
        [SerializeField]
        private bool trailRenderOn = true;
#endif
        private void Start()
        {
            meshFilter = transform.GetComponent<MeshFilter>();
            meshRender = transform.GetComponent<MeshRenderer>();
            if (parent == null)
            {
                parent = transform.parent;
            }
            if (meshFilter)
            {

                breakable = new BreakableObj(meshFilter.mesh, transform);
                strategy = new WholeCutStrategy(breakable, new GenSmallerPieces(new GenPyramidPieces(pieceDepth, boxInnerStartUV, boxInnerEndUV), areaUnit));
            }
            else
            {
                return;
            }
        }
        public IBreakable GetBreakable()
        {
            return breakable;
        }
        public void Traversal()
        {
            switch (strategy.genPieces.GetEnumType())
            {
                case GenPiecesType.GenSmallerPieces:
                    var genPieces = strategy.genPieces as GenSmallerPieces;
                    if (areaEffectByDistance)
                    {
                        if (genPieces != null)
                        {
                            genPieces.SetCrossPoint(crossPointLocal, true, distance);
                        }
                    }
                    break;
            }
            strategy.Traversal();
        }
        public void Explode()
        {
            if (meshFilter == null)
            {
                return;
            }
            Matrix4x4 local2World = transform.localToWorldMatrix;
            Vector3 hitDirectionLocal = transform.worldToLocalMatrix.MultiplyVector(hitDirection);
            hitDirectionLocal.Normalize();
            hitDirection.Normalize();
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
                var meshRenderer = obj.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(meshRender.material);
                obj.name = string.Format("{0} Pieces {1}", gameObject.name, i);
                obj.transform.parent = parent;
                obj.transform.position = transform.position;
                obj.transform.localScale = transform.localScale;
                obj.transform.localRotation = transform.localRotation;
#if UNITY_EDITOR
                if (trailRenderOn)
                {
                    var trailRender = obj.AddComponent<TrailRenderer>();
                    trailRender.startWidth = 0.1f;
                    trailRender.endWidth = 0.1f;
                    trailRender.material = new Material(Shader.Find("Standard"));
                    trailRender.material.color = Color.black;
                }
#endif
                //物体空间下的计算
                Vector3 offset = peciesMesh[i].bounds.center - meshFilter.mesh.bounds.center;
                offset.Normalize();
                Vector3 baseDir = hitDirectionLocal * hitStength + offset * (1 - hitStength);
                //转到世界空间下的计算
                baseDir = local2World.MultiplyVector(baseDir);
                var rgBody = obj.AddComponent<Rigidbody>();
                rgBody.mass = mass;
                if (forceEffectedByDistance)
                {
                    float distance = (peciesMesh[i].bounds.center - crossPointLocal).magnitude;
                    distance = distance > forceEffectRangeUnit ? forceEffectRangeUnit / distance : 1f;
                    rgBody.AddForce((baseDir + Vector3.Dot(baseDir, hitDirection) * hitDirection * focusLevel).normalized * force * distance, ForceMode.Impulse);
                }
                else
                {
                    rgBody.AddForce((baseDir + Vector3.Dot(baseDir, hitDirection) * hitDirection * focusLevel).normalized * force, ForceMode.Impulse);
                }
                var collider = obj.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = peciesMesh[i];
                peciesObj.Add(obj);
            }
            Destroy(gameObject);
        }
    }
}
