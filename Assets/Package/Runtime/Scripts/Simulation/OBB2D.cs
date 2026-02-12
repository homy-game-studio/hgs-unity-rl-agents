using System;
using UnityEngine;

[Serializable]
public struct OBB2D
{
    public Vector2 center;
    public Vector2 size;
    public float rotation;

    public Vector2 Half => size * 0.5f;

    public Vector2 Right
    {
        get
        {
            float rad = rotation * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }

    public Vector2 Up
    {
        get
        {
            float rad = rotation * Mathf.Deg2Rad;
            return new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        }
    }

    public void DrawGizmos()
    {
        float rad = rotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        Vector2 right = new Vector2(cos, sin) * size.x * 0.5f;
        Vector2 up = new Vector2(-sin, cos) * size.y * 0.5f;

        Vector2 c1 = center + right + up;
        Vector2 c2 = center + right - up;
        Vector2 c3 = center - right - up;
        Vector2 c4 = center - right + up;

        Gizmos.DrawLine(c1, c2);
        Gizmos.DrawLine(c2, c3);
        Gizmos.DrawLine(c3, c4);
        Gizmos.DrawLine(c4, c1);
    }
}