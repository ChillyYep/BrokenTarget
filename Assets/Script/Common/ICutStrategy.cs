using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;
/// <summary>
/// 切割策略
/// </summary>
public interface ICutStrategy
{
    IBreakable breakable { get; }
    IGenPieces genPieces { get; }
    List<ModelData> pecies { get; }
    void Traversal();
}