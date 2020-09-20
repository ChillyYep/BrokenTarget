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
    public Transform gizmosParent;
    private List<GameObject> gizmos = new List<GameObject>();
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
    public void ClearGizmos()
    {
        if (Input.GetMouseButtonUp(0))
        {
            for(int i=0;i<gizmos.Count;++i)
            {
                if(gizmos[i]!=null)
                {
                    gizmos[i].SetActive(false);
                }
            }
        }
    }
    void Update()
    {
        ClearGizmos();
        if ((catched = CatchSelectTarget()))
        {
            //var breakableBehaviour = crossTarget.GetComponent<BreakableObjBehaviour>();
            var breakableBehaviour = crossTarget.GetComponent<BreakableObjBehaviour>();
            if (breakableBehaviour != null)
            {
                breakableBehaviour.DrawGizmos(gizmosParent, crossPoint, gizmos);
                breakableBehaviour.Traversal();
                breakableBehaviour.Explode();
            }
        }
    }
}