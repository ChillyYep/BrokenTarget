using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;
public class TotalCutTriangleStrategy : CutTriangleStrategy
{
    public IBreakable breakable { get; private set; }
    public List<ModelData> pecies { get; private set; }
    public TotalCutTriangleStrategy(IBreakable breakable)
    {
        this.breakable = breakable;
    }
    public ModelData GenModelData(ref TriangleFace triangleFace, float depth)
    {
        ModelData modelInfo = new ModelData();
        modelInfo.Init();
        Vector3 vector = ((triangleFace.pointA.vertex + triangleFace.pointB.vertex) / 2 + triangleFace.pointC.vertex) / 2 - triangleFace.normal * depth;
        modelInfo.vertices.Add(triangleFace.pointA.vertex);
        modelInfo.vertices.Add(triangleFace.pointB.vertex);
        modelInfo.vertices.Add(triangleFace.pointC.vertex);
        modelInfo.vertices.Add(triangleFace.pointB.vertex);
        modelInfo.vertices.Add(triangleFace.pointA.vertex);
        modelInfo.vertices.Add(vector);
        modelInfo.vertices.Add(triangleFace.pointC.vertex);
        modelInfo.vertices.Add(triangleFace.pointB.vertex);
        modelInfo.vertices.Add(vector);
        modelInfo.vertices.Add(triangleFace.pointA.vertex);
        modelInfo.vertices.Add(triangleFace.pointC.vertex);
        modelInfo.vertices.Add(vector);
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
                pecies.Add(GenModelData(ref breakableFace, 0.5f));
            }
        }
    }
}
