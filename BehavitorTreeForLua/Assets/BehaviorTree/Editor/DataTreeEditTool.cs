using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

public class DataTreeEditTool
{
    private static Dictionary<int, List<string>> dic = new Dictionary<int, List<string>>();

    private static string curpath;

    private static Matrix4x4 prevGuiMatrix;

    private static Rect groupRect = default(Rect);


    public static GUIStyle GUILabelType(TextAnchor anchor = TextAnchor.UpperCenter)
    {
        GUIStyle labelstyle = GUI.skin.GetStyle("Label");
        labelstyle.alignment = anchor;
        return labelstyle;
    }

    public static T GUIobject_CaneditArea<T>(string content,T data, bool surtecancel = true, Action enableFunc = null) where T:UnityEngine.Object
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(content);
        data = (T)EditorGUILayout.ObjectField(data, typeof(T),false);

        if (GUILayout.Button("确定"))
        {
            if (enableFunc != null)
                enableFunc();
        }

        GUILayout.EndHorizontal();
        return data;
    }

    public static string GUILabel_CaneditArea(string content,string defualttext,bool surtecancel= true,Action enableFunc =null)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(content);
        string str= GUILayout.TextField(defualttext);

        if (GUILayout.Button("确定"))
        {
            if (enableFunc != null)
                enableFunc();
        }

        GUILayout.EndHorizontal();
        return str;
    }

    public static void CreateSplit()
    {
        GUILayout.Label("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    }

    public static Vector2 TopLeft(Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin);
    }

    public static Rect ScaleSizeBy(Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale;
        result.xMax *= scale;
        result.yMin *= scale;
        result.yMax *= scale;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }

    public static Rect Begin(Rect screenCoordsArea, float zoomScale)
    {
        GUI.EndGroup();
        Rect rect = ScaleSizeBy(screenCoordsArea, 1f / zoomScale, TopLeft(screenCoordsArea));
        rect.y += 21f;
        GUI.BeginGroup(rect);
        prevGuiMatrix = GUI.matrix;
        Matrix4x4 lhs = Matrix4x4.TRS(TopLeft(rect), Quaternion.identity, Vector3.one);
        Vector3 one = Vector3.one;
        one.y = zoomScale;
        one.x = zoomScale;
        Matrix4x4 rhs = Matrix4x4.Scale(one);
        GUI.matrix = lhs * rhs * lhs.inverse * GUI.matrix;
        return rect;
    }

    public static void End()
    {
        GUI.matrix = prevGuiMatrix;
        GUI.EndGroup();
        groupRect.y = 21f;
        groupRect.width = (float)Screen.width;
        groupRect.height = (float)Screen.height;
        GUI.BeginGroup(groupRect);
    }
}