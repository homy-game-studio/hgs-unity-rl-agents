using System;
using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents.Sensors
{
    [Serializable]
    public struct Raycast2DSensorInfo
    {
        public float distance;
        public float[] tags;
        public Vector2 position;

        public void SetTag(int index, float value)
        {
            tags[index] = value;
        }

        public bool IsTouched => distance < 1;
    }

    [ExecuteInEditMode]
    public class RaySensor : MonoBehaviour
    {
        [SerializeField] float sensorLength = 5f;
        [SerializeField] int sensorCount = 4;
        [SerializeField] float sensorStartAngle = 0f;
        [SerializeField] float sensorAngle = 0f;
        [SerializeField] LayerMask detectionLayer;
        [SerializeField] bool showGizmos = true;
        [SerializeField] List<string> tagList;

        Vector2[] _directions;
        Raycast2DSensorInfo[] _infos;

        public Raycast2DSensorInfo[] Infos => _infos;

        void Awake()
        {
            _infos = CreateInfos();
            _directions = CreateDirections();
        }

        private Raycast2DSensorInfo[] CreateInfos()
        {
            var infos = new Raycast2DSensorInfo[sensorCount];
            for (int i = 0; i < sensorCount; i++)
            {
                infos[i].tags = new float[tagList.Count];
            }
            return infos;
        }

        private Vector2[] CreateDirections()
        {
            var directions = new Vector2[sensorCount];
            var angleStep = sensorAngle / (sensorCount - 1);

            for (int i = 0; i < sensorCount; i++)
            {
                float angle = angleStep * i;
                float x = Mathf.Cos((sensorStartAngle + angle) * Mathf.Deg2Rad);
                float y = Mathf.Sin((sensorStartAngle + angle) * Mathf.Deg2Rad);
                directions[i] = new Vector2(x, y);
            }

            return directions;
        }

        public void ExecuteRay(Vector2 direction, ref Raycast2DSensorInfo info)
        {
            var dir = transform.TransformDirection(direction);
            var hit = Physics2D.Raycast(transform.position, dir, sensorLength, detectionLayer);

            info.distance = hit.collider != null ? hit.distance / sensorLength : 1f;
            for (int i = 0; i < tagList.Count; i++)
            {
                if (hit.collider != null && hit.collider.CompareTag(tagList[i]))
                {
                    info.tags[i] = 1f;
                }
                else
                {
                    info.tags[i] = 0f;
                }
            }
            info.position = hit.point;
        }

        void FixedUpdate()
        {
            if (!Application.isPlaying) return;

            for (int i = 0; i < _directions.Length; i++)
            {
                ExecuteRay(_directions[i], ref _infos[i]);
            }
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            var infos = CreateInfos();
            var directions = CreateDirections();

            for (int i = 0; i < directions.Length; i++)
            {
                ExecuteRay(directions[i], ref infos[i]);

                var dir = transform.TransformDirection(directions[i]);
                var distance = infos[i].distance * sensorLength;

                if (infos[i].IsTouched)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(infos[i].position, 0.1f);
                }
                else
                {
                    Gizmos.color = Color.cyan;
                }

                Gizmos.DrawRay(transform.position, dir * distance);
            }
        }
    }
}