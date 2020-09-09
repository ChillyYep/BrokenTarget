﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi2
{
    public class BreakableObjBehaviour : MonoBehaviour
    {
        private IBreakable breakable;
        private Transform parent;
        private void Awake()
        {
            parent = transform;
            if (gameObject.GetComponent<MeshFilter>())
            {
                breakable = new BreakableObj(gameObject.GetComponent<MeshFilter>().mesh);
            }
        }
        private void Start()
        {
            CreateBoxSet();
        }
        private void CreateBoxSet()
        {
            //cube.transform.loca
            for (float x = -4.5f; x <= 4.5f; x += 1f)
            {
                for (float z = -4.5f; z <= 4.5f; z += 1f)
                {
                    for (float y = -4.5f; y <= 4.5f; y += 1f)
                    {
                        var cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cell.transform.parent = parent;
                        cell.transform.localPosition = new Vector3(x, y, z);
                    }
                }
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
