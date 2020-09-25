using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chenyi;
public enum GenPiecesType
{
    GenPyramidPieces,
    GenFrustumPieces,
    GenSmallerPieces
}
/// <summary>
/// 可生成碎块的对象
/// </summary>
public interface IGenPieces
{
    List<ModelData> GenModelData(TriangleFace triangleFace);
    GenPiecesType GetEnumType();
}
