using System;
using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents.Sensors
{
    [Serializable]
    public struct Raycast2DSensorInfo
    {
        public float distance;
        public string tag;
        public float tagIndex;
        public Vector2 position;

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

        public Raycast2DSensorInfo[] Infos
        {
            private set { _infos = value; }
            get
            {
                if (_infos == null)
                {
                    _infos = new Raycast2DSensorInfo[sensorCount];
                }
                return _infos;
            }
        }

        void Awake()
        {
            _directions = CreateDirections();
        }

        public Vector2[] CreateDirections()
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

        public void ExecuteRayInfo(Vector2 direction, out Raycast2DSensorInfo info)
        {
            var dir = transform.TransformDirection(direction);
            var hit = Physics2D.Raycast(transform.position, dir, sensorLength, detectionLayer);

            info.distance = hit.collider != null ? hit.distance / sensorLength : 1f;
            info.tag = hit.collider != null ? hit.collider.tag : "";
            info.tagIndex = hit.collider != null ? tagList.IndexOf(hit.collider.tag)  : -1f;
            info.position = hit.point;
        }

        void FixedUpdate()
        {
            if (!Application.isPlaying) return;

            for (int i = 0; i < _directions.Length; i++)
            {
                ExecuteRayInfo(_directions[i], out Infos[i]);
            }
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            var directions = CreateDirections();

            for (int i = 0; i < directions.Length; i++)
            {
                Raycast2DSensorInfo info;
                ExecuteRayInfo(directions[i], out info);

                var dir = transform.TransformDirection(directions[i]);
                var distance = info.distance * sensorLength;

                if (info.IsTouched)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(info.position, 0.1f);
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