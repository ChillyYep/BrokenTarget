using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace chenyi
{
    public class Line
    {
        public Vector3 p1;
        public Vector3 p2;
        private const float diff = 0.01f;
        private Vector3 p1p2;
        private bool hasComputeDir = false;
        public bool ComputeCrossPoint(Line otherLine, out Vector3 crossPoint, bool noMatterLength = false)
        {
            p1p2 = Dir();
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
            if (vector1 == Vector3.zero)
            {
                return true;
            }
            p1p2 = Dir();
            Vector3 vector2 = p1p2.normalized;
            return (Mathf.Abs(vector1.x - vector2.x) < diff && Mathf.Abs(vector1.y - vector2.y) < diff && Mathf.Abs(vector1.z - vector2.z) < diff) ||
                (Mathf.Abs(vector1.x + vector2.x) < diff && Mathf.Abs(vector1.y + vector2.y) < diff && Mathf.Abs(vector1.z + vector2.z) < diff);
        }
        public float Distance(Vector3 vertex)
        {
            p1p2 = Dir();
            Vector3 vector = vertex - p1;
            return Mathf.Sqrt(vector.magnitude * vector.magnitude - Mathf.Pow(Vector3.Dot(vector, p1p2.normalized), 2));
        }
        public Vector3 Dir()
        {
            if (!hasComputeDir)
            {
                p1p2 = p2 - p1;
                hasComputeDir = true;
            }
            return p1p2;
        }
        /// <summary>
        /// 计算球与直线的两个交点
        /// </summary>
        public int ComputeCrossPointWithSphere(Vector3 center, float radius, out Vector3[] vectors, bool onLine = true)
        {
            p1p2 = p2 - p1;
            float A = Vector3.Dot(p1p2, p1p2);
            float B = Vector3.Dot(p1p2, p1) * 2f;
            float C = Vector3.Dot(p1, p1) - radius * radius;
            float m1 = (-B + Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);
            float m2 = (-B - Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);
            int index = 0;
            vectors = new Vector3[2];
            if (onLine)
            {
                if (m1 >= 0f && m1 <= 1f)
                {
                    vectors[++index] = p1 + m1 * p1p2;
                }
                if (m2 >= 0f && m2 <= 1f)
                {
                    vectors[++index] = p1 + m1 * p1p2;
                }
                return index;
            }
            vectors[0] = p1 + m1 * p1p2;
            vectors[1] = p1 + m2 * p1p2;
            return 2;
        }
    }
    public class TriangleOutData
    {
        public List<Vector3> vertexes = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
    }
    public struct VertexData
    {
        public Vector3 vertex;
        public Vector2 uv;
        public Vector3 normal;
        public VertexData(Vector3 vertex, Vector2 uv, Vector3 normal)
        {
            this.vertex = vertex;
            this.uv = uv;
            this.normal = normal;
        }
    }
    public struct TriangleFace
    {
        public Vector3 normal;
        public VertexData pointA;
        public VertexData pointB;
        public VertexData pointC;
        public bool needInterlopte { get; set; }
        public TriangleFace(VertexData point1, VertexData point2, VertexData point3)
        {
            normal = Vector3.Cross(point1.vertex - point2.vertex, point1.vertex - point3.vertex);
            normal.Normalize();
            pointA = point1;
            pointB = point2;
            pointC = point3;
            needInterlopte = true;
        }
        public TriangleFace(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            normal = Vector3.Cross(point1 - point2, point1 - point3);
            normal.Normalize();
            pointA = new VertexData() { vertex = point1 };
            pointB = new VertexData() { vertex = point2 };
            pointC = new VertexData() { vertex = point3 };
            needInterlopte = false;
        }
        public void ReCompute(VertexData point1, VertexData point2, VertexData point3)
        {
            normal = Vector3.Cross(point1.vertex - point2.vertex, point1.vertex - point3.vertex);
            normal.Normalize();
            pointA = point1;
            pointB = point2;
            pointC = point3;
        }
        public void ReCompute(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            normal = Vector3.Cross(point1 - point2, point1 - point3);
            normal.Normalize();
            pointA.vertex = point1;
            pointB.vertex = point2;
            pointC.vertex = point3;
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
            var t = Vector3.Dot(pointA.vertex - line.p1, normal) / dotProduct;
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
        public VertexData Interlopation(Vector3 vertex)
        {
            VertexData triangleData = new VertexData() { vertex = vertex };
            //可以利用点与三边的距离来计算中心坐标
            if (needInterlopte)
            {
                Vector3 ab = pointB.vertex - pointA.vertex;
                ab.Normalize();
                Vector3 ac = pointC.vertex - pointA.vertex;
                ac.Normalize();
                Vector3 bc = pointC.vertex - pointB.vertex;
                bc.Normalize();
                Vector3 a2v = vertex - pointA.vertex;
                Vector3 b2v = vertex - pointB.vertex;
                Vector3 c2v = vertex - pointC.vertex;
                if (IsInTriangle(a2v, b2v, c2v, ab, bc, -ac))
                {
                    float w1 = (Vector3.Dot(a2v, ab) * ab - a2v).magnitude;
                    float w2 = (Vector3.Dot(a2v, ac) * ac - a2v).magnitude;
                    float w3 = (Vector3.Dot(a2v, bc) * bc - a2v).magnitude;
                    float W = w1 + w2 + w3;
                    w1 = w1 / W;
                    w2 = w2 / W;
                    w3 = 1f - w1 - w2;
                    triangleData.normal = w1 * pointC.normal + w2 * pointB.normal + w3 * pointA.normal;
                    triangleData.uv = w1 * pointC.uv + w2 * pointB.uv + w3 * pointA.uv;
                }
            }
            return triangleData;
        }
        public bool IsAboveTriangle(Vector3 vertex)
        {
            var vector = vertex - pointA.vertex;
            return Vector3.Dot(vector, normal) > 0;
        }
        public bool IsBelowTriangle(Vector3 vertex)
        {
            var vector = vertex - pointA.vertex;
            return Vector3.Dot(vector, normal) < 0;
        }
        static public float Distance(Vector3 vertex, TriangleFace triangleFace)
        {
            var vector = vertex - triangleFace.pointA.vertex;
            return Mathf.Abs(Vector3.Dot(vector, triangleFace.normal));
        }
        public float Distance(Vector3 vertex)
        {
            return Distance(vertex, this);
        }
        public Vector3 Map2ThisFace(Vector3 vertex)
        {
            float distance = Vector3.Dot(vertex - pointA.vertex, normal);
            return vertex - normal * distance;
        }
        
        /// <summary>
        /// 包含了在边上的顶点
        /// </summary>
        public bool IsInTriangle(Vector3 center, bool includeTriSide = true)
        {
            Vector3 a2c = center - pointA.vertex;
            Vector3 b2c = center - pointB.vertex;
            Vector3 c2c = center - pointC.vertex;
            Vector3 ab = pointB.vertex - pointA.vertex;
            Vector3 bc = pointC.vertex - pointB.vertex;
            Vector3 ca = pointA.vertex - pointC.vertex;
            return IsInTriangle(a2c, b2c, c2c, ab, bc, ca, includeTriSide);
        }
        static public bool IsInTriangle(Vector3 center, TriangleFace triangleFace, bool includeTriSide = true)
        {
            Vector3 a2c = center - triangleFace.pointA.vertex;
            Vector3 b2c = center - triangleFace.pointB.vertex;
            Vector3 c2c = center - triangleFace.pointC.vertex;
            Vector3 ab = triangleFace.pointB.vertex - triangleFace.pointA.vertex;
            Vector3 bc = triangleFace.pointC.vertex - triangleFace.pointB.vertex;
            Vector3 ca = triangleFace.pointA.vertex - triangleFace.pointC.vertex;
            return IsInTriangle(a2c, b2c, c2c, ab, bc, ca, includeTriSide);
        }
        /// <summary>
        /// 内部辅助函数,前三个参数为三角形三顶点到center的向量，后三个参数为三角形三边向量
        /// </summary>
        public static bool IsInTriangle(Vector3 a2c, Vector3 b2c, Vector3 c2c, Vector3 ab, Vector3 bc, Vector3 ca, bool includeTriSide = true)
        {
            float diff = 0.1f;
            Vector3 normal1 = Vector3.Cross(a2c, ab).normalized;
            Vector3 normal2 = Vector3.Cross(b2c, bc).normalized;
            Vector3 normal3 = Vector3.Cross(c2c, ca).normalized;
            if (!includeTriSide && (normal1 == Vector3.zero || normal2 == Vector3.zero || normal3 == Vector3.zero))
            {
                return false;
            }
            //注意，可能出现点恰好在三角形某边的直线上或与三角形顶点重合，所以要将三个法线两两比较
            if ((Mathf.Abs(normal1.x + normal2.x) < diff && Mathf.Abs(normal1.y + normal2.y) < diff && Mathf.Abs(normal1.z + normal2.z) < diff) ||
                (Mathf.Abs(normal1.x + normal3.x) < diff && Mathf.Abs(normal1.y + normal3.y) < diff && Mathf.Abs(normal1.z + normal3.z) < diff) ||
                (Mathf.Abs(normal2.x + normal3.x) < diff && Mathf.Abs(normal2.y + normal3.y) < diff && Mathf.Abs(normal2.z + normal3.z) < diff))
            {
                return false;
            }
            return true;
        }

    }
}
