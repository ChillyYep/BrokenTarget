using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;

public class BrokenManager : MonoBehaviour
{
    Vector3 crossPoint;
    Vector3 crossNormal;
    Vector3 hitDirection;
    GameObject crossTarget;
    DetectionSphere detectionSphere = new DetectionSphere(Vector3.one, 0.5f, SegmentVerticalType.Four, SegmentCircleType.Five);
    public Transform gizmosParent;
    private GameObject explodeSphere;
    private bool catched = false;
    private bool CatchSelectTarget()
    {
        RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out raycastHit) && Input.GetMouseButtonDown(0))
        {
            crossPoint = raycastHit.point;
            crossNormal = raycastHit.normal;
            hitDirection = ray.direction;
            crossTarget = raycastHit.collider.gameObject;
            return true;
        }
        return false;
    }
    private GameObject DrawSphere(Vector3 center, float radius)
    {
        GameObject sphere = new GameObject();
        var meshfilter = sphere.AddComponent<MeshFilter>();
        sphere.transform.position = center;
        detectionSphere.radius = radius;
        detectionSphere.DrawSphere();
        meshfilter.mesh = detectionSphere.SphereMesh;
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        sphere.AddComponent<MeshRenderer>().material = mat;
        return sphere;
    }
    private void ClearGizmos()
    {
        if (Input.GetMouseButtonUp(0) && explodeSphere != null)
        {
            //for (int i = 0; i < gizmosParent.transform.childCount; ++i)
            //{
            //    DestroyImmediate(gizmosParent.transform.GetChild(i).gameObject);
            //}
            //gizmosParent.transform.DetachChildren();
            explodeSphere.SetActive(false);
        }
    }
    private void DrawGizmos()
    {
        if (explodeSphere == null)
        {
            explodeSphere = DrawSphere(detectionSphere.center, detectionSphere.radius);
            explodeSphere.transform.parent = gizmosParent.transform;
        }
        else
        {
            explodeSphere.transform.localPosition = detectionSphere.center;
        }
        explodeSphere.SetActive(true);
    }
    void Update()
    {
        ClearGizmos();
        if ((catched = CatchSelectTarget()))
        {
            detectionSphere.center = crossPoint;
            DrawGizmos();
            //var breakableBehaviour = crossTarget.transform.parent.GetComponent<BreakableObjBehaviour>();
            var breakableBehaviour = crossTarget.GetComponent<BreakableObjBehaviour>();
            if (breakableBehaviour != null)
            {
                detectionSphere.Traversal(breakableBehaviour.GetBreakable());
                //breakableBehaviour.Explode(20f, hitDirection);
                breakableBehaviour.Explode();
            }
        }
    }
}