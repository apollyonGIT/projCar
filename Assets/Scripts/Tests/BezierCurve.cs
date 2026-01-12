using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public int numberOfPoints = 100; // 曲线上点的数量

    public Transform startPoint; // 起点
    public Transform controlPoint; // 控制点
    public Transform endPoint; // 终点

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numberOfPoints;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1); // 参数t，从0到1，分割曲线
            Vector3 point = CalculateBezierPoint(t, startPoint.position, controlPoint.position, endPoint.position);
            point.z = 10;
            lineRenderer.SetPosition(i, point);
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0; // first-order terms
        p += 2 * u * t * p1; // second-order terms
        p += tt * p2; // third-order terms
        return p;
    }
}
