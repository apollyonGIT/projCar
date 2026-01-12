#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadRoot))]
public class RoadRootScript : UnityEditor.Editor
{
    RoadRoot root;

    RoadPoint add_point;

    public void OnEnable()
    {
        root = (RoadRoot)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        add_point = EditorGUILayout.ObjectField(add_point, typeof(RoadPoint),true) as RoadPoint;        //可以分配资源文件,但是注意不要这么去做

        if(GUILayout.Button("add point"))
        {
            root.AddPoint(add_point);
        }

        if (GUILayout.Button("save"))
        {
            root.Save();
        }
    }
}
#endif