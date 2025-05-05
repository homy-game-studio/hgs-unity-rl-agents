using System;
using HGS.RLAgents.Sensors;
using UnityEngine;

namespace HGS.RLAgents.FollowTargetSample
{
    public class FollowTargetAgent : Agent
    {
        [SerializeField] RaySensor sensor;
        [SerializeField] Transform target;
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] SpriteRenderer targetSpriteRenderer;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float maxDistance = 11f;

        Vector2 _dir = Vector2.zero;
        Vector2 _startPosition;

        public float DistanceToTarget { get; set; }
        public bool IsReachedTarget => DistanceToTarget <= 0.02f;

        public Action onReachTarget;

        protected override void Awake()
        {
            base.Awake();
            var randomColor = UnityEngine.Random.ColorHSV();

            _startPosition = transform.position;
            spriteRenderer.color = randomColor;
            targetSpriteRenderer.color = randomColor;

            randomColor.a = 0.5f;
            lineRenderer.material.color = randomColor;
        }

        protected override float[] GetInput()
        {
            var input = sensor.Infos;
            return new float[] {
                input[0].distance,
                input[1].distance,
                input[2].distance,
                input[3].distance,
                input[4].distance,
                input[5].distance,
                input[6].distance,
                input[7].distance,
                input[8].distance,
                input[9].distance,
                input[10].distance,
                input[11].distance,
                input[12].distance,
                input[13].distance,
                input[14].distance,
            };
        }

        protected override void EvaluateOutput(float[] output)
        {
            _dir.x = Mathf.Clamp(output[0], -1f, 1f);
            _dir.y = Mathf.Clamp(output[1], -1f, 1f);
        }

        protected override void Update()
        {
            base.Update();
            if (!active) return;
            transform.Translate(_dir.normalized * Time.deltaTime * moveSpeed);
            transform.position = new Vector2(
                Mathf.Clamp(transform.position.x, -maxDistance, maxDistance),
                Mathf.Clamp(transform.position.y, -maxDistance, maxDistance)
            );

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.position);

            DistanceToTarget = Vector2.Distance(transform.position, target.position);
        }

        public override void Respawn()
        {
            _dir = Vector2.zero;
            transform.position = _startPosition;
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}