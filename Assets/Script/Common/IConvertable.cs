using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 拥有物体空间与世界空间转换的矩阵
/// </summary>
public interface IConvertable
{
    Matrix4x4 World2Object { get; }
    Matrix4x4 Object2World { get; }
}
