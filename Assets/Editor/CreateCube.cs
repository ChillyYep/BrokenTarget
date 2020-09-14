using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateCube
{
    [MenuItem("GameObject/Tools/CreateMutiCubeObj",false,10)]
    public static void Create()
    {
        if(Selection.activeGameObject==null)
        {
            return;
        }
        float delta = 0.5f;
        for (float x = -4.5f; x <= 4.5f; x += delta)
        {
            for (float z = -4.5f; z <= 4.5f; z += delta)
            {
                for (float y = -4.5f; y <= 4.5f; y += delta)
                {
                    var cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cell.transform.parent = Selection.activeGameObject.transform;
                    cell.transform.localPosition = new Vector3(x, y, z);
                    cell.transform.localScale = new Vector3(delta, delta, delta);
                }
            }
        }
    }
    [MenuItem("GameObject/Tools/RemoveChildren")]
    public static void ClearChildren()
    {
        if (Selection.activeGameObject == null)
        {
            return;
        }
        Transform root = Selection.activeGameObject.transform;
        for (int i = root.childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(root.GetChild(i).gameObject);
        }

    }
}
