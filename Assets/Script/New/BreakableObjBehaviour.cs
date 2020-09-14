using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi2
{
    public class BreakableObjBehaviour : MonoBehaviour
    {
        private IBreakable breakable;
        private List<GameObject> cells = new List<GameObject>();
        private void Awake()
        {
            for(int i=0;i<transform.childCount;++i)
            {
                cells.Add(transform.GetChild(i).gameObject);
            }
        }
        private void Start()
        {
            breakable = new BreakableObj(cells);
        }
        public IBreakable GetBreakable()
        {
            return breakable;
        }
        public void Explode(float power, Vector3 direction)
        {
            breakable.Explode(power, direction);
        }
    }
}
