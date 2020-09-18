using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IConvertable
{
    Matrix4x4 World2Object { get; }
    Matrix4x4 Object2World { get; }
}
