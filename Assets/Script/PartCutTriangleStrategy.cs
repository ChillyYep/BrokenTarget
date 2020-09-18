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

public class PartCutTriangleStrategy : CutTriangleStrategy, IConvertable
{
    public SegmentCircleType segmentCircleType { get; private set; }
    public Matrix4x4 World2Object { get; private set; }
    public Matrix4x4 Object2World { get; private set; }
    private List<Vector3> vertexs = new List<Vector3>();
    private List<Vector3> vertexsCopy = new List<Vector3>();

    int[] sideVertexIndex = new int[2];
    int[] vertexsIndicesInTriangle;
    private IncludeVetextType includeVetextType = IncludeVetextType.Three;
    private DetectionSphere detectionSphere;
    public IBreakable breakable { get; private set; }
    public List<ModelData> pecies { get; private set; }
    public PartCutTriangleStrategy(IBreakable breakable, DetectionSphere detectionSphere)
    {
        World2Object = breakable.transform.worldToLocalMatrix;
        Object2World = breakable.transform.localToWorldMatrix;
        this.breakable = breakable;
        this.segmentCircleType = detectionSphere.segmentCircleType;
        this.detectionSphere = detectionSphere;
    }
    private void Init()
    {
        vertexs.Clear();
        for (float angle = -180f; angle < 180f; angle += 360f / (float)segmentCircleType)
        {
            vertexs.Add(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)));
        }
    }
    public ModelData GenModelData(ref TriangleFace triangleFace, float depth)
    {
        return new ModelData();
    }
    private bool IsSphereOutTriangle(ref TriangleFace triangleFace, out TriangleOutData triangleOutInfo)
    {
        float distance = triangleFace.Distance(detectionSphere.center);
        triangleOutInfo = null;
        if (distance > detectionSphere.radius)
        {
            return true;
        }
        Vector3 mapPoint = triangleFace.Map2ThisFace(detectionSphere.center);
        float radius2 = Mathf.Sqrt(detectionSphere.radius * detectionSphere.radius - distance * distance);
        Vector3 a2c = mapPoint - triangleFace.pointA.vertex;
        Vector3 b2c = mapPoint - triangleFace.pointB.vertex;
        Vector3 c2c = mapPoint - triangleFace.pointC.vertex;
        Vector3 ab = triangleFace.pointB.vertex - triangleFace.pointA.vertex;
        Vector3 bc = triangleFace.pointC.vertex - triangleFace.pointB.vertex;
        Vector3 ca = triangleFace.pointA.vertex - triangleFace.pointC.vertex;
        if (TriangleFace.IsInTriangle(a2c, b2c, c2c, ab, bc, ca))
        {
            triangleOutInfo = new TriangleOutData();
            Prepare(triangleFace, mapPoint, radius2);
            CutTriangle(triangleFace, mapPoint, radius2, triangleOutInfo);
            return false;
        }
        if ((Mathf.Sqrt(a2c.magnitude * a2c.magnitude + Mathf.Pow(Vector3.Dot(a2c, ab.normalized), 2)) < radius2) ||
            (Mathf.Sqrt(c2c.magnitude * c2c.magnitude + Mathf.Pow(Vector3.Dot(c2c, ca.normalized), 2)) < radius2) ||
            Mathf.Sqrt(c2c.magnitude * c2c.magnitude + Mathf.Pow(Vector3.Dot(c2c, (-bc).normalized), 2)) < radius2
            )
        {
            triangleOutInfo = new TriangleOutData();
            Prepare(triangleFace, mapPoint, radius2);
            CutTriangle(triangleFace, mapPoint, radius2, triangleOutInfo);
            return false;
        }
        return true;
    }
    private void JudgeIncludeVetextType(ref TriangleFace triangleFace, Vector3 center, float radius)
    {
        int count = 0;
        Vector3[] points = new Vector3[]
        {
            triangleFace.pointA.vertex,
            triangleFace.pointB.vertex,
            triangleFace.pointC.vertex
        };
        if ((triangleFace.pointA.vertex - center).magnitude < radius)
        {
            ++count;
        }
        if ((triangleFace.pointB.vertex - center).magnitude < radius)
        {
            ++count;
        }
        if ((triangleFace.pointC.vertex - center).magnitude < radius)
        {
            ++count;
        }
        //for (int j = 0; j < 3; ++j)
        //{
        //    if (IsInSphere(points[j],center))
        //    {
        //        ++count;
        //    }
        //}
        includeVetextType = (IncludeVetextType)count;
    }
    private bool IsInSphere(Vector3 vertex, Vector3 center)
    {
        TriangleFace face = new TriangleFace();
        for (int i = 0; i < vertexsCopy.Count; ++i)
        {
            face.ReCompute(center, vertexsCopy[i], vertexsCopy[(i + 1) % vertexsCopy.Count]);
            if (face.IsInTriangle(vertex))
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 保持静态的物体
    /// </summary>
    /// <param name="target"></param>
    public virtual void Traversal()
    {
        Init();
        TriangleFace breakableFace = new TriangleFace();
        List<ModelData> newModelInfoList = new List<ModelData>();
        ModelData addModelInfo = new ModelData();
        addModelInfo.Init();
        foreach (var target in breakable.ModelInfos)
        {
            var newModelInfo = new ModelData();
            newModelInfo.Init();
            for (int i = 0; i < target.triangles.Count; i += 3)
            {
                Vector3[] targetPoint = new Vector3[]
                {
                    breakable.Object2World.MultiplyPoint(target.vertices[target.triangles[i]]),
                    breakable.Object2World.MultiplyPoint(target.vertices[target.triangles[i + 1]]),
                    breakable.Object2World.MultiplyPoint(target.vertices[target.triangles[i + 2]])
                };
                breakableFace.ReCompute(targetPoint[0], targetPoint[1], targetPoint[2]);
                //用球半径粗略计算哪些三角形要加入三角形切割，其中可能会有不被切割的三角形
                TriangleOutData triangleOutInfo;
                if (IsSphereOutTriangle(ref breakableFace, out triangleOutInfo))
                {
                    newModelInfo.triangles.Add(target.triangles[i]);
                    newModelInfo.triangles.Add(target.triangles[i + 1]);
                    newModelInfo.triangles.Add(target.triangles[i + 2]);
                }
                else
                {
                    int tempCount = addModelInfo.vertices.Count;
                    for (int x = 0; x < triangleOutInfo.triangles.Count; ++x)
                    {
                        addModelInfo.triangles.Add(triangleOutInfo.triangles[x] + tempCount);
                    }
                    addModelInfo.vertices.AddRange(triangleOutInfo.vertexes);
                    addModelInfo.normals.AddRange(triangleOutInfo.normals);
                    addModelInfo.uvs.AddRange(triangleOutInfo.uvs);
                }
            }
            newModelInfo.vertices.AddRange(target.vertices);
            newModelInfo.normals.AddRange(target.normals);
            newModelInfo.uvs.AddRange(target.uvs);
            newModelInfoList.Add(newModelInfo);
        }
        newModelInfoList.Add(addModelInfo);
        breakable.ModelInfos.Clear();
        foreach (var modelInfo in newModelInfoList)
        {
            breakable.ModelInfos.Add(modelInfo);
        }
        breakable.CreateMesh();

    }
    public void Prepare(TriangleFace triangleFace, Vector3 center, float radius)
    {
        vertexsCopy.Clear();
        Vector3 originNormal = new Vector3(0f, 1f, 0f);
        //逆时针生成
        for (int i = 0; i < vertexs.Count; ++i)
        {
            vertexsCopy.Add(vertexs[i] * radius);
        }

        Quaternion quaternion = Quaternion.FromToRotation(originNormal, triangleFace.normal);
        for (int i = 0; i < vertexsCopy.Count; ++i)
        {
            vertexsCopy[i] = quaternion * vertexsCopy[i] + center;
        }
    }
    public void CutTriangle(TriangleFace triangleFace, Vector3 center, float radius, TriangleOutData triangleOutInfo)
    {
        JudgeIncludeVetextType(ref triangleFace, center, radius);
        if (vertexsCopy.Count <= 0 || includeVetextType == IncludeVetextType.Three)
        {
            return;
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
    #region 辅助
    private void Blit(TriangleOutData from, TriangleOutData to)
    {
        if (from != null && to != null)
        {
            int[] trianglesTemp = new int[from.triangles.Count];
            int count = to.vertexes.Count;
            for (int i = 0; i < from.triangles.Count; ++i)
            {
                trianglesTemp[i] = from.triangles[i] + count;
            }
            to.triangles.AddRange(trianglesTemp);
            to.vertexes.AddRange(from.vertexes);
            to.normals.AddRange(from.normals);
            to.uvs.AddRange(from.uvs);
        }
    }
    private bool Compare(float distance, float radius, bool isGreater)
    {
        if (isGreater)
        {
            return distance > radius;
        }
        return distance < radius;
    }
    /// <summary>
    /// 生成顶点
    /// </summary>
    /// <param name="isGreater">三角面上保留或去除的唯一点与圆心的距离，需要和圆半径比较</param>
    private void GenVertex(ref TriangleFace triangleFace, ref Vector3 center, float radius, TriangleOutData triangleOutInfo, Line line1, Line line2, bool isGreater = false)
    {
        for (int i = 0; i < 2; ++i)
        {
            sideVertexIndex[i] = 0;
        }
        List<int> auxIndices = new List<int>();
        vertexsIndicesInTriangle = null;
        for (int i = 0; i < vertexsCopy.Count; ++i)
        {
            if (triangleFace.IsInTriangle(vertexsCopy[i]))
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
            vertexsIndicesInTriangle = new int[auxIndices.Count];
            for (int j = 0; j < auxIndices.Count; ++j)
            {
                vertexsIndicesInTriangle[j] = auxIndices[(index + j) % auxIndices.Count];
            }
        }
        else
        {
            vertexsIndicesInTriangle = auxIndices.ToArray();
        }
        if (vertexsIndicesInTriangle.Length > 0)
        {
            sideVertexIndex[0] = (vertexsIndicesInTriangle[0] + vertexsCopy.Count - 1) % vertexsCopy.Count;
            sideVertexIndex[1] = (vertexsIndicesInTriangle[vertexsIndicesInTriangle.Length - 1] + 1) % vertexsCopy.Count;
        }
        else
        {
            const float diff = 0.001f;
            Vector3 c2p;
            if (Compare((triangleFace.pointA.vertex - center).magnitude, radius, isGreater))
            {
                c2p = triangleFace.pointA.vertex - center;
            }
            else if (Compare((triangleFace.pointB.vertex - center).magnitude, radius, isGreater))
            {
                c2p = triangleFace.pointB.vertex - center;
            }
            else if (Compare((triangleFace.pointC.vertex - center).magnitude, radius, isGreater))
            {
                c2p = triangleFace.pointC.vertex - center;
            }
            else
            {
                Debug.LogError("没有要生成的三角形");
                return;
            }
            c2p.Normalize();
            for (int i = 0; i < vertexsCopy.Count; ++i)
            {
                Vector3 vector1 = vertexsCopy[i] - center;
                Vector3 vector2 = vertexsCopy[(i + 1) % vertexsCopy.Count] - center;
                Vector3 vector3 = vector1 - Vector3.Dot(vector1, c2p) * c2p;
                Vector3 vector4 = vector2 - Vector3.Dot(vector2, c2p) * c2p;
                if ((1f - Vector3.Dot(vector1, c2p)) < diff || (1f - Vector3.Dot(vector2, c2p)) < diff || Vector3.Dot(vector1, c2p) > 0 &&
                    (Mathf.Abs(vector3.x + vector4.x) < diff && Mathf.Abs(vector3.y + vector4.y) < diff && Mathf.Abs(vector3.z + vector4.z) < diff))
                {
                    sideVertexIndex[0] = i;
                    sideVertexIndex[1] = (i + 1) % vertexsCopy.Count;
                    break;
                }
            }
        }
        Vector3 crossPoint;

        if (vertexsIndicesInTriangle.Length > 0)
        {
            if (!line1.IsOnLineNoMatterLength(vertexsCopy[vertexsIndicesInTriangle[0]]))    //防止重复Add
            {
                if (line1.ComputeCrossPoint(new Line()
                {
                    p1 = vertexsCopy[vertexsIndicesInTriangle[0]],
                    p2 = vertexsCopy[sideVertexIndex[0]]
                }, out crossPoint))
                {
                    triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(crossPoint));
                }
            }

            for (int i = 0; i < vertexsIndicesInTriangle.Length; ++i)
            {
                triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(vertexsCopy[vertexsIndicesInTriangle[i]]));
            }
            if (!line2.IsOnLineNoMatterLength(vertexsCopy[vertexsIndicesInTriangle[vertexsIndicesInTriangle.Length - 1]]))
            {
                if (line2.ComputeCrossPoint(new Line()
                {
                    p1 = vertexsCopy[vertexsIndicesInTriangle[vertexsIndicesInTriangle.Length - 1]],
                    p2 = vertexsCopy[sideVertexIndex[1]]
                }, out crossPoint))
                {
                    triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(crossPoint));
                }
            }
        }
        else
        {
            if (line1.ComputeCrossPoint(new Line()
            {
                p1 = vertexsCopy[sideVertexIndex[0]],
                p2 = vertexsCopy[sideVertexIndex[1]]
            }, out crossPoint))
            {
                triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(crossPoint));
            }
            if (line2.ComputeCrossPoint(new Line()
            {
                p1 = vertexsCopy[sideVertexIndex[0]],
                p2 = vertexsCopy[sideVertexIndex[1]]
            }, out crossPoint))
            {
                triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(crossPoint));
            }
        }
    }
    #endregion
    #region 按类型切割函数
    private void CutTriangleIncludeZeroVertex(ref TriangleFace triangleFace, ref Vector3 center, float radius, TriangleOutData triangleOutInfo)
    {

    }
    private void CutTriangleIncludeOneVertex(ref TriangleFace triangleFace, ref Vector3 center, float radius, TriangleOutData triangleOutInfo)
    {
        Line line1;
        Line line2;
        if (IsInSphere(triangleFace.pointA.vertex, center))
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointA.vertex,
                p2 = triangleFace.pointC.vertex
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointA.vertex,
                p2 = triangleFace.pointB.vertex
            };
        }
        else if (IsInSphere(triangleFace.pointB.vertex, center))
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointB.vertex,
                p2 = triangleFace.pointA.vertex
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointB.vertex,
                p2 = triangleFace.pointC.vertex
            };
        }
        else
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointC.vertex,
                p2 = triangleFace.pointB.vertex
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointC.vertex,
                p2 = triangleFace.pointA.vertex
            };
        }
        Line line3 = new Line() { p1 = line1.p2, p2 = line2.p2 };
        float distance = line3.Distance(center);
        //圆与三角形有四个交点
        if (distance < radius)
        {
            //分解成两个三角形送入CutTriangleIncludeTwoVertex中处理
            Vector3 newP = line3.Dir().normalized * distance + line3.p1;
            TriangleFace triangleFace1 = new TriangleFace(newP, line3.p2, line1.p1);
            TriangleFace triangleFace2 = new TriangleFace(newP, line1.p1, line3.p1);
            TriangleOutData info1 = new TriangleOutData();
            TriangleOutData info2 = new TriangleOutData();
            CutTriangleIncludeTwoVertex(ref triangleFace1, ref center, radius, info1);
            CutTriangleIncludeTwoVertex(ref triangleFace2, ref center, radius, info2);
            Blit(info1, triangleOutInfo);
            Blit(info2, triangleOutInfo);
            return;
        }
        GenVertex(ref triangleFace, ref center, radius, triangleOutInfo, line1, line2);

        int count = triangleOutInfo.vertexes.Count;
        Vector3 vector = line3.p2 - line3.p1;
        Vector3 objectVector = World2Object.MultiplyPoint(line3.p2) - World2Object.MultiplyPoint(line3.p1);
        if (count == 0)
        {
            triangleOutInfo.vertexes.Clear();
            triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(line1.p1));
            triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(line3.p1));
            triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(line3.p2));
            triangleOutInfo.triangles.Add(0);
            triangleOutInfo.triangles.Add(1);
            triangleOutInfo.triangles.Add(2);
            return;
        }
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
    private void CutTriangleIncludeTwoVertex(ref TriangleFace triangleFace, ref Vector3 center, float radius, TriangleOutData triangleOutInfo)
    {
        Line line1;
        Line line2;

        if (!IsInSphere(triangleFace.pointA.vertex, center))
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointA.vertex,
                p2 = triangleFace.pointB.vertex
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointA.vertex,
                p2 = triangleFace.pointC.vertex
            };
        }
        else if (!IsInSphere(triangleFace.pointB.vertex, center))
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointB.vertex,
                p2 = triangleFace.pointC.vertex
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointB.vertex,
                p2 = triangleFace.pointA.vertex
            };
        }
        else
        {
            line1 = new Line()
            {
                p1 = triangleFace.pointC.vertex,
                p2 = triangleFace.pointA.vertex
            };
            line2 = new Line()
            {
                p1 = triangleFace.pointC.vertex,
                p2 = triangleFace.pointB.vertex
            };
        }
        GenVertex(ref triangleFace, ref center, radius, triangleOutInfo, line1, line2, true);
        triangleOutInfo.vertexes.Add(World2Object.MultiplyPoint(line1.p1));
        Debug.LogFormat("**********顶点生成完毕*********************{0}个顶点", triangleOutInfo.vertexes.Count);
        for (int i = 0; i < triangleOutInfo.vertexes.Count - 2; ++i)
        {
            triangleOutInfo.triangles.Add(i);
            triangleOutInfo.triangles.Add(i + 1);
            triangleOutInfo.triangles.Add(triangleOutInfo.vertexes.Count - 1);
        }
        Debug.LogFormat("**********三角面生成完毕*********************{0}个三角形", triangleOutInfo.triangles.Count);
    }
    #endregion
}
