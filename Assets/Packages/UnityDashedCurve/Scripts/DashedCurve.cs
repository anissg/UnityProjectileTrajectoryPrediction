using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class DashedCurve : MonoBehaviour
{
    public enum CurveType
    {
        Quadratic,
        Cubic
    }

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    Bezier2 quadraticBezier;
    Bezier3 cubicBezier;

    [SerializeField]
    CurveType type = CurveType.Quadratic;
    [SerializeField]
    Vector3 start = new Vector3(-1, 0, 0);
    [SerializeField]
    Vector3 handle1 = new Vector3(0, 1, 0);
    [SerializeField]
    Vector3 handle2 = new Vector3(0, 1, 0);
    [SerializeField]
    Vector3 end = new Vector3(1, 0);
    [SerializeField]
    float width = 1;
    [SerializeField]
    int subdivisions = 0;
    [SerializeField]
    float stripLenght = 0.5f;
    [SerializeField]
    float emptyStripLenght = 0.5f;

    public void setQuadraticPoints(Vector2 start, Vector2 handle1, Vector2 end)
    {
        meshFilter.mesh.Clear();

        this.start = start;
        this.handle1 = handle1;
        this.end = end;
    }

    public void setCubicPoints(Vector2 start, Vector2 handle1, Vector2 handle2, Vector2 end)
    {
        meshFilter.mesh.Clear();

        this.start = start;
        this.handle1 = handle1;
        this.handle2 = handle2;
        this.end = end;
    }

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        quadraticBezier = new Bezier2(start, handle1, end);
        cubicBezier = new Bezier3(start, handle1, handle2, end);
    }

    void Update()
    {
        if (type == CurveType.Quadratic)
        {
            quadraticBezier.p0 = start;
            quadraticBezier.p1 = handle1;
            quadraticBezier.p2 = end;
        }
        else
        {
            cubicBezier.p0 = start;
            cubicBezier.p1 = handle1;
            cubicBezier.p2 = handle2;
            cubicBezier.p3 = end;
        }

        var mesh = new Mesh();

        List<Vector2> points;
        List<Vector2> normals;

        GetDashedCurvePointsNormals(out points, out normals);

        List<Vector3> vertices;
        List<int> triangles;

        GenerateCurveVertices(points, normals, subdivisions + 2, width, out vertices, out triangles);

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        meshFilter.mesh = mesh;
    }

    private void GetDashedCurvePointsNormals(out List<Vector2> points, out List<Vector2> normals)
    {
        points = new List<Vector2>();
        normals = new List<Vector2>();
        float curveLength = 0;
        float param = 0;
        float s = 0;
        
        if (type == CurveType.Quadratic)
        {
            points.Add(start);
            normals.Add(quadraticBezier.GetQuadraticFirstDerivativeNormal(0));

            curveLength = quadraticBezier.GetQuadraticArcLength(0, 1);
            while (s <= curveLength)
            {
                s += stripLenght;
                if (curveLength <= s)
                {
                    for (int i = subdivisions; i > 0; i--)
                    {
                        param = quadraticBezier.GetQuadraticArcParameterAtLength(s - i * stripLenght / (subdivisions + 1));
                        points.Add(quadraticBezier.GetQuadraticPoint(param));
                        normals.Add(quadraticBezier.GetQuadraticFirstDerivativeNormal(param));
                    }
                    points.Add(quadraticBezier.GetQuadraticPoint(1));
                    normals.Add(quadraticBezier.GetQuadraticFirstDerivativeNormal(1));
                }
                else
                {
                    for (int i = subdivisions; i >=0 ; i--)
                    {
                        param = quadraticBezier.GetQuadraticArcParameterAtLength(s - i * stripLenght / (subdivisions + 1));
                        points.Add(quadraticBezier.GetQuadraticPoint(param));
                        normals.Add(quadraticBezier.GetQuadraticFirstDerivativeNormal(param));
                    }
                    s += emptyStripLenght;
                    if(s < curveLength)
                    {
                        param = quadraticBezier.GetQuadraticArcParameterAtLength(s);
                        points.Add(quadraticBezier.GetQuadraticPoint(param));
                        normals.Add(quadraticBezier.GetQuadraticFirstDerivativeNormal(param));
                    }
                }
            }
        }
        else
        {
            points.Add(start);
            normals.Add(cubicBezier.GetCubicFirstDerivativeNormal(0));

            curveLength = cubicBezier.GetCubicArcLength(0, 1);
            while (s <= curveLength)
            {
                s += stripLenght;
                if (curveLength <= s)
                {
                    for (int i = subdivisions; i > 0; i--)
                    {
                        param = cubicBezier.GetCubicArcParameterAtLength(s - i * stripLenght / (subdivisions + 1));
                        points.Add(cubicBezier.GetCubicPoint(param));
                        normals.Add(cubicBezier.GetCubicFirstDerivativeNormal(param));
                    }
                    points.Add(cubicBezier.GetCubicPoint(1));
                    normals.Add(cubicBezier.GetCubicFirstDerivativeNormal(1));
                }
                else
                {
                    for (int i = subdivisions; i >= 0; i--)
                    {
                        param = cubicBezier.GetCubicArcParameterAtLength(s - i * stripLenght / (subdivisions + 1));
                        points.Add(cubicBezier.GetCubicPoint(param));
                        normals.Add(cubicBezier.GetCubicFirstDerivativeNormal(param));
                    }
                }
                s += emptyStripLenght;
                if (s < curveLength)
                {
                    param = cubicBezier.GetCubicArcParameterAtLength(s);
                    points.Add(cubicBezier.GetCubicPoint(param));
                    normals.Add(cubicBezier.GetCubicFirstDerivativeNormal(param));
                }
            }
        }
    }

    void GenerateCurveVertices(List<Vector2> points, List<Vector2> normals, int pointPerStrip, float width, out List<Vector3> vertices, out List<int> triangles)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0; i < points.Count; i+= pointPerStrip)
        {
            for (int j = i; j < i + pointPerStrip; j++)
            {
                vertices.Add(points[j] - normals[j] * width / 2);
                vertices.Add(points[j]);
                vertices.Add(points[j] + normals[j] * width / 2);
                if (j != i)
                {
                    triangles.Add(3 * j);
                    triangles.Add(3 * j - 3);
                    triangles.Add(3 * j + 1);

                    triangles.Add(3 * j - 3);
                    triangles.Add(3 * j - 2);
                    triangles.Add(3 * j + 1);

                    triangles.Add(3 * j + 1);
                    triangles.Add(3 * j - 2);
                    triangles.Add(3 * j + 2);

                    triangles.Add(3 * j - 2);
                    triangles.Add(3 * j - 1);
                    triangles.Add(3 * j + 2);
                }
            }
        }
        
    }
}

