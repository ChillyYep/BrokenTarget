using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BrokenSys;

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
    private const string forceFactorMsg = "力的衰减因子越大，力与距离的关系曲线越陡峭";
    public override void OnInspectorGUI()
    {
        var breakableGroup = target as BreakableGroup;
        base.OnInspectorGUI();
        GUILayout.BeginVertical();
        uvoffsetEnable = GUILayout.Toggle(uvoffsetEnable, "启用uv偏移");
        if (uvoffsetEnable)
        {
            breakableGroup.boxInnerStartUV = EditorGUILayout.Vector2Field("uv偏移起点", breakableGroup.boxInnerStartUV);
            breakableGroup.boxInnerEndUV = EditorGUILayout.Vector2Field("uv偏移终点", breakableGroup.boxInnerEndUV);
        }
        breakableGroup.cutType = (CutToPiecesPerTime)EditorGUILayout.EnumPopup("单个三角面切割方案", breakableGroup.cutType, GUILayout.ExpandWidth(true));
        EditorGUILayout.HelpBox(cutTypeHelpMsg, MessageType.Info);

        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "受距离影响的因素");
        if (foldout)
        {
            breakableGroup.areaEffectByDistance = GUILayout.Toggle(breakableGroup.areaEffectByDistance, "启用碎块大小随距离变化的功能");
            if (breakableGroup.areaEffectByDistance)
            {
                breakableGroup.distance = EditorGUILayout.FloatField("影响距离单位", breakableGroup.distance);
                EditorGUILayout.HelpBox(distanceHelpMsg, MessageType.Info);
                breakableGroup.limitAreaUnit = GUILayout.Toggle(breakableGroup.limitAreaUnit, "限制碎块大小范围");
                if (breakableGroup.limitAreaUnit)
                {
                    breakableGroup.minAreaUnit = EditorGUILayout.FloatField("最小面积", breakableGroup.minAreaUnit);
                    breakableGroup.maxAreaUnit = EditorGUILayout.FloatField("最大面积", breakableGroup.maxAreaUnit);
                    if (breakableGroup.minAreaUnit > breakableGroup.maxAreaUnit)//限制minAreaUnit<maxAreaUnit
                    {
                        breakableGroup.minAreaUnit = EditorGUILayout.FloatField("最小面积", breakableGroup.maxAreaUnit);
                        breakableGroup.maxAreaUnit = EditorGUILayout.FloatField("最大面积", breakableGroup.maxAreaUnit);
                    }
                }
                else
                {
                    breakableGroup.minAreaUnit = breakableGroup.maxAreaUnit = breakableGroup.areaUnit;
                }
            }
            else
            {
                breakableGroup.boxInnerStartUV = Vector2.zero;
                breakableGroup.boxInnerEndUV = Vector2.one;
            }
            breakableGroup.forceEffectedByDistance = GUILayout.Toggle(breakableGroup.forceEffectedByDistance, "启用碎块受力随距离变化的功能");
            if (breakableGroup.forceEffectedByDistance)
            {
                breakableGroup.forceEffectRangeUnit = EditorGUILayout.FloatField("受力距离影响单位", breakableGroup.forceEffectRangeUnit);
                breakableGroup.forceFactor=EditorGUILayout.FloatField("力衰减因子", breakableGroup.forceFactor);
                EditorGUILayout.HelpBox(forceFactorMsg, MessageType.Info);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.EndVertical();
    }
}
