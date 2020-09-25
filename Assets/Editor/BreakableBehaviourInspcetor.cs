using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using BreakableBehaviour = chenyi.BreakableObjBehaviour;
[CustomEditor(typeof(BreakableBehaviour))]
public class BreakableBehaviourInspcetor : Editor
{
    private bool uvoffsetEnable = false;
    public override void OnInspectorGUI()
    {
        var breakableBehaviour = target as BreakableBehaviour;
        base.OnInspectorGUI();
        GUILayout.BeginVertical();
        breakableBehaviour.areaEffectByDistance = GUILayout.Toggle(breakableBehaviour.areaEffectByDistance, "是否启用碎块大小随距离变化的功能");
        if (breakableBehaviour.areaEffectByDistance)
        {
            breakableBehaviour.distance = EditorGUILayout.FloatField("影响距离单位", breakableBehaviour.distance);
        }
        uvoffsetEnable = GUILayout.Toggle(uvoffsetEnable, "是否启用uv偏移");
        if (uvoffsetEnable)
        {
            breakableBehaviour.boxInnerStartUV = EditorGUILayout.Vector2Field("uv偏移起点", breakableBehaviour.boxInnerStartUV);
            breakableBehaviour.boxInnerEndUV = EditorGUILayout.Vector2Field("uv偏移终点", breakableBehaviour.boxInnerEndUV);
        }
        else
        {
            breakableBehaviour.boxInnerStartUV = Vector2.zero;
            breakableBehaviour.boxInnerEndUV = Vector2.one;
        }
        breakableBehaviour.forceEffectedByDistance = GUILayout.Toggle(breakableBehaviour.forceEffectedByDistance, "是否启用碎块受力随距离变化的功能");
        if(breakableBehaviour.forceEffectedByDistance)
        {
            breakableBehaviour.forceEffectRangeUnit = EditorGUILayout.FloatField("受力距离影响单位", breakableBehaviour.forceEffectRangeUnit);
        }
        GUILayout.EndVertical();
    }
}
