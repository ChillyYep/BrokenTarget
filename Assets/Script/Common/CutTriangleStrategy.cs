using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;
public enum IncludeVetextType
{
    Zero,
    One,
    Two,
    Three
}
public interface CutTriangleStrategy : IConvertable
{
    SegmentCircleType segmentCircleType { get; }

    //SegmentVerticalType segmentVerticalType { get; set; }
    void CutTriangle(IncludeVetextType includeVetextType, chenyi.TriangleFace triangleFace, Vector3 center, float radius, TriangleOutInfo triangleOutInfo);
}
public class DefaultCutTriangleStrategy : CutTriangleStrategy
{
    public SegmentCircleType segmentCircleType { get; private set; }
    public Matrix4x4 World2Object { get; private set; }
    public Matrix4x4 Object2World { get; private set; }
    //public SegmentVerticalType segmentVerticalType { get; set; }
    private int[] vertexsInTriangleIndices;
    private List<Vector3> vertexs = new List<Vector3>();
    int[] sideVertexIndex = new int[2];
    public DefaultCutTriangleStrategy(SegmentCircleType segmentCircleType, Transform transform)
    {
        World2Object = transform.worldToLocalMatrix;
        Object2World = transform.localToWorldMatrix;
        this.segmentCircleType = segmentCircleType;
    }
    public void CutTriangle(IncludeVetextType includeVetextType, TriangleFace triangleFace, Vector3 center, float radius, TriangleOutInfo triangleOutInfo)
    {
        vertexs.Clear();
        List<int> auxIndices = new List<int>();

        Vector3 originNormal = new Vector3(0f, 1f, 0f);
        //逆时针生成
        for (float angle = -180f; angle < 180f; angle += 360f / (float)segmentCircleType)
        {
            vertexs.Add(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius);
        }

        Quaternion quaternion = Quaternion.FromToRotation(originNormal, triangleFace.normal);

        for (int i = 0; i < vertexs.Count; ++i)
        {
            Vector3 oldVertex = vertexs[i];
            Vector3 oldVertex2 = quaternion * oldVertex;
            vertexs[i] = oldVertex2 + center;
            if (triangleFace.IsInTriangle(vertexs[i]))
            {
                auxIndices.Add(i);
            }
        }
        //判断索引是否连续
        int index = -1;
        for (int i = 1; i < auxIndices.Count; ++i)
        {
            if (auxIndices[i] != auxIndices[i - 1] + 1)
            {
                index = i;
                break;
            }
        }
        if (index > -1)
        {
            vertexsInTriangleIndices = new int[auxIndices.Count];
            for (int j = 0; j < auxIndices.Count; ++j)
            {
                vertexsInTriangleIndices[j] = auxIndices[(index + j) % auxIndices.Count];
            }
        }
        else
        {
            vertexsInTriangleIndices = auxIndices.ToArray();
        }
        if (vertexsInTriangleIndices.Length > 0)
        {
            sideVertexIndex[0] = (vertexsInTriangleIndices[0] + vertexs.Count - 1) % vertexs.Count;
            sideVertexIndex[1] = (vertexsInTriangleIndices[vertexsInTriangleIndices.Length - 1] + 1) % vertexs.Count;
        }
        else
        {
            const float diff = 0.1f;
            Vector3 c2p;
            if ((triangleFace.pointA - center).magnitude < radius)
            {
                c2p = triangleFace.pointA - center;
            }
            else if ((triangleFace.pointB - center).magnitude < radius)
            {
                c2p = triangleFace.pointB - center;
            }
            else if ((triangleFace.pointC - center).magnitude < radius)
            {
                c2p = triangleFace.pointC - center;
            }
            else
            {
                Debug.LogError("没有要生成的三角形");
                return;
            }
            for (int i = 0; i < vertexs.Count; ++i)
            {
                Vector3 normal1 = Vector3.Cross(vertexs[i] - center, c2p).normalized;
                Vector3 normal2 = Vector3.Cross(c2p, vertexs[(i + 1) % vertexs.Count] - center).normalized;
                if (normal1.x + normal2.x < diff && normal1.y + normal2.y < diff && normal1.z + normal2.z < diff)
                {
                    sideVertexIndex[0] = i;
                    sideVertexIndex[1] = (i + 1) % vertexs.Count;
                    break;
                }
            }
        }
        switch (includeVetextType)
        {
            case IncludeVetextType.Zero:
                CutTriangleIncludeZeroVertex(ref triangleFace, ref center, radius, triangleOutInfo);
                break;
            case IncludeVetextType.One:
                CutTriangleIncludeOneVertex(ref triangleFace, ref center, radius, triangleOutInfo);
                break;
            case IncludeVetextType.Two:
                CutTriangleIncludeTwoVertex(ref triangleFace, ref center, radius, triangleOutInfo);
                break;
        }
    }
    private void CutTriangleIncludeZeroVertex(ref TriangleFace triangleFace, ref Vector3 center, float radius, TriangleOutInfo triangleOutInfo)
    {

    }
    private void GenVertex(ref TriangleFace triangleFace, ref Vector3 center, float radius, TriangleOutInfo triangleOutInfo, Line line1, Line line2)
    {
        Vector3 crossPoint;

        if (vertexsInTriangleIndices.Length > 0)
        {
            if (!line1.IsOnLineNoMatterLength(vertexs[vertexsInTriangleIndices[0]]))
            {
                if (line1.ComputeCrossPoint(new Line()
                {
                    p1 = vertexs[vertexsInTriangleIndices[0]],
                    p2 = vertexs[sideVertexIndex[0]]
                }, out crossPoint))
                {
                    triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(crossPoint));
                }
            }

            for (int i = 0; i < vertexsInTriangleIndices.Length; ++i)
            {
                triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(vertexs[vertexsInTriangleIndices[i]]));
            }
            if (!line2.IsOnLineNoMatterLength(vertexs[vertexsInTriangleIndices[vertexsInTriangleIndices.Length - 1]]))
            {
                if (line2.ComputeCrossPoint(new Line()
                {
                    p1 = vertexs[vertexsInTriangleIndices[vertexsInTriangleIndices.Length - 1]],
                    p2 = vertexs[sideVertexIndex[1]]
                }, out crossPoint))
                {
                    triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(crossPoint));
                }
            }
        }
        else
        {
            {
                if (line1.ComputeCrossPoint(new Line()
                {
                    p1 = vertexs[sideVertexIndex[0]],
                    p2 = vertexs[sideVertexIndex[1]]
                }, out crossPoint))
                {
                    triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(crossPoint));
                }
            }
            {
                if (line2.ComputeCrossPoint(new Line()
                {
                    p1 = vertexs[sideVertexIndex[0]],
                    p2 = vertexs[sideVertexIndex[1]]
                }, out crossPoint))
                {
                    triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(crossPoint));
                }
            }
        }
    }
    private void CutTriangleIncludeOneVertex(ref TriangleFace triangleFace, ref Vector3 center, float radius, TriangleOutInfo triangleOutInfo)
    {
        Line line1;
        Line line2;
        if ((triangleFace.pointA - center).magnitude < radius)
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointA,
                p2 = triangleFace.pointC
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointA,
                p2 = triangleFace.pointB
            };
        }
        else if ((triangleFace.pointB - center).magnitude < radius)
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointB,
                p2 = triangleFace.pointA
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointB,
                p2 = triangleFace.pointC
            };
        }
        else
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointC,
                p2 = triangleFace.pointB
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointC,
                p2 = triangleFace.pointA
            };
        }

        GenVertex(ref triangleFace, ref center, radius, triangleOutInfo, line1, line2);

        int count = triangleOutInfo.vertexes.Count;
        Line line3 = new Line() { p1 = line1.p2, p2 = line2.p2 };
        Vector3 vector = line3.p2 - line3.p1;
        Vector3 objectVector = World2Object.MultiplyPoint(line3.p2) - World2Object.MultiplyPoint(line3.p1);
        if (count <= 1) return;
        float[] sideWeight = new float[count - 1];
        float totalWeight = 0f;
        for (int i = 0; i < count - 1; ++i)
        {
            Vector3 sideVec = triangleOutInfo.vertexes[i + 1] - triangleOutInfo.vertexes[i];
            float dotProduct = Mathf.Abs(Vector3.Dot(sideVec, objectVector));
            totalWeight += dotProduct;
            sideWeight[i] = totalWeight;
        }
        triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(line3.p1));
        for (int i = 0; i < count - 2; ++i)
        {
            triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint((line3.p1 + (sideWeight[i] / totalWeight) * vector)));
        }
        triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(line3.p2));
        Debug.LogFormat("**********顶点生成完毕*********************{0}个顶点", triangleOutInfo.vertexes.Count);
        if (triangleOutInfo.vertexes.Count != count * 2)
        {
            Debug.LogError("顶点生成有误！");
            return;
        }
        for (int i = 0; i < count - 1; ++i)
        {
            triangleOutInfo.triangles.Add(i);
            triangleOutInfo.triangles.Add(i + 1);
            triangleOutInfo.triangles.Add(i + count);
            triangleOutInfo.triangles.Add(i + 1);
            triangleOutInfo.triangles.Add(i + count + 1);
            triangleOutInfo.triangles.Add(i + count);
        }
        Debug.LogFormat("**********三角面生成完毕*********************{0}个三角形", triangleOutInfo.triangles.Count);
    }

    private void CutTriangleIncludeTwoVertex(ref TriangleFace triangleFace, ref Vector3 center, float radius, TriangleOutInfo triangleOutInfo)
    {
        Line line1;
        Line line2;

        if ((triangleFace.pointA - center).magnitude > radius)
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointA,
                p2 = triangleFace.pointC
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointA,
                p2 = triangleFace.pointB
            };
        }
        else if ((triangleFace.pointB - center).magnitude > radius)
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointB,
                p2 = triangleFace.pointA
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointB,
                p2 = triangleFace.pointC
            };
        }
        else
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointC,
                p2 = triangleFace.pointB
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointC,
                p2 = triangleFace.pointA
            };
        }
        triangleOutInfo.vertexes.Add(line1.p1);
        GenVertex(ref triangleFace, ref center, radius, triangleOutInfo, line1, line2);
        Debug.LogFormat("**********顶点生成完毕*********************{0}个顶点", triangleOutInfo.vertexes.Count);
        for (int i = 0; i < triangleOutInfo.vertexes.Count - 1; ++i)
        {
            triangleOutInfo.triangles.Add(i);
            triangleOutInfo.triangles.Add(i + 1);
            triangleOutInfo.triangles.Add(0);
        }
        Debug.LogFormat("**********三角面生成完毕*********************{0}个三角形", triangleOutInfo.triangles.Count);
    }
}
