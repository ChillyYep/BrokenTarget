using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BrokenSys
{
    public enum SegmentVerticalType
    {
        Two = 2,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten
    }
    public enum SegmentCircleType
    {
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Twelve = 12,
        Fifteen = 15,
        Eighteen = 18
    }
    public interface GenDetectionObjectStrategy
    {
        /// <summary>
        /// 生成球体数据
        /// </summary>
        void GenSphereData();
        /// <summary>
        /// 将球体数据填入Mesh中
        /// </summary>
        /// <param name="mesh"></param>
        void GenMesh(ref Mesh mesh);
        /// <summary>
        /// 依据极坐标两个角度获得三角面片索引,注意极值的讨论
        /// </summary>
        /// <param name="angleA">竖直方向，从90°到-90°的角度，弧度制</param>
        /// <param name="angleB">水平方向，从-180°到180°的角度，弧度制</param>
        /// <returns>三角面索引</returns>
        int GetTriangleIndex(float angleA, float angleB);
        /// <summary>
        /// offset是圆心坐标，point是检测对象上的顶点位置，据此得到检测球上的三角面片索引
        /// </summary>
        /// <param name="point">检测对象上的顶点位置</param>
        /// <param name="offset">圆心坐标偏移</param>
        /// <returns></returns>
        int GetTriangleIndex(Vector3 point, Vector3 offset);
        int GetTriangleIndex(Vector3 point);
    }
    public class DefaultGenSphereStrategy : GenDetectionObjectStrategy
    {
        public DefaultGenSphereStrategy(SegmentVerticalType segmentType = SegmentVerticalType.Five, SegmentCircleType segmentCircleType = SegmentCircleType.Eighteen, float r = 1f)
        {
            radius = r;
            SetSegmentType(segmentType, segmentCircleType);
        }
        #region property
        public float radius { get; set; }
        public StrategyInfo info { get; set; }
        List<Vector3> verticis = new List<Vector3>();
        List<int> triangles = new List<int>();
        #endregion
        public class StrategyInfo
        {
            public int segmentVerticalCount;
            public int segmentCircleCount;
            public float[] angleSeperator;
            public StrategyInfo(int segmentVerticalCount, int segmentCircleCount = 18)
            {
                angleSeperator = new float[2];
                SetValue(segmentVerticalCount, segmentCircleCount);
            }
            public void SetValue(int segmentVerticalCount, int segmentCircleCount)
            {
                this.segmentVerticalCount = segmentVerticalCount;
                this.segmentCircleCount = segmentCircleCount;
                switch (segmentVerticalCount)
                {
                    case 2:
                        angleSeperator[0] = 90f;
                        break;
                    case 3:
                        angleSeperator[0] = 70f;
                        break;
                    case 4:
                        angleSeperator[0] = 45f;
                        break;
                    case 5:
                        angleSeperator[0] = 39f;
                        break;
                    case 6:
                        angleSeperator[0] = 30f;
                        break;
                    case 7:
                        angleSeperator[0] = 25f;
                        break;
                    case 8:
                        angleSeperator[0] = 18f;
                        break;
                    case 9:
                        angleSeperator[0] = 13f;
                        break;
                    case 10:
                        angleSeperator[0] = 10f;
                        break;
                    default:
                        angleSeperator[0] = 10f;
                        break;
                }
                if (segmentVerticalCount > 2)
                {
                    angleSeperator[1] = (180f - angleSeperator[0] * 2) / (segmentVerticalCount - 2);
                }
                else
                {
                    angleSeperator[1] = 0f;
                }
            }
        }
        public void SetSegmentType(SegmentVerticalType segmentVerticalType, SegmentCircleType segmentCircleType)
        {
            if (info == null)
            {
                info = new StrategyInfo((int)segmentVerticalType, (int)segmentCircleType);
            }
            else
            {
                info.SetValue((int)segmentVerticalType, (int)segmentCircleType);
            }
        }
        public void GenSphereData()
        {
            verticis.Clear();
            triangles.Clear();
            float height = 90f - info.angleSeperator[0];
            int noHeadSegmentAmount = info.segmentVerticalCount - 2;
            int cicleSegmentAmount = info.segmentCircleCount;
            //顶部
            verticis.Add(new Vector3(0f, radius, 0f));
            //中间
            for (int i = 0; i <= noHeadSegmentAmount; ++i)
            {
                VerticisStrip(verticis, height - info.angleSeperator[1] * i, i * cicleSegmentAmount + 1, (i + 1) * cicleSegmentAmount);
            }
            //底部
            verticis.Add(new Vector3(0f, -radius, 0f));

            //顶部
            TriangleFan(triangles, 0, 1, cicleSegmentAmount, false);
            //中间
            for (int i = 1; i <= noHeadSegmentAmount / 2; ++i)
            {
                TriangleStrip(triangles, i * cicleSegmentAmount + 1, (i + 1) * cicleSegmentAmount);
            }
            for (int i = noHeadSegmentAmount / 2 + 1; i <= noHeadSegmentAmount; ++i)
            {
                TriangleStrip(triangles, i * cicleSegmentAmount + 1, (i + 1) * cicleSegmentAmount);
            }
            //底部
            TriangleFan(triangles, verticis.Count - 1, noHeadSegmentAmount * cicleSegmentAmount + 1, (noHeadSegmentAmount + 1) * cicleSegmentAmount, true);
        }
        public void GenMesh(ref Mesh mesh)
        {
            if (verticis.Count <= 0 || triangles.Count <= 0)
            {
                return;
            }
            mesh.vertices = verticis.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = mesh.vertices;
        }
        public int GetTriangleIndex(Vector3 point)
        {
            return GetTriangleIndex(Mathf.Atan(point.y / Mathf.Sqrt(Mathf.Pow(point.x, 2) + Mathf.Pow(point.z, 2))),
                TangentEx(point.x, point.z));
        }
        public int GetTriangleIndex(Vector3 point, Vector3 offset)
        {
            //以球心为原点的坐标系下，新的坐标
            Vector3 newPoint = point - offset;
            return GetTriangleIndex(Mathf.Atan(newPoint.y / Mathf.Sqrt(Mathf.Pow(newPoint.x, 2) + Mathf.Pow(newPoint.z, 2))),
                TangentEx(newPoint.x, newPoint.z));
        }
        public int GetTriangleIndex(float angleA, float angleB)
        {
            int indexV = GetVerticalAngleIndex(angleA);
            int indexH = GetHorizontalAngleIndex(angleB);
            if (indexV == 0)
            {
                return indexH;
            }
            if (indexV == info.segmentVerticalCount - 1)
            {
                return info.segmentCircleCount * (1 + (info.segmentVerticalCount - 2) * 2) + indexH;
            }
            float x = Mathf.Cos(angleA) * Mathf.Cos(angleB);
            float y = Mathf.Sin(angleA);
            float z = Mathf.Cos(angleA) * Mathf.Sin(angleB);
            int pIndex1 = info.segmentCircleCount + (indexV - 1) * info.segmentCircleCount * 2 + indexH * 2;
            int pIndex2 = pIndex1 + 1;
            Vector3 line1 = new Vector3(x, y, z) - verticis[triangles[pIndex1 * 3 + 2]];
            Vector3 line2 = verticis[triangles[pIndex1 * 3 + 2]] - verticis[triangles[pIndex2 * 3 + 2]];//两个三角形的共边
            Vector3 line3 = verticis[triangles[pIndex1 * 3 + 1]] - verticis[triangles[pIndex1 * 3 + 2]];
            Vector3 normal1 = Vector3.Cross(line2, line3).normalized;
            Vector3 normal2 = Vector3.Cross(line2, line1).normalized;
            if (Mathf.Approximately(normal1.x + normal2.x, 0f) && Mathf.Approximately(normal1.y + normal2.y, 0f) && Mathf.Approximately(normal1.z + normal2.z, 0f))
            {
                return pIndex1;
            }
            return pIndex2;
        }
        /// <summary>
        /// 反切的取值范围是(-90°,90°),而水平面的取值是从(-180°,180°)，此函数用于映射
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private float TangentEx(float x, float y)
        {
            if (Mathf.Approximately(x, 0f))
            {
                if (Mathf.Approximately(y, 0f))
                {
                    return 0f;
                }
                else if (y > 0f)
                {
                    return Mathf.PI / 2;
                }
                else if (y < 0f)
                {
                    return -Mathf.PI / 2;
                }
            }
            if (x > 0f)
            {
                return Mathf.Atan(y / x);
            }
            if (y > 0f)
            {
                return Mathf.PI + Mathf.Atan(y / x);
            }
            return -Mathf.PI + Mathf.Atan(y / x);
        }
        //从-180°到180°
        //返回值从0开始
        private int GetHorizontalAngleIndex(float angle)
        {
            if (Mathf.Approximately(angle, Mathf.PI))
            {
                return info.segmentCircleCount - 1;
            }
            return (int)((angle + Mathf.PI) / (Mathf.PI * 2) * info.segmentCircleCount);
        }
        //从90°到-90°
        private int GetVerticalAngleIndex(float angle)
        {
            angle *= Mathf.Rad2Deg;
            if (angle > 90f - info.angleSeperator[0])
            {
                return 0;
            }
            if (angle < -90f + info.angleSeperator[0])
            {
                return info.segmentVerticalCount - 1;
            }
            return (int)((-angle + 90f - info.angleSeperator[0]) / info.angleSeperator[1]) + 1;
        }
        #region 辅助生成函数
        private void VerticisStrip(List<Vector3> verticis, float angleHeight, int begin, int end)
        {
            for (float angle = -180f; angle < 180f; angle += 360f / (end - begin + 1))
            {
                float A = Mathf.Deg2Rad * angleHeight;
                float B = Mathf.Deg2Rad * angle;
                verticis.Add(new Vector3(Mathf.Cos(B) * Mathf.Cos(A), Mathf.Sin(A), Mathf.Sin(B) * Mathf.Cos(A)) * radius);
            }
        }
        private void TriangleFan(List<int> triangles, int center, int begin, int end, bool reverse)
        {
            if (!reverse)
            {
                for (int i = begin; i < end; ++i)
                {
                    triangles.Add(center);
                    triangles.Add(i + 1);
                    triangles.Add(i);
                }
                triangles.Add(center);
                triangles.Add(begin);
                triangles.Add(end);
            }
            else
            {
                for (int i = begin; i < end; ++i)
                {
                    triangles.Add(center);
                    triangles.Add(i);
                    triangles.Add(i + 1);
                }
                triangles.Add(center);
                triangles.Add(end);
                triangles.Add(begin);
            }
        }
        private void TriangleStrip(List<int> triangles, int begin, int end)
        {
            int delta = end - begin + 1;
            for (int i = begin; i < end; ++i)
            {
                triangles.Add(i + 1);
                triangles.Add(i);
                triangles.Add(i - delta);
                triangles.Add(i - delta);
                triangles.Add(i - delta + 1);
                triangles.Add(i + 1);
            }
            {
                triangles.Add(begin);
                triangles.Add(end);
                triangles.Add(end - delta);
                triangles.Add(end - delta);
                triangles.Add(begin - delta);
                triangles.Add(begin);
            }
        }
        #endregion
    }
}