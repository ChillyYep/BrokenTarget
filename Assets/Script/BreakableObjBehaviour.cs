using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//namespace chenyi
//{
//    /// <summary>
//    /// 持有可破碎物体的数据
//    /// </summary>
//    [RequireComponent(typeof(MeshFilter))]
//    [RequireComponent(typeof(MeshRenderer))]
//    public class BreakableObjBehaviour : MonoBehaviour
//    {
//        public float pieceDepth = 0.5f;
//        public float areaUnit = 0.25f;
//        [Range(0f, 1f)]
//        public float hitStength = 0.5f;
//        [Tooltip("飞行轨迹向击打方向集中的程度")]
//        [Range(0f, 20f)]
//        public float focusLevel = 0f;
//        public float force = 10f;
//        public float mass = 1f;

//        [HideInInspector]
//        public bool areaEffectByDistance = true;
//        [HideInInspector]
//        public float distance = 0.5f;
//        [HideInInspector]
//        public float forceEffectRangeUnit = 1f;
//        [HideInInspector]
//        public bool forceEffectedByDistance = true;
//        [HideInInspector]
//        public Vector2 boxInnerStartUV = Vector2.zero;
//        [HideInInspector]
//        public Vector2 boxInnerEndUV = Vector2.one;
//        [HideInInspector]
//        public CutToPiecesPerTime cutType = CutToPiecesPerTime.All;
//        [Tooltip("爆炸行为脚本开关状态")]
//        public bool swichOn = true;
//        public bool newVertexRandom = true;
//        public BreakableGroup group { get; set; }
//        private static int instanceId = 0;
//        public int InstacneId { get; private set; }
//        public Vector3 crossPointLocal
//        {
//            get
//            {
//                if (!crossPointAssigned)
//                {
//                    //转到物体空间
//                    _crossPointLocal = transform.worldToLocalMatrix.MultiplyPoint(BrokenManager.Instance.crossPoint);
//                    //排除scale影响
//                    Vector3 scale = transform.localScale;
//                    scale.x = scale.x == 0f ? 0f : 1f / scale.x;
//                    scale.y = scale.y == 0f ? 0f : 1f / scale.y;
//                    scale.z = scale.z == 0f ? 0f : 1f / scale.z;
//                    _crossPointLocal.Scale(scale);
//                    crossPointAssigned = true;
//                }
//                return _crossPointLocal;
//            }
//        }
//        public Vector3 hitDirection
//        {
//            get
//            {
//                return BrokenManager.Instance.hitDirection;
//            }
//        }

//        private bool crossPointAssigned = false;
//        private Vector3 _crossPointLocal;
//        private IBreakable breakable;
//        private List<IBreakable> breakables = new List<IBreakable>();
//#if UNITY_EDITOR
//        [Tooltip("仅在编辑器模式下生效")]
//        [SerializeField]
//        private bool trailRenderOn = true;
//#endif
//        private void Awake()
//        {
//            ++instanceId;
//            InstacneId = instanceId;
//            if (transform.GetComponentInParent<BreakableGroup>() == null)
//            {
//                Debug.LogWarning("挂载了BreakableObjBehaviour组件的游戏对象，其祖先游戏对象没有BreakableGroup组件，可能出现预期以外的情况");
//                //enabled = false;
//            }
//        }
//        private void Start()
//        {
//            breakable = new BreakableObj(this);
//            for(int i=0;i<transform.childCount;++i)
//            {
//                breakables.Add(new BreakableObj(this));
//            }
//        }
//        public bool IsTrailRenderOn()
//        {
//#if UNITY_EDITOR
//            return trailRenderOn;
//#else
//            return false;
//#endif
//        }
//        public void ExplodeImmediately()
//        {
//            if (swichOn)
//            {
//                breakable.BeforeExplode();
//                breakable.Explode();
//                breakable.AfterExplode();
//                group.AddToExpireList(this);
//            }
//        }
//        [ContextMenu("Same Property In Same Group")]
//        public void BreakableBehaviourClone()
//        {
//            foreach (var behaviour in transform.parent.GetComponentsInChildren<BreakableObjBehaviour>())
//            {
//                behaviour.pieceDepth = pieceDepth;
//                behaviour.areaUnit = areaUnit;
//                behaviour.hitStength = hitStength;
//                behaviour.focusLevel = focusLevel;
//                behaviour.force = force;
//                behaviour.mass = mass;
//                behaviour.areaEffectByDistance = areaEffectByDistance;
//                behaviour.distance = distance;
//                behaviour.forceEffectRangeUnit = forceEffectRangeUnit;
//                behaviour.forceEffectedByDistance = forceEffectedByDistance;
//                behaviour.boxInnerStartUV = boxInnerStartUV;
//                behaviour.boxInnerEndUV = boxInnerEndUV;
//                behaviour.cutType = cutType;
//                behaviour.swichOn = swichOn;
//                behaviour.newVertexRandom = newVertexRandom;
//            }
//        }
//    }
//}
