using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    public class BreakableGroup : MonoBehaviour
    {
        public float pieceDepth = 0.5f;
        public float areaUnit = 0.25f;
        [Range(0f, 1f)]
        public float hitStength = 0.5f;
        [Tooltip("飞行轨迹向击打方向集中的程度")]
        [Range(0f, 1f)]
        public float focusLevel = 0f;
        public float force = 10f;
        public float mass = 1f;
        [HideInInspector]
        public bool areaEffectByDistance = true;
        [HideInInspector]
        public float distance = 0.5f;
        [HideInInspector]
        public bool limitAreaUnit = false;
        [HideInInspector]
        public float maxAreaUnit = 0.25f;
        [HideInInspector]
        public float minAreaUnit = 0.25f;
        [HideInInspector]
        public float forceEffectRangeUnit = 1f;
        [HideInInspector]
        public bool forceEffectedByDistance = true;
        [HideInInspector]
        public Vector2 boxInnerStartUV = Vector2.zero;
        [HideInInspector]
        public Vector2 boxInnerEndUV = Vector2.one;
        [HideInInspector]
        public CutToPiecesPerTime cutType = CutToPiecesPerTime.All;
        [Tooltip("爆炸行为脚本开关状态")]
        public bool swichOn = true;
        public bool newVertexRandom = true;
        public Vector3 hitDirection
        {
            get
            {
                return BrokenManager.Instance.hitDirection;
            }
        }
        [Tooltip("注：在ContextMenu上有一个选项自动添加对象的方法")]
        public List<GameObject> breakableObjList = new List<GameObject>();
#if UNITY_EDITOR
        [Tooltip("仅在编辑器模式下生效")]
        [SerializeField]
        private bool trailRenderOn = true;
#endif
        public Dictionary<int, IBreakable> breakableObjDict { get; private set; }
        private List<IBreakable> expireList = new List<IBreakable>();

        private void Awake()
        {
            breakableObjDict = new Dictionary<int, IBreakable>();
            for (int i = 0; i < breakableObjList.Count; ++i)
            {
                if (breakableObjList[i].GetComponent<MeshRenderer>() != null)
                {
                    AddMember(new BreakableObj(this, breakableObjList[i]));
                }
            }
        }
        public void ExplodeAllImmediately()
        {
            if (swichOn)
            {
                foreach (var breakable in breakableObjDict.Values)
                {
                    Vector3 crossPointLocal = breakable.transform.worldToLocalMatrix.MultiplyPoint(BrokenManager.Instance.crossPoint);
                    Vector3 vec3 = breakable.transform.GetComponent<MeshFilter>().mesh.bounds.ClosestPoint(crossPointLocal);
                    if (DetectionSphere.IsInSphere(vec3, crossPointLocal, BrokenManager.Instance.detectionRadius))
                    {
                        breakable.BeforeExplode();
                        breakable.Explode();
                        breakable.AfterExplode();
                        AddToExpireList(breakable);
                    }
                }
                RemoveExpiredMembers();
            }
        }
        public void ExplodeOneImmediately(GameObject crossTarget)
        {
            if (swichOn)
            {
                var breakable = breakableObjDict[crossTarget.GetInstanceID()];
                breakable.BeforeExplode();
                breakable.Explode();
                breakable.AfterExplode();
                AddToExpireList(breakable);
                breakableObjDict.Remove(breakable.InstanceId);
            }
        }
        private void AddMember(IBreakable breakable)
        {
            breakableObjDict[breakable.InstanceId] = breakable;
        }
        private void AddToExpireList(IBreakable breakable)
        {
            expireList.Add(breakable);
        }
        private void RemoveExpiredMembers()
        {
            foreach (var breakable in expireList)
            {
                breakableObjDict.Remove(breakable.InstanceId);
            }
            expireList.Clear();
        }
        public bool IsTrailRenderOn()
        {
#if UNITY_EDITOR
            return trailRenderOn;
#else
            return false;
#endif
        }
        [ContextMenu("Add All MeshRenderer Children To BreakableObjList")]
        public void AddAllChildrenToBreakableObjList()
        {
            breakableObjList.Clear();
            foreach (var item in transform.GetComponentsInChildren<MeshRenderer>())
            {
                breakableObjList.Add(item.gameObject);
            }

        }
    }

}
