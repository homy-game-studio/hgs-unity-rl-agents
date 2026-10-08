using System;
using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents.Sensors
{
    [Serializable]
    public struct RaycastSensorInfo
    {
        public float distance;
        public float[] tags;
        public Vector3 position;

        public void SetTag(int index, float value)
        {
            tags[index] = value;
        }

        public bool IsTouched => distance < 1f;
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
        [SerializeField] Vector3 offset;

        Vector3[] _directions;
        RaycastSensorInfo[] _infos;

        public RaycastSensorInfo[] Infos => _infos;

        void Awake()
        {
            _infos = CreateInfos();
            _directions = CreateDirections();
        }

        private RaycastSensorInfo[] CreateInfos()
        {
            var infos = new RaycastSensorInfo[sensorCount];
            for (int i = 0; i < sensorCount; i++)
            {
                infos[i].tags = new float[tagList?.Count ?? 0];
            }
            return infos;
        }

        private Vector3[] CreateDirections()
        {
            var directions = new Vector3[sensorCount];
            float angleStep = sensorAngle / (sensorCount - 1);

            for (int i = 0; i < sensorCount; i++)
            {
                float angle = sensorStartAngle + angleStep * i;
                float rad = angle * Mathf.Deg2Rad;

                // Direção no plano XZ
                float x = Mathf.Sin(rad);
                float z = Mathf.Cos(rad);
                directions[i] = new Vector3(x, 0f, z); // y = 0 para plano horizontal
            }

            return directions;
        }

        public void ExecuteRay(Vector3 direction, ref RaycastSensorInfo info)
        {
            var origin = transform.position + transform.TransformDirection(offset);
            var dir = transform.TransformDirection(direction);

            if (Physics.Raycast(origin, dir, out RaycastHit hit, sensorLength, detectionLayer))
            {
                info.distance = hit.distance / sensorLength;
                info.position = hit.point;

                for (int i = 0; i < tagList.Count; i++)
                    info.tags[i] = hit.collider.CompareTag(tagList[i]) ? 1f : 0f;
            }
            else
            {
                info.distance = 1f;
                info.position = origin + dir * sensorLength;

                for (int i = 0; i < tagList.Count; i++)
                    info.tags[i] = 0f;
            }
        }

        public void Sense()
        {
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

                var origin = transform.position + transform.TransformDirection(offset);
                var dir = transform.TransformDirection(directions[i]);
                var distance = infos[i].distance * sensorLength;

                Gizmos.color = infos[i].IsTouched ? Color.green : Color.cyan;
                Gizmos.DrawRay(origin, dir * distance);

                if (infos[i].IsTouched)
                    Gizmos.DrawWireSphere(infos[i].position, 0.1f);
            }
        }
    }
}