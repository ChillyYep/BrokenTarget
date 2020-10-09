using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class GameObjectMenuEx
{
    [MenuItem("GameObject/Ex/AddBoxColliderToMeshOwnerInChildren", false, priority = 0)]
    public static void AddBoxColliderToMeshOwnerInChildren()
    {
        GameObject gameObject = Selection.activeGameObject;
        foreach (var item in gameObject.GetComponentsInChildren<MeshFilter>())
        {
            var collider = item.GetComponent<Collider>();
            if (collider != null)
            {
                Component.DestroyImmediate(collider);
            }
            item.gameObject.AddComponent<BoxCollider>();
        }
    }
    [MenuItem("GameObject/Ex/AddMeshColliderToMeshOwnerInChildren", false)]
    public static void AddMeshColliderToMeshOwnerInChildren()
    {
        GameObject gameObject = Selection.activeGameObject;
        foreach (var item in gameObject.GetComponentsInChildren<MeshFilter>())
        {
            var collider = item.GetComponent<Collider>();
            if (collider != null)
            {
                Component.DestroyImmediate(collider);
            }
            item.gameObject.AddComponent<MeshCollider>();
        }
    }
    [MenuItem("GameObject/Ex/RemoveColliderToMeshOwnerInChildren", false)]
    public static void RemoveColliderToMeshOwnerInChildren()
    {
        GameObject gameObject = Selection.activeGameObject;
        foreach (var item in gameObject.GetComponentsInChildren<MeshFilter>())
        {
            var colliders = item.gameObject.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                if (collider != null)
                {
                    Component.DestroyImmediate(collider);
                }
            }
        }
    }
}
