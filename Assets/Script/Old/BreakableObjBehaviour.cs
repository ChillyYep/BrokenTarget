using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    //[RequireComponent(typeof(MeshFilter))]
    public class BreakableObjBehaviour : MonoBehaviour
    {
        private IBreakable breakable;
        private void Awake()
        {
            if (gameObject.GetComponent<MeshFilter>())
            {
                breakable = new BreakableObj(gameObject.GetComponent<MeshFilter>().mesh);
            }
        }
        public IBreakable GetBreakable()
        {
            return breakable;
        }
        public void Refresh()
        {
            Mesh mesh = new Mesh();
            breakable.GetNew(ref mesh);
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}
