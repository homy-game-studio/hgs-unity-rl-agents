using System;
using UnityEngine;

namespace HGS.RLAgents
{
    [Serializable]
    public struct ConvexPolygon2D
    {
        public Vector2[] points;

        public void DrawGizmos()
        {
            if (points == null || points.Length < 3) return;

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 p1 = points[i];
                Vector2 p2 = points[(i + 1) % points.Length];

                Gizmos.DrawLine(p1, p2);

                Vector2 edge = p2 - p1;
                Vector2 normal = new Vector2(-edge.y, edge.x).normalized;

                Vector2 mid = (p1 + p2) * 0.5f;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(mid, mid + normal * 0.5f);
            }
        }
    }
}
