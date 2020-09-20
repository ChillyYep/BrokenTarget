using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;
public class TotalCutTriangleStrategy : CutTriangleStrategy
{
    public IBreakable breakable { get; private set; }
    public List<ModelData> pecies { get; private set; }
    private float depth = 0.5f;
    //偏向
    private float v2cFactor = 0.5f;
    public TotalCutTriangleStrategy(IBreakable breakable)
    {
        this.breakable = breakable;
    }
    public ModelData GenModelData(ref TriangleFace triangleFace)
    {
        ModelData modelInfo = new ModelData();
        modelInfo.Init();
        Vector3 newPoint = ((triangleFace.pointA.vertex + triangleFace.pointB.vertex) / 2f + triangleFace.pointC.vertex) / 2f - triangleFace.normal * depth;
        modelInfo.vertices.Add(triangleFace.pointA.vertex);
        modelInfo.vertices.Add(triangleFace.pointB.vertex);
        modelInfo.vertices.Add(triangleFace.pointC.vertex);
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
            Vector3.Cross(modelInfo.vertices[3] - triangleFace.pointA.vertex,triangleFace.pointB.vertex - triangleFace.pointA.vertex).normalized,
            Vector3.Cross(modelInfo.vertices[3] - triangleFace.pointB.vertex,triangleFace.pointC.vertex - triangleFace.pointB.vertex).normalized,
            Vector3.Cross(modelInfo.vertices[3] - triangleFace.pointC.vertex,triangleFace.pointA.vertex - triangleFace.pointC.vertex).normalized
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
        return modelInfo;
    }
    public ModelData GenModelData2(ref TriangleFace triangleFace)
    {
        ModelData modelInfo = new ModelData();
        modelInfo.Init();
        Vector3 center = ((triangleFace.pointA.vertex + triangleFace.pointB.vertex) / 2f + triangleFace.pointC.vertex) / 2f;
        Vector3 modelCenter = center - triangleFace.normal * depth / 2f;
        Vector3 a2c = center - triangleFace.pointA.vertex;
        Vector3 b2c = center - triangleFace.pointB.vertex;
        Vector3 c2c = center - triangleFace.pointC.vertex;
        Vector3 a1 = triangleFace.pointA.vertex + a2c * Random.value - triangleFace.normal * depth;
        Vector3 b1 = triangleFace.pointB.vertex + b2c * Random.value - triangleFace.normal * depth;
        Vector3 c1 = triangleFace.pointC.vertex + c2c * Random.value - triangleFace.normal * depth;
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
        return modelInfo;
    }
    public void Traversal()
    {
        pecies = new List<ModelData>();
        foreach (var target in breakable.ModelInfos)
        {
            TriangleFace breakableFace = new TriangleFace();
            for (int i = 0; i < target.triangles.Count; i += 3)
            {
                if (target.normals != null && target.normals.Count > 0)
                {
                    VertexData[] vertexData = new VertexData[]
                    {
                        new VertexData(){vertex= target.vertices[target.triangles[i]],normal=target.normals[target.triangles[i]] },
                        new VertexData(){vertex= target.vertices[target.triangles[i+1]],normal=target.normals[target.triangles[i+1]] },
                        new VertexData(){vertex= target.vertices[target.triangles[i+2]],normal=target.normals[target.triangles[i+2]] }
                    };
                    breakableFace.ReCompute(vertexData[0], vertexData[1], vertexData[2]);
                }
                else
                {
                    breakableFace.ReCompute(target.vertices[target.triangles[i]], target.vertices[target.triangles[i + 1]], target.vertices[target.triangles[i + 2]]);
                }
                pecies.Add(GenModelData2(ref breakableFace));
            }
        }
    }
}
