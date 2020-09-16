using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace chenyi
{
    public class Line
    {
        public Vector3 p1;
        public Vector3 p2;
        private Vector3 p1p2;
        public bool ComputeCrossPoint(Line otherLine, out Vector3 crossPoint, bool noMatterLength = false)
        {
            p1p2 = p2 - p1;
            Vector3 p3p4 = otherLine.p2 - otherLine.p1;
            float n;
            if (!Mathf.Approximately(p1p2.y * p3p4.x - p1p2.x * p3p4.y, 0f))
            {
                n = (p1p2.x * (otherLine.p1.y - p1.y) - p1p2.y * (otherLine.p1.x - p1.x)) / (p1p2.y * p3p4.x - p1p2.x * p3p4.y);
            }
            else if (!Mathf.Approximately(p1p2.z * p3p4.x - p1p2.x * p3p4.z, 0f))
            {
                n = (p1p2.x * (otherLine.p1.z - p1.z) - p1p2.z * (otherLine.p1.x - p1.x)) / (p1p2.z * p3p4.x - p1p2.x * p3p4.z);
            }
            else if (!Mathf.Approximately(p1p2.z * p3p4.y - p1p2.y * p3p4.z, 0f))
            {
                n = (p1p2.y * (otherLine.p1.z - p1.z) - p1p2.z * (otherLine.p1.y - p1.y)) / (p1p2.z * p3p4.y - p1p2.y * p3p4.z);
            }
            else if (!Mathf.Approximately(p1p2.x * p3p4.y - p1p2.y * p3p4.x, 0f))
            {
                n = (p1p2.y * (otherLine.p1.x - p1.x) - p1p2.x * (otherLine.p1.y - p1.y)) / (p1p2.x * p3p4.y - p1p2.y * p3p4.x);
            }
            else if (!Mathf.Approximately(p1p2.x * p3p4.z - p1p2.z * p3p4.x, 0f))
            {
                n = (p1p2.z * (otherLine.p1.x - p1.x) - p1p2.x * (otherLine.p1.z - p1.z)) / (p1p2.x * p3p4.z - p1p2.z * p3p4.x);
            }
            else if (!Mathf.Approximately(p1p2.y * p3p4.z - p1p2.z * p3p4.y, 0f))
            {
                n = (p1p2.z * (otherLine.p1.y - p1.y) - p1p2.y * (otherLine.p1.z - p1.z)) / (p1p2.y * p3p4.z - p1p2.z * p3p4.y);
            }
            else
            {
                crossPoint = Vector3.zero;
                return false;
            }
            if (!noMatterLength && (n > 1 || n < 0))
            {
                crossPoint = Vector3.zero;
                return false;
            }
            crossPoint = otherLine.p1 + n * p3p4;
            return true;
        }
        public bool IsOnLineNoMatterLength(Vector3 vertex)
        {
            Vector3 vector1 = (vertex - p1).normalized;
            Vector3 vector2 = p1p2.normalized;
            return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y) && Mathf.Approximately(vector1.z, vector2.z);
        }
    }
    public class TriangleOutInfo
    {
        public List<Vector3> vertexes = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
    }
    public struct TriangleFace
    {
        public Vector3 normal;
        public Vector3 pointA;
        public Vector3 pointB;
        public Vector3 pointC;
        public void Compute(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            normal = Vector3.Cross(p1 - p2, p1 - p3);
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
        /// <summary>
        /// 包含了在边上的顶点
        /// </summary>
        public bool IsInTriangle(Vector3 center)
        {
            Vector3 a2c = center - pointA;
            Vector3 b2c = center - pointB;
            Vector3 c2c = center - pointC;
            Vector3 ab = pointB - pointA;
            Vector3 bc = pointC - pointB;
            Vector3 ca = pointA - pointC;
            return IsInTriangle(a2c, b2c, c2c, ab, bc, ca);
        }
        /// <summary>
        /// 内部辅助函数,前三个参数为三角形三顶点到center的向量，后三个参数为三角形三边向量
        /// </summary>
        private bool IsInTriangle(Vector3 a2c, Vector3 b2c, Vector3 c2c, Vector3 ab, Vector3 bc, Vector3 ca)
        {
            float diff = 0.1f;
            Vector3 normal1 = Vector3.Cross(a2c, ab).normalized;
            Vector3 normal2 = Vector3.Cross(b2c, bc).normalized;
            Vector3 normal3 = Vector3.Cross(c2c, ca).normalized;
            //注意，可能出现点恰好在三角形某边的直线上，所以要将三个法线两两比较
            if ((Mathf.Abs(normal1.x + normal2.x) < diff && Mathf.Abs(normal1.y + normal2.y) < diff && Mathf.Abs(normal1.z + normal2.z) < diff) ||
                (Mathf.Abs(normal1.x + normal3.x) < diff && Mathf.Abs(normal1.y + normal3.y) < diff && Mathf.Abs(normal1.z + normal3.z) < diff) ||
                (Mathf.Abs(normal2.x + normal3.x) < diff && Mathf.Abs(normal2.y + normal3.y) < diff && Mathf.Abs(normal2.z + normal3.z) < diff))
            {
                return false;
            }
            return true;
        }
        public bool IsSphereOutTriangle(Vector3 center, float radius, out TriangleOutInfo triangleOutInfo, CutTriangleStrategy cutTriangleStrategy = null)
        {
            float distance = Distance(center);
            triangleOutInfo = null;
            if (distance > radius)
            {
                return true;
            }
            Vector3 mapPoint = Map2ThisFace(center);
            float radius2 = Mathf.Sqrt(radius * radius - distance * distance);
            Vector3 a2c = mapPoint - pointA;
            Vector3 b2c = mapPoint - pointB;
            Vector3 c2c = mapPoint - pointC;
            Vector3 ab = pointB - pointA;
            Vector3 bc = pointC - pointB;
            Vector3 ca = pointA - pointC;
            if (IsInTriangle(a2c, b2c, c2c, ab, bc, ca))
            {
                if (cutTriangleStrategy != null)
                {
                    triangleOutInfo = new TriangleOutInfo();
                    int count = 0;
                    if ((pointA - mapPoint).magnitude < radius2)
                    {
                        ++count;
                    }
                    if ((pointB - mapPoint).magnitude < radius2)
                    {
                        ++count;
                    }
                    if ((pointC - mapPoint).magnitude < radius2)
                    {
                        ++count;
                    }
                    if (count <= (int)IncludeVetextType.Three && count >= (int)IncludeVetextType.Zero)
                    {
                        cutTriangleStrategy.CutTriangle((IncludeVetextType)count, this, mapPoint, radius2, triangleOutInfo);
                    }
                }
                return false;
            }
            if ((Mathf.Sqrt(a2c.magnitude * a2c.magnitude + Mathf.Pow(Vector3.Dot(a2c, ab.normalized), 2)) < radius2) ||
                (Mathf.Sqrt(c2c.magnitude * c2c.magnitude + Mathf.Pow(Vector3.Dot(c2c, ca.normalized), 2)) < radius2) ||
                Mathf.Sqrt(c2c.magnitude * c2c.magnitude + Mathf.Pow(Vector3.Dot(c2c, (-bc).normalized), 2)) < radius2
                )
            {
                if (cutTriangleStrategy != null)
                {
                    triangleOutInfo = new TriangleOutInfo(); 
                    int count = 0;
                    if ((pointA - mapPoint).magnitude < radius2)
                    {
                        ++count;
                    }
                    if ((pointB - mapPoint).magnitude < radius2)
                    {
                        ++count;
                    }
                    if ((pointC - mapPoint).magnitude < radius2)
                    {
                        ++count;
                    }
                    if (count <= (int)IncludeVetextType.Three && count >= (int)IncludeVetextType.Zero)
                    {
                        cutTriangleStrategy.CutTriangle((IncludeVetextType)count, this, mapPoint, radius2, triangleOutInfo);
                    }
                }
                return false;
            }
            return true;
        }
    }
}
