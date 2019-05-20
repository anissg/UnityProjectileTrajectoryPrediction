using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DashedCurve))]
public class DashedCurveEditor : Editor
{
    DashedCurve dashedCurve;
    Transform transform;

    SerializedProperty type;
    SerializedProperty start;
    SerializedProperty handle1;
    SerializedProperty handle2;
    SerializedProperty end;
    SerializedProperty width;
    SerializedProperty subdivisions;
    SerializedProperty stripLenght;
    SerializedProperty emptyStripLenght;

    void OnEnable()
    {
        dashedCurve = target as DashedCurve;
        transform = dashedCurve.transform;
        type = serializedObject.FindProperty("type");
        start = serializedObject.FindProperty("start");
        handle1 = serializedObject.FindProperty("handle1");
        handle2 = serializedObject.FindProperty("handle2");
        end = serializedObject.FindProperty("end");
        width = serializedObject.FindProperty("width");
        subdivisions = serializedObject.FindProperty("subdivisions");
        stripLenght = serializedObject.FindProperty("stripLenght");
        emptyStripLenght = serializedObject.FindProperty("emptyStripLenght");
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10f);

        GUILayout.Label("Interpolation", EditorStyles.boldLabel);
        serializedObject.Update();
        EditorGUILayout.PropertyField(type);

        GUILayout.Space(5f);

        GUILayout.Label("Control points", EditorStyles.boldLabel);
        DashedCurve.CurveType t = (DashedCurve.CurveType)type.enumValueIndex;
        switch(t)
        {
            case DashedCurve.CurveType.Quadratic:
                EditorGUILayout.PropertyField(start);
                EditorGUILayout.PropertyField(handle1);
                handle2.vector3Value = handle1.vector3Value;
                EditorGUILayout.PropertyField(end);
                break;
            case DashedCurve.CurveType.Cubic:
                EditorGUILayout.PropertyField(start);
                EditorGUILayout.PropertyField(handle1);
                EditorGUILayout.PropertyField(handle2);
                EditorGUILayout.PropertyField(end);
                break;
        }

        GUILayout.Space(5f);

        GUILayout.Label("Curve params", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(subdivisions);
        if (subdivisions.intValue < 0) subdivisions.intValue = 0;
        EditorGUILayout.PropertyField(stripLenght);
        EditorGUILayout.PropertyField(emptyStripLenght);

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        // save position in local space
        DashedCurve.CurveType t = (DashedCurve.CurveType)type.enumValueIndex;
        switch (t)
        {
            case DashedCurve.CurveType.Quadratic:
                start.vector3Value = ShowPoint(start.vector3Value);
                handle1.vector3Value = ShowPoint(handle1.vector3Value);
                handle2.vector3Value = handle1.vector3Value;
                end.vector3Value = ShowPoint(end.vector3Value);
                break;
            case DashedCurve.CurveType.Cubic:
                start.vector3Value = ShowPoint(start.vector3Value);
                handle1.vector3Value = ShowPoint(handle1.vector3Value);
                handle2.vector3Value = ShowPoint(handle2.vector3Value);
                end.vector3Value = ShowPoint(end.vector3Value);
                break;
        }
        serializedObject.ApplyModifiedProperties();

        // draw in world space
        Handles.color = Color.gray;
        Handles.DrawAAPolyLine(3,
            transform.TransformPoint(start.vector3Value),
            transform.TransformPoint(handle1.vector3Value));
        Handles.DrawAAPolyLine(3,
            transform.TransformPoint(end.vector3Value),
            transform.TransformPoint(handle2.vector3Value));

        switch (t)
        {
            case DashedCurve.CurveType.Quadratic:
                HandleExtensions.DrawBezier2(transform.TransformPoint(start.vector3Value),
                    transform.TransformPoint(end.vector3Value),
                    transform.TransformPoint(handle1.vector3Value),
                    Color.cyan, null, 5);
                break;
            case DashedCurve.CurveType.Cubic:
                Handles.DrawBezier(
                    transform.TransformPoint(start.vector3Value),
                    transform.TransformPoint(end.vector3Value),
                    transform.TransformPoint(handle1.vector3Value),
                    transform.TransformPoint(handle2.vector3Value),
                    Color.cyan, null, 5);
                break;
        }
    }

    private Vector3 ShowPoint(Vector3 point)
    {
        // move and draw points in world space
        Vector3 res_point = transform.TransformPoint(point);

        float zoom = SceneView.currentDrawingSceneView.camera.orthographicSize;

        EditorGUI.BeginChangeCheck();

        res_point = Handles.FreeMoveHandle(res_point, Quaternion.identity, zoom * .05f, Vector3.one, 
            (int aControlID, Vector3 aPosition, Quaternion aRotation, float aSize, EventType e) =>
            {
                if (e != EventType.Layout)
                {
                    if (e == EventType.Repaint)
                    {
                        if (HandleUtility.nearestControl == aControlID && GUI.enabled)
                        {
                            Handles.DotHandleCap(aControlID, aPosition, aRotation, 1.2f * aSize, e);
                        }
                        else
                            Handles.DotHandleCap(aControlID, aPosition, aRotation, aSize, e);
                    }
                }
                else
                {
                    HandleUtility.AddControl(aControlID, HandleUtility.DistanceToCircle(aPosition, aSize));
                }
            });

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(dashedCurve, "Move Point");
            EditorUtility.SetDirty(dashedCurve);
        }

        // return position in local space
        return transform.InverseTransformPoint(res_point);
    }


}
