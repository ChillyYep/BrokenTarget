using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;

public interface CutTriangleStrategy
{
    IBreakable breakable { get; }
    List<ModelData> pecies { get; }
    ModelData GenModelData(ref TriangleFace triangleFace, float depth);
    void Traversal();
}