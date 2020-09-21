using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    //public enum CutType
    //{
    //    PartCut,
    //    TotalCut
    //}
    /// <summary>
    /// 持有可破碎物体的数据
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class BreakableObjBehaviour : MonoBehaviour
    {
        //public CutType cutType = CutType.PartCut;

        public Transform parent;
        public float minHeight;
        public float pieceDepth = 0.5f;
        public float areaUnit = 0.25f;
        [Range(0f, 1f)]
        public float hitStength = 0.5f;
        public float force = 10f;
        private IBreakable breakable;
        private ICutStrategy strategy;
        private IDrawableGizmos drawableGizmos;
        GameObject gizmosSphere;
        MeshFilter meshFilter;
        private void Awake()
        {
            meshFilter = transform.GetComponent<MeshFilter>();
            if (meshFilter)
            {
                breakable = new BreakableObj(meshFilter.mesh, transform);
                strategy = new WholeCutStrategy(breakable, new GenSmallerPieces(new GenPyramidPieces(pieceDepth), areaUnit, transform.position));
            }
            else
            {
                return;
            }
            //SwitchCutType();
        }
        //private void SwitchCutType()
        //{
        //    switch (cutType)
        //    {
        //        case CutType.PartCut:
        //            DetectionSphere detectionSphere = new DetectionSphere(Vector3.one, 0.3f, SegmentVerticalType.Nine, SegmentCircleType.Eight);
        //            strategy = new PartCutTriangleStrategy(breakable, detectionSphere);
        //            drawableGizmos = detectionSphere;
        //            break;
        //        case CutType.TotalCut:
        //            strategy = new WholeCutStrategy(breakable, new GenSmallerPieces(new GenPyramidPieces(pieceDepth), areaUnit, transform.position));
        //            break;
        //    }
        //}
        private GameObject DrawSphere(Vector3 center)
        {
            GameObject sphere = new GameObject();
            var meshfilter = sphere.AddComponent<MeshFilter>();
            sphere.transform.position = center;
            drawableGizmos.transform = sphere.transform;
            drawableGizmos.DrawMesh();
            meshfilter.mesh = drawableGizmos.SphereMesh;
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.red;
            sphere.AddComponent<MeshRenderer>().material = mat;
            return sphere;
        }
        public IBreakable GetBreakable()
        {
            return breakable;
        }
        public void Traversal()
        {
            strategy.Traversal();
        }
        public void Explode(Vector3 hitDirection)
        {
            if (meshFilter == null)
            {
                return;
            }
            Mesh mesh = new Mesh();
            breakable.GetMesh(ref mesh);
            meshFilter.mesh = mesh;
            List<Mesh> peciesMesh = new List<Mesh>();
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
            List<GameObject> peciesObj = new List<GameObject>();
            for (int i = 0; i < peciesMesh.Count; ++i)
            {
                GameObject obj = new GameObject();
                obj.AddComponent<MeshFilter>().mesh = peciesMesh[i];
                obj.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
                obj.name = string.Format("Pieces {0}", i);
                var rgBody = obj.AddComponent<Rigidbody>();
                obj.transform.parent = parent;
                obj.transform.position = transform.position;
                Vector3 offset = peciesMesh[i].bounds.center - meshFilter.mesh.bounds.center;
                offset.Normalize();
                hitDirection.Normalize();
                rgBody.AddForce((hitDirection * hitStength + offset * (1 - hitStength)).normalized * force, ForceMode.Impulse);
                var collider = obj.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = peciesMesh[i];
                peciesObj.Add(obj);
            }
            //if (cutType == CutType.TotalCut)
            //{
            //    Destroy(gameObject);
            //}
        }
        public GameObject DrawGizmos(Transform gizmosParent, Vector3 center, List<GameObject> gizmos)
        {
            //if (cutType != CutType.PartCut)
            //{
            //    return null;
            //}
            if (gizmosSphere == null)
            {
                gizmosSphere = DrawSphere(center);
                gizmosSphere.transform.parent = gizmosParent.transform;
                gizmos.Add(gizmosSphere);
            }
            else
            {
                gizmosSphere.transform.position = center;
                drawableGizmos.transform = gizmosSphere.transform;
            }
            gizmosSphere.SetActive(true);
            return gizmosSphere;
        }
    }
}
