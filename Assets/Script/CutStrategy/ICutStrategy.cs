using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BrokenSys
{
    /// <summary>
    /// 切割策略
    /// </summary>
    public interface ICutStrategy
    {
        IBreakable breakable { get; }
        IGenPieces genPieces { get; }
        List<ModelData> pecies { get; }
        void Traversal();
        IEnumerator Travesal2(Action callback);
    }
}