using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace chenyi
{
    public class Line
    {
        public Vector3 p1;
        public Vector3 p2;
    }
    public class TriangleFace
    {
        public Vector3 normal;
        public Vector3 point;
        public void Compute(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            normal = Vector3.Cross(p1 - p2, p2 - p3);
            normal.Normalize();
            point = p1;
        }
        public bool ComputeCrossPoint(Line line, out Vector3 crossPoint)
        {
            var dotProduct = Vector3.Dot(normal, line.p1);
            if (dotProduct == 0f)
            {
                crossPoint = Vector3.zero;
                return false;
            }
            var t = Vector3.Dot(point - line.p2, normal) / dotProduct;
            crossPoint = line.p2 + line.p1 * t;
            return true;
        }
    }
}
