using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeAction : MonoBehaviour
{
    //模拟重力
    Vector3 gravity = new Vector3(0f, -10f, 0f);
    Rigidbody rgBody;
    public float minHeight;
    private float realMinHeight;
    Vector3 speed;
    Mesh mesh;
    public void Initialized()
    {
        rgBody = GetComponent<Rigidbody>();
        mesh = transform.GetComponent<MeshFilter>().mesh;
        //realMinHeight = minHeight + mesh.bounds.extents.y;
        enabled = false;
    }
    /// <summary>
    /// 启动发射器
    /// </summary>
    /// <param name="flyDir">碎块总体飞去的方向</param>
    /// <param name="offset">离爆炸中心距离不同，偏移程度不同</param>
    /// <param name="explodeForce">模拟爆炸力量</param>
    public void Emit(Vector3 flyDir, Vector3 offset, float explodeForce)
    {
        Vector3 explodeDir = flyDir + offset;
        speed = explodeDir.normalized;
        speed *= explodeForce * explodeDir.magnitude;
        if (rgBody != null && rgBody.isKinematic)
        {
            enabled = true;
        }
    }
    public void Stop()
    {
        enabled = false;
    }
    private void Update()
    {
        if (rgBody.isKinematic)
        {
            if (transform.position.y <= minHeight)
            {
                speed = Vector3.zero;
                transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
                enabled = false;
            }
            else
            {
                transform.position += speed * Time.deltaTime;
                speed += gravity * Time.deltaTime;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision enabled");
    }
}
