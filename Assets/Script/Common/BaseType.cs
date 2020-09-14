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
    public struct TriangleFace
    {
        public Vector3 normal;
        public Vector3 pointA;
        public Vector3 pointB;
        public Vector3 pointC;
        public void Compute(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            normal = Vector3.Cross(p1 - p2, p2 - p3);
            normal.Normalize();
            pointA = p1;
            pointB = p2;
            pointC = p3;
        }
        /// <summary>
        /// 线段与三角面的交点,必须在三角形内
        /// </summary>
        /// <param name="line"></param>
        /// <param name="crossPoint"></param>
        /// <returns></returns>
        public bool ComputeCrossPoint(Line line, out Vector3 crossPoint, bool checkIfInTriangle = false)
        {
            var p1p2 = line.p2 - line.p1;
            var dotProduct = Vector3.Dot(normal, p1p2);
            if (dotProduct == 0f)
            {
                crossPoint = Vector3.zero;
                return false;
            }
            var t = Vector3.Dot(pointA - line.p1, normal) / dotProduct;
            if (t > 1f)
            {
                crossPoint = Vector3.zero;
                return false;
            }
            crossPoint = line.p1 + p1p2 * t;
            if (checkIfInTriangle && !IsInTriangle(crossPoint))
            {
                crossPoint = Vector3.zero;
                return false;
            }
            return true;
        }
        public bool IsAboveTriangle(Vector3 vertex)
        {
            var vector = vertex - pointA;
            return Vector3.Dot(vector, normal) > 0;
        }
        public bool IsBelowTriangle(Vector3 vertex)
        {
            var vector = vertex - pointA;
            return Vector3.Dot(vector, normal) < 0;
        }
        public float Distance(Vector3 vertex)
        {
            var vector = vertex - pointA;
            return Mathf.Abs(Vector3.Dot(vector, normal));
        }
        public Vector3 Map2ThisFace(Vector3 vertex)
        {
            float distance = Vector3.Dot(vertex - pointA, normal);
            return vertex - normal * distance;
        }
        public bool IsInTriangle(Vector3 center)
        {
            float a = 0f;
            float b = 0f;
            if (!Mathf.Approximately(pointA.x, pointB.x) && !Mathf.Approximately(pointA.y, pointB.y))
            {
                b = (pointA.x * pointC.y - pointC.x * pointA.y + pointC.x * pointB.y - pointB.x * pointC.y + pointB.x * pointA.y - pointA.x * pointB.y) / (center.x * (pointB.y - pointA.y) - center.y * (pointB.x - pointA.x));
                a = (center.x - b * (pointC.x - pointA.x)) / (pointB.x - pointA.x);
            }
            return a + b < 1f && a > 0f && b > 0f;
        }
        public bool IsSphereOutTriangle(Vector3 center, float radius)
        {
            float distance = Distance(center);
            if (distance > radius)
            {
                return true;
            }
            Vector3 mapPoint = Map2ThisFace(center);
            if (IsInTriangle(mapPoint))
            {
                return false;
            }
            float radius2 = Mathf.Sqrt(radius * radius - distance * distance);
            Vector3 a2c = mapPoint - pointA;
            Vector3 c2c = mapPoint - pointC;
            Vector3 ab = pointB - pointA;
            Vector3 cb = pointB - pointC;
            Vector3 ca = pointA - pointC;
            if (Mathf.Sqrt(a2c.magnitude * a2c.magnitude + Mathf.Pow(Vector3.Dot(a2c, ab.normalized), 2)) < radius2)
            {
                return false;
            }
            if (Mathf.Sqrt(c2c.magnitude * c2c.magnitude + Mathf.Pow(Vector3.Dot(c2c, ca.normalized), 2)) < radius2)
            {
                return false;
            }
            if (Mathf.Sqrt(c2c.magnitude * c2c.magnitude + Mathf.Pow(Vector3.Dot(c2c, cb.normalized), 2)) < radius2)
            {
                return false;
            }
            return true;
        }
    }
}
