using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    /// <summary>
    /// 持有可破碎物体的数据
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class BreakableObjBehaviour : MonoBehaviour
    {
        private IBreakable breakable;
        private void Awake()
        {
            if (gameObject.GetComponent<MeshFilter>())
            {
                breakable = new BreakableObj(gameObject.GetComponent<MeshFilter>().mesh,transform);
            }
        }
        public IBreakable GetBreakable()
        {
            return breakable;
        }
        public void Explode()
        {
            Mesh mesh = new Mesh();
            breakable.GetNew(ref mesh);
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}
