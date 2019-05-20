using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class HandleExtensions
{
    public static void DrawBezier2(Vector3 startPosition, Vector3 endPosition, Vector3 control, Color color, Texture2D texture, float width)
    {
        Bezier2 curve = new Bezier2(startPosition, control, endPosition);
        Vector2 lineStart = startPosition;
        Handles.color = color;
        for (int i = 1; i <= 32; i++)
        {
            Vector2 lineEnd = curve.GetQuadraticPoint(i / (float)16);
            Handles.DrawAAPolyLine(texture, width, lineStart, lineEnd);
            lineStart = lineEnd;
        }
    }
}
