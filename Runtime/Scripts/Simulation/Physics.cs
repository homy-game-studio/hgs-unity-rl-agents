using System;
using UnityEngine;

namespace HGS.RLAgents.Simulation
{
    public static class Physics
    {
        public static bool Raycast(Vector2 origin, Vector2 direction, OBB2D[] obb2d, out float distance, float maxDistance = Mathf.Infinity)
        {
            distance = maxDistance;
            bool hasCollision = false;
            for (int i = 0; i < obb2d.Length; i++)
            {
                if (Raycast(origin, direction, obb2d[i], out float d, maxDistance))
                {
                    if (d < distance)
                    {
                        distance = d;
                        hasCollision = true;
                    }
                }
            }
            return hasCollision;
        }

        public static bool Raycast(Vector2 origin, Vector2 direction, OBB2D obb2d, out float distance, float maxDistance = Mathf.Infinity)
        {
            distance = maxDistance;

            // Transformar para espaço local da OBB
            Vector2 delta = origin - obb2d.center;

            Vector2 localOrigin = new Vector2(
                Vector2.Dot(delta, obb2d.Right),
                Vector2.Dot(delta, obb2d.Up)
            );

            Vector2 localDir = new Vector2(
                Vector2.Dot(direction, obb2d.Right),
                Vector2.Dot(direction, obb2d.Up)
            );

            Vector2 min = -obb2d.Half;
            Vector2 max = obb2d.Half;

            float tMin = float.NegativeInfinity;
            float tMax = float.PositiveInfinity;

            // --- X axis ---
            if (Mathf.Abs(localDir.x) < 0.0001f)
            {
                if (localOrigin.x < min.x || localOrigin.x > max.x)
                    return false;
            }
            else
            {
                float tx1 = (min.x - localOrigin.x) / localDir.x;
                float tx2 = (max.x - localOrigin.x) / localDir.x;

                float txMin = Mathf.Min(tx1, tx2);
                float txMax = Mathf.Max(tx1, tx2);

                tMin = Mathf.Max(tMin, txMin);
                tMax = Mathf.Min(tMax, txMax);
            }

            // --- Y axis ---
            if (Mathf.Abs(localDir.y) < 0.0001f)
            {
                if (localOrigin.y < min.y || localOrigin.y > max.y)
                    return false;
            }
            else
            {
                float ty1 = (min.y - localOrigin.y) / localDir.y;
                float ty2 = (max.y - localOrigin.y) / localDir.y;

                float tyMin = Mathf.Min(ty1, ty2);
                float tyMax = Mathf.Max(ty1, ty2);

                tMin = Mathf.Max(tMin, tyMin);
                tMax = Mathf.Min(tMax, tyMax);
            }

            if (tMax < 0) return false;
            if (tMin > tMax) return false;
            if (tMin > maxDistance) return false;

            distance = Mathf.Max(0, tMin);

            return true;
        }

        public static bool CheckCollision(OBB2D obb, ConvexPolygon2D polygon)
        {
            if (polygon.points == null || polygon.points.Length < 3)
                return false;

            for (int i = 0; i < polygon.points.Length; i++)
            {
                Vector2 p1 = polygon.points[i];
                Vector2 p2 = polygon.points[(i + 1) % polygon.points.Length];

                Vector2 edge = p2 - p1;
                Vector2 axis = new Vector2(-edge.y, edge.x);

                if (!OverlapOnAxis(obb, polygon, axis))
                    return false;
            }

            if (!OverlapOnAxis(obb, polygon, obb.Right))
                return false;

            if (!OverlapOnAxis(obb, polygon, obb.Up))
                return false;

            return true;
        }

        public static bool CheckCollision(in OBB2D a, in OBB2D b)
        {
            if (!OverlapOnAxis(a, b, a.Right)) return false;
            if (!OverlapOnAxis(a, b, a.Up)) return false;
            if (!OverlapOnAxis(a, b, b.Right)) return false;
            if (!OverlapOnAxis(a, b, b.Up)) return false;

            return true;
        }

        public static bool CheckCollision(in OBB2D a, OBB2D[] others)
        {
            for (int i = 0; i < others.Length; i++)
            {
                if (CheckCollision(a, others[i]))
                    return true;
            }
            return false;
        }

        static bool OverlapOnAxis(OBB2D obbA, OBB2D obbB, Vector2 axis)
        {
            ProjectOBB(obbA, axis, out float minA, out float maxA);
            ProjectOBB(obbB, axis, out float minB, out float maxB);
            return !(maxA < minB || maxB < minA);
        }

        static bool OverlapOnAxis(OBB2D obb, ConvexPolygon2D polygon, Vector2 axis)
        {
            ProjectOBB(obb, axis, out float minA, out float maxA);
            ProjectPolygon(polygon, axis, out float minB, out float maxB);
            return !(maxA < minB || maxB < minA);
        }

        static void ProjectPolygon(ConvexPolygon2D polygon, Vector2 axis, out float min, out float max)
        {
            float projection = Vector2.Dot(polygon.points[0], axis);
            min = max = projection;

            for (int i = 1; i < polygon.points.Length; i++)
            {
                projection = Vector2.Dot(polygon.points[i], axis);
                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }
        }

        static void ProjectOBB(OBB2D obb, Vector2 axis, out float min, out float max)
        {
            float centerProjection = Vector2.Dot(obb.center, axis);

            Vector2 right = obb.Right;
            Vector2 up = obb.Up;

            float halfWidth = obb.size.x * 0.5f;
            float halfHeight = obb.size.y * 0.5f;

            float radius =
                halfWidth * Mathf.Abs(Vector2.Dot(axis, right)) +
                halfHeight * Mathf.Abs(Vector2.Dot(axis, up));

            min = centerProjection - radius;
            max = centerProjection + radius;
        }
    }
}
