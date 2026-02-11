using System;
using UnityEngine;

namespace HGS.RLAgents.Sensors
{
    [Serializable]
    public struct Intersection2DSensorInfo
    {
        public float distance;
        public float percentage;
    }

    [ExecuteInEditMode]
    public class IntersectionSensor2D : MonoBehaviour
    {
        [SerializeField] Collider2D origin;
        [SerializeField] Collider2D destination;
        [SerializeField] int samplesX = 5;
        [SerializeField] int samplesY = 3;
        [SerializeField] bool showGizmos = true;

        private Intersection2DSensorInfo _info;
        public Intersection2DSensorInfo Info => _info;

        public void Sense()
        {
            if (origin == null || destination == null)
                return;

            _info.distance = GetDistance();
            _info.percentage = GetPercent();
        }

        float GetDistance()
        {
            return Vector2.Distance(
                origin.bounds.center,
                destination.bounds.center
            );
        }

        float GetPercent()
        {
            Bounds b = origin.bounds;

            int inside = 0;
            int total = samplesX * samplesY;

            for (int x = 0; x < samplesX; x++)
            {
                for (int y = 0; y < samplesY; y++)
                {
                    float px = Mathf.Lerp(b.min.x, b.max.x, (x + 0.5f) / samplesX);
                    float py = Mathf.Lerp(b.min.y, b.max.y, (y + 0.5f) / samplesY);

                    Vector2 point = new Vector2(px, py);

                    if (destination.OverlapPoint(point))
                        inside++;
                }
            }

            return (float)inside / total;
        }

        private void OnDrawGizmos()
        {
            // progress bar with percentage
            if (!showGizmos || origin == null || destination == null)
                return;
            float percentage = GetPercent();
            Vector3 pos = destination.bounds.center;
            Vector3 size = new Vector3(1f, 0.1f, 0f);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(pos, size);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(
                pos - new Vector3((1f - percentage) / 2f, 0f, 0f),
                new Vector3(percentage, size.y, size.z)
            );
        }
    }
}