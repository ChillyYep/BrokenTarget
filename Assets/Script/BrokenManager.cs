using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;

public class BrokenManager : MonoBehaviour
{
    public Vector3 crossPoint { get; private set; }
    public Vector3 crossNormal { get; private set; }
    public Vector3 hitDirection { get; private set; }
    GameObject crossTarget;
    public Transform gizmosParent;
    private List<GameObject> gizmos = new List<GameObject>();
    private bool catched = false;
    public static BrokenManager Instance { get; private set; }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }
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
            for (int i = 0; i < gizmos.Count; ++i)
            {
                if (gizmos[i] != null)
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
            var breakableBehaviour = crossTarget.GetComponent<BreakableObjBehaviour>();
            if (breakableBehaviour != null)
            {
                breakableBehaviour.Traversal();
                breakableBehaviour.Explode();
            }
        }
    }
}