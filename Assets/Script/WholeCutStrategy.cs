using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;
public class WholeCutStrategy : ICutStrategy
{
    public IBreakable breakable { get; private set; }
    public List<ModelData> pecies { get; private set; }

    public IGenPieces genPieces { get; private set; }
    public WholeCutStrategy(IBreakable breakable, IGenPieces genPieces)
    {
        this.breakable = breakable;
        this.genPieces = genPieces;
        pecies = new List<ModelData>();
    }
    public void Traversal()
    {
        pecies.Clear();
        foreach (var target in breakable.ModelInfos)
        {
            TriangleFace breakableFace = new TriangleFace();
            for (int i = 0; i < target.triangles.Count; i += 3)
            {
                if (target.normals != null && target.normals.Count > 0)
                {
                    VertexData[] vertexData;
                    if (target.uvs != null && target.uvs.Count > 0)
                    {
                        vertexData = new VertexData[]{
                            new VertexData(){ vertex = target.vertices[target.triangles[i]], normal=target.normals[target.triangles[i]], uv=target.uvs[target.triangles[i]] },
                            new VertexData(){ vertex = target.vertices[target.triangles[i+1]], normal=target.normals[target.triangles[i+1]], uv=target.uvs[target.triangles[i+1]] },
                            new VertexData(){ vertex = target.vertices[target.triangles[i+2]], normal=target.normals[target.triangles[i+2]], uv=target.uvs[target.triangles[i+2]] }
                        };
                    }
                    else
                    {
                        vertexData = new VertexData[]
                        {
                            new VertexData(){ vertex = target.vertices[target.triangles[i]], normal=target.normals[target.triangles[i]] },
                            new VertexData(){ vertex = target.vertices[target.triangles[i+1]], normal=target.normals[target.triangles[i+1]] },
                            new VertexData(){ vertex = target.vertices[target.triangles[i+2]], normal=target.normals[target.triangles[i+2]] }
                        };
                    }
                    breakableFace.ReCompute(vertexData[0], vertexData[1], vertexData[2]);
                }
                else
                {
                    breakableFace.ReCompute(target.vertices[target.triangles[i]], target.vertices[target.triangles[i + 1]], target.vertices[target.triangles[i + 2]]);
                }
                pecies.AddRange(genPieces.GenModelData(breakableFace));
            }
        }
    }
}
