using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    public class DriverSensors : MonoBehaviour
    {
        [SerializeField] float sensorLength = 5f;
        [SerializeField] LayerMask detectionLayer;
        [SerializeField] bool showGizmos = true;

        readonly Vector2[] directions = new Vector2[]
        {
        Vector2.right,
        (Vector2.right + Vector2.up).normalized,
        (Vector2.right + Vector2.down).normalized,
        Vector2.up,
        Vector2.down
        };

        float[] _distances;
        public float[] Distances => _distances;

        void Awake()
        {
            _distances = new float[directions.Length];
        }

        void FixedUpdate()
        {
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2 dir = transform.TransformDirection(directions[i]);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, sensorLength, detectionLayer);

                if (hit.collider != null)
                {
                    _distances[i] = hit.distance / sensorLength;
                }
                else
                {
                    _distances[i] = 1f;
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmos || _distances == null || _distances.Length == 0) return;

            Gizmos.color = Color.cyan;

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2 dir = transform.TransformDirection(directions[i]);
                float distance = (_distances != null && _distances.Length > i) ? _distances[i] * sensorLength : sensorLength;

                Gizmos.DrawRay(transform.position, dir * distance);
            }
        }
    }
}