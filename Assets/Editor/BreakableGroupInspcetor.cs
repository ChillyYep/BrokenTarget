using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using BreakableGroup = chenyi.BreakableGroup;
[CustomEditor(typeof(BreakableGroup))]
public class BreakableGroupInspcetor : Editor
{
    //简单的显隐控制
    private bool uvoffsetEnable = false;
    private bool foldout = false;
    private const string cutTypeHelpMsg = "此枚举值用于确定切割策略,Two代表每次将一个三角面切成面积相等两个，Three代表将一个三角面切成面积相等的三个，Four同理。可以两两组合，也可以" +
            "全部混用，但推荐使用Two或Four或二者结合，容易产生锐角三角形。\n" +
            "若切割后的三角形面积仍大于areaUnit就会继续切割，直到面积不大于areaUnit。";
    private const string distanceHelpMsg = "从交点出发，在球形范围内，每经过一个单位长度，碎块的就会变大一些";
    public override void OnInspectorGUI()
    {
        var breakableBehaviour = target as BreakableGroup;
        base.OnInspectorGUI();
        GUILayout.BeginVertical();
        uvoffsetEnable = GUILayout.Toggle(uvoffsetEnable, "启用uv偏移");
        if (uvoffsetEnable)
        {
            breakableBehaviour.boxInnerStartUV = EditorGUILayout.Vector2Field("uv偏移起点", breakableBehaviour.boxInnerStartUV);
            breakableBehaviour.boxInnerEndUV = EditorGUILayout.Vector2Field("uv偏移终点", breakableBehaviour.boxInnerEndUV);
        }
        breakableBehaviour.cutType = (CutToPiecesPerTime)EditorGUILayout.EnumPopup("单个三角面切割方案", breakableBehaviour.cutType, GUILayout.ExpandWidth(true));
        EditorGUILayout.HelpBox(cutTypeHelpMsg, MessageType.Info);

        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "受距离影响的因素");
        if (foldout)
        {
            breakableBehaviour.areaEffectByDistance = GUILayout.Toggle(breakableBehaviour.areaEffectByDistance, "启用碎块大小随距离变化的功能");
            if (breakableBehaviour.areaEffectByDistance)
            {
                breakableBehaviour.distance = EditorGUILayout.FloatField("影响距离单位", breakableBehaviour.distance);
                EditorGUILayout.HelpBox(distanceHelpMsg, MessageType.Info);
                breakableBehaviour.limitAreaUnit = GUILayout.Toggle(breakableBehaviour.limitAreaUnit, "限制碎块大小范围");
                if (breakableBehaviour.limitAreaUnit)
                {
                    breakableBehaviour.minAreaUnit = EditorGUILayout.FloatField("最小面积", breakableBehaviour.minAreaUnit);
                    breakableBehaviour.maxAreaUnit = EditorGUILayout.FloatField("最大面积", breakableBehaviour.maxAreaUnit);
                    if (breakableBehaviour.minAreaUnit > breakableBehaviour.maxAreaUnit)//限制minAreaUnit<maxAreaUnit
                    {
                        breakableBehaviour.minAreaUnit = EditorGUILayout.FloatField("最小面积", breakableBehaviour.maxAreaUnit);
                        breakableBehaviour.maxAreaUnit = EditorGUILayout.FloatField("最大面积", breakableBehaviour.maxAreaUnit);
                    }
                }
                else
                {
                    breakableBehaviour.minAreaUnit = breakableBehaviour.maxAreaUnit = breakableBehaviour.areaUnit;
                }
            }
            else
            {
                breakableBehaviour.boxInnerStartUV = Vector2.zero;
                breakableBehaviour.boxInnerEndUV = Vector2.one;
            }
            breakableBehaviour.forceEffectedByDistance = GUILayout.Toggle(breakableBehaviour.forceEffectedByDistance, "启用碎块受力随距离变化的功能");
            if (breakableBehaviour.forceEffectedByDistance)
            {
                breakableBehaviour.forceEffectRangeUnit = EditorGUILayout.FloatField("受力距离影响单位", breakableBehaviour.forceEffectRangeUnit);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.EndVertical();
    }
}
