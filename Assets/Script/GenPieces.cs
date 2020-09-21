using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;
/// <summary>
/// 生成棱台式的碎块
/// </summary>
public class GenFrustumPieces : IGenPieces
{
    private float depth;
    //偏向
    private float v2cMaxFactor = 0.5f;
    public GenFrustumPieces(float depth)
    {
        this.depth = depth;
    }
    public List<ModelData> GenModelData(TriangleFace triangleFace)
    {
        List<ModelData> modelDatas = new List<ModelData>();
        ModelData modelInfo = new ModelData();
        modelDatas.Add(modelInfo);
        modelInfo.Init();
        Vector3 center = ((triangleFace.pointA.vertex + triangleFace.pointB.vertex) / 2f + triangleFace.pointC.vertex) / 2f;
        Vector3 modelCenter = center - triangleFace.normal * depth / 2f;
        Vector3 a2c = center - triangleFace.pointA.vertex;
        Vector3 b2c = center - triangleFace.pointB.vertex;
        Vector3 c2c = center - triangleFace.pointC.vertex;
        Vector3 a1 = triangleFace.pointA.vertex + a2c * v2cMaxFactor * Random.value - triangleFace.normal * depth;
        Vector3 b1 = triangleFace.pointB.vertex + b2c * v2cMaxFactor * Random.value - triangleFace.normal * depth;
        Vector3 c1 = triangleFace.pointC.vertex + c2c * v2cMaxFactor * Random.value - triangleFace.normal * depth;
        modelInfo.vertices.Add(triangleFace.pointA.vertex);
        modelInfo.normals.Add((triangleFace.pointA.vertex - modelCenter).normalized);
        modelInfo.vertices.Add(triangleFace.pointB.vertex);
        modelInfo.normals.Add((triangleFace.pointB.vertex - modelCenter).normalized);
        modelInfo.vertices.Add(triangleFace.pointC.vertex);
        modelInfo.normals.Add((triangleFace.pointC.vertex - modelCenter).normalized);
        modelInfo.vertices.Add(a1);
        modelInfo.normals.Add((a1 - modelCenter).normalized);
        modelInfo.vertices.Add(b1);
        modelInfo.normals.Add((b1 - modelCenter).normalized);
        modelInfo.vertices.Add(c1);
        modelInfo.normals.Add((c1 - modelCenter).normalized);
        modelInfo.triangles.Add(0);
        modelInfo.triangles.Add(1);
        modelInfo.triangles.Add(2);
        for (int i = 0; i < 3; ++i)
        {
            modelInfo.triangles.Add(i);
            modelInfo.triangles.Add(i + 3);
            modelInfo.triangles.Add((i + 1) % 3);
            modelInfo.triangles.Add(i + 3);
            modelInfo.triangles.Add((i + 1) % 3 + 3);
            modelInfo.triangles.Add((i + 1) % 3);
        }
        modelInfo.triangles.Add(4);
        modelInfo.triangles.Add(3);
        modelInfo.triangles.Add(5);
        return modelDatas;
    }
}
/// <summary>
/// 生成棱锥式的碎块
/// </summary>
public class GenPyramidPieces : IGenPieces
{
    private float depth;
    public GenPyramidPieces(float depth)
    {
        this.depth = depth;
    }
    public List<ModelData> GenModelData(TriangleFace triangleFace)
    {
        List<ModelData> modelDatas = new List<ModelData>();
        ModelData modelInfo = new ModelData();
        modelDatas.Add(modelInfo);
        modelInfo.Init();
        Vector3 newPoint = ((triangleFace.pointA.vertex + triangleFace.pointB.vertex) / 2f + triangleFace.pointC.vertex) / 2f - triangleFace.normal * depth;
        modelInfo.vertices.Add(triangleFace.pointA.vertex);
        //modelInfo.uvs.Add(triangleFace.pointA.uv);
        modelInfo.vertices.Add(triangleFace.pointB.vertex);
        //modelInfo.uvs.Add(triangleFace.pointB.uv);
        modelInfo.vertices.Add(triangleFace.pointC.vertex);
        //modelInfo.uvs.Add(triangleFace.pointC.uv);
        modelInfo.vertices.Add(triangleFace.pointB.vertex);
        modelInfo.vertices.Add(triangleFace.pointA.vertex);
        modelInfo.vertices.Add(newPoint);
        modelInfo.vertices.Add(triangleFace.pointC.vertex);
        modelInfo.vertices.Add(triangleFace.pointB.vertex);
        modelInfo.vertices.Add(newPoint);
        modelInfo.vertices.Add(triangleFace.pointA.vertex);
        modelInfo.vertices.Add(triangleFace.pointC.vertex);
        modelInfo.vertices.Add(newPoint);
        Vector3[] normals = new Vector3[]
        {
            Vector3.Cross(triangleFace.pointB.vertex - triangleFace.pointA.vertex, triangleFace.pointC.vertex - triangleFace.pointA.vertex).normalized,
            Vector3.Cross(modelInfo.vertices[5] - triangleFace.pointA.vertex,triangleFace.pointB.vertex - triangleFace.pointA.vertex).normalized,
            Vector3.Cross(modelInfo.vertices[5] - triangleFace.pointB.vertex,triangleFace.pointC.vertex - triangleFace.pointB.vertex).normalized,
            Vector3.Cross(modelInfo.vertices[5] - triangleFace.pointC.vertex,triangleFace.pointA.vertex - triangleFace.pointC.vertex).normalized
        };
        for (int i = 0; i < 12; ++i)
        {
            modelInfo.triangles.Add(i);
        }
        for (int i = 0; i < 4; ++i)
        {
            modelInfo.normals.Add(normals[i]);
            modelInfo.normals.Add(normals[i]);
            modelInfo.normals.Add(normals[i]);
        }
        return modelDatas;
    }
}
/// <summary>
/// 嵌套一个IGenPieces对象,曲面细分后再用该IGenPieces对象生成碎片
/// </summary>
public class GenSmallerPieces : IGenPieces
{
    private List<TriangleFace> splitTriangleFace = new List<TriangleFace>();
    private IGenPieces sonGenPieces;
    private float areaUnit;
    private Vector3 center;
    public GenSmallerPieces(IGenPieces sonGenPieces, float areaUnit,Vector3 center)
    {
        this.sonGenPieces = sonGenPieces;
        this.areaUnit = areaUnit;
        this.center = center;
    }
    private float ComputeArea(ref TriangleFace triangleFace)
    {
        Vector3 ab = triangleFace.pointA.vertex - triangleFace.pointB.vertex;
        Vector3 ac = triangleFace.pointC.vertex - triangleFace.pointA.vertex;
        float height = Mathf.Sqrt(Vector3.Dot(ab, ab) - Mathf.Pow(Vector3.Dot(ab, ac.normalized), 2f));
        return height * ab.magnitude / 2f;
    }
    private void SplitTwoPart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
    {
        Vector3 ba = triangleFace.pointA.vertex - triangleFace.pointB.vertex;
        Vector3 ac = triangleFace.pointC.vertex - triangleFace.pointA.vertex;
        Vector3 cb = triangleFace.pointB.vertex - triangleFace.pointC.vertex;
        Vector3 maxLenVec;
        VertexData startPointData;
        VertexData endPointData;
        VertexData leftPointData;
        if (ba.magnitude > ac.magnitude)
        {
            maxLenVec = ba;
            startPointData = triangleFace.pointB;
            endPointData = triangleFace.pointA;
            leftPointData = triangleFace.pointC;
        }
        else
        {
            maxLenVec = ac;
            startPointData = triangleFace.pointA;
            endPointData = triangleFace.pointC;
            leftPointData = triangleFace.pointB;
        }
        if (maxLenVec.magnitude < cb.magnitude)
        {
            maxLenVec = cb;
            startPointData = triangleFace.pointB;
            endPointData = triangleFace.pointC;
            leftPointData = triangleFace.pointA;
        }
        Vector3 newPoint = startPointData.vertex + maxLenVec * 0.5f;
        VertexData newPointData = triangleFace.Interlopation(newPoint, true, true);
        faceStack.Push(new TriangleFace(leftPointData, endPointData, newPointData));
        faceStack.Push(new TriangleFace(leftPointData, newPointData, startPointData));
    }
    private void SplitThreePart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
    {
        Vector3 barycentric;
        triangleFace.ComputeBarycentric(out barycentric);
        VertexData newPointData = triangleFace.Interlopation(barycentric, true, true);
        faceStack.Push(new TriangleFace(triangleFace.pointA, triangleFace.pointB, newPointData));
        faceStack.Push(new TriangleFace(triangleFace.pointB, triangleFace.pointC, newPointData));
        faceStack.Push(new TriangleFace(triangleFace.pointC, triangleFace.pointA, newPointData));
    }
    private void SplitFourPart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
    {
        Vector3 midAB = (triangleFace.pointA.vertex + triangleFace.pointB.vertex) * 0.5f;
        Vector3 midAC = (triangleFace.pointA.vertex + triangleFace.pointC.vertex) * 0.5f;
        Vector3 midBC = (triangleFace.pointC.vertex + triangleFace.pointB.vertex) * 0.5f;
        VertexData midABData = triangleFace.Interlopation(midAB, true, true);
        VertexData midACData = triangleFace.Interlopation(midAC, true, true);
        VertexData midBCData = triangleFace.Interlopation(midBC, true, true);
        faceStack.Push(new TriangleFace(triangleFace.pointA, midABData, midACData));
        faceStack.Push(new TriangleFace(midABData, triangleFace.pointB, midBCData));
        faceStack.Push(new TriangleFace(midACData, midABData, midBCData));
        faceStack.Push(new TriangleFace(midACData, midBCData, triangleFace.pointC));
    }
    /// <summary>
    /// 切割后的三角形面积会小于areaUnit
    /// </summary>
    /// <param name="areaUnit"></param>
    /// <returns></returns>
    public void SplitByArea(ref TriangleFace triangleFace)
    {
        Stack<TriangleFace> faceStack = new Stack<TriangleFace>();
        faceStack.Push(triangleFace);
        while (faceStack.Count > 0)
        {
            var curFace = faceStack.Pop();
            float triangleArea = ComputeArea(ref curFace);
            int count = (int)(triangleArea / areaUnit) + 1;
            if (count <= 1)
            {
                splitTriangleFace.Add(curFace);
                continue;
            }
            if (count % 4 == 0)
            {
                SplitFourPart(ref curFace, ref faceStack);
            }
            else if (count % 3 == 0)
            {
                SplitThreePart(ref curFace, ref faceStack);
            }
            else if (count % 2 == 0)
            {
                SplitTwoPart(ref curFace, ref faceStack);
            }
            else
            {
                if (count / 2 > 0)
                {
                    SplitTwoPart(ref curFace, ref faceStack);
                }
                else if (count / 3 > 0)
                {
                    SplitThreePart(ref curFace, ref faceStack);
                }
                else if (count / 4 > 0)
                {
                    SplitFourPart(ref curFace, ref faceStack);
                }
            }
        }
    }
    private void PreProcess(ref TriangleFace triangleFace)
    {
        splitTriangleFace.Clear();
        SplitByArea(ref triangleFace);
    }
    public List<ModelData> GenModelData(TriangleFace triangleFace)
    {
        List<ModelData> modelDatas = new List<ModelData>();
        PreProcess(ref triangleFace);
        for (int i = 0; i < splitTriangleFace.Count; ++i)
        {
            modelDatas.AddRange(sonGenPieces.GenModelData(splitTriangleFace[i]));
        }
        return modelDatas;
    }
}
