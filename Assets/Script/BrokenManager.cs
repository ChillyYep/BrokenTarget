using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrokenSys
{
    public class BrokenManager : MonoBehaviour
    {
        public Vector3 crossPoint { get; private set; }
        public Vector3 crossNormal { get; private set; }
        public Vector3 hitDirection { get; private set; }
        GameObject crossTarget;
        public Transform gizmosParent;
        private List<GameObject> gizmos = new List<GameObject>();
        public static BrokenManager Instance { get; private set; }
        public float detectionRadius = 2f;
        public bool multiple = false;
        public List<BreakableGroup> breakableGroups = new List<BreakableGroup>();
        private DetectionSphere detection;
        private bool catched = false;
        private void Awake()
        {
            detection = new DetectionSphere(Vector3.zero, 2f, SegmentVerticalType.Eight, SegmentCircleType.Eighteen);
            if (Instance == null)
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }
        private void CatchSelectTarget()
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit) && Input.GetMouseButtonDown(0))
            {
                crossPoint = raycastHit.point;
                crossNormal = raycastHit.normal;
                hitDirection = ray.direction;
                crossTarget = raycastHit.collider.gameObject;
                catched = true;
            }
            else
            {
                catched = false;
            }
        }
        public void EmitRay(Vector3 origin, Vector3 direction)
        {
            RaycastHit raycastHit;
            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out raycastHit) && Input.GetMouseButtonDown(0))
            {
                crossPoint = raycastHit.point;
                crossNormal = raycastHit.normal;
                hitDirection = ray.direction;
                crossTarget = raycastHit.collider.gameObject;
                catched = true;
            }
            else
            {
                catched = false;
            }
        }
        public void EmitRay(Ray ray)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit) && Input.GetMouseButtonDown(0))
            {
                crossPoint = raycastHit.point;
                crossNormal = raycastHit.normal;
                hitDirection = ray.direction;
                crossTarget = raycastHit.collider.gameObject;
                catched = true;
            }
            else
            {
                catched = false;
            }
        }
        public void ClearGizmos()
        {
            if (Input.GetMouseButtonUp(0))
            {
                for (int i = 0; i < gizmos.Count; ++i)
                {
                    if (gizmos[i] != null)
                    {
                        gizmos[i].SetActive(false);
                    }
                }
            }
        }
        public void StartOver()
        {
            enabled = true;
        }
        public void Stop()
        {
            enabled = false;
        }
        public int GetBreakableGroupIndex(GameObject target)
        {
            for (int i = 0; i < breakableGroups.Count; ++i)
            {
                if (breakableGroups[i].breakableObjDict == null)
                {
                    continue;
                }
                if (breakableGroups[i].breakableObjDict.ContainsKey(target.GetInstanceID()))
                {
                    return i;
                }
            }
            return -1;
        }
        void DrawSphereGimos()
        {
            if (gizmos.Count == 0)
            {
                detection.DrawMesh();
                GameObject sphereObj = new GameObject();
                var meshFilter = sphereObj.AddComponent<MeshFilter>();
                meshFilter.mesh = detection.SphereMesh;
                meshFilter.transform.position = crossPoint;
                meshFilter.transform.parent = gizmosParent;
                var meshRenderer = sphereObj.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Standard"));
                meshRenderer.material.color = Color.red;
                gizmos.Add(sphereObj);
            }
            else
            {
                gizmos[0].transform.position = crossPoint;
                gizmos[0].SetActive(true);
            }
        }
        void Update()
        {
            ClearGizmos();
#if UNITY_EDITOR
            CatchSelectTarget();
#endif
            if (catched)
            {
                catched = false;
                int groupIndex = GetBreakableGroupIndex(crossTarget);
                if (groupIndex != -1)
                {
                    //DrawSphereGimos();
                    if (multiple)
                    {
                        breakableGroups[groupIndex].ExplodeAllImmediately();
                    }
                    else
                    {
                        breakableGroups[groupIndex].ExplodeOneImmediately(crossTarget);
                    }
                }
            }
        }
    }
}