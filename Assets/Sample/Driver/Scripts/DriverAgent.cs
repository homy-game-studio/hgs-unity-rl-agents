using System;
using System.Collections.Generic;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.Sensors;
using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    public class DriverAgent : Agent
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Rigidbody2D myRigidbody2D;
        [SerializeField] RaySensor raySensor;
        [SerializeField] float maxSpeed = 5f;
        [SerializeField] float maxSteeringSpeed = 45f;

        Vector2 _startPosition;
        Vector3 _startEulerAngles;

        public List<int> Checkpoints { get; private set; } = new List<int>();
        public float Speed { get; private set; } = 0;
        public float Steering { get; private set; } = 0;
        public bool IsCollidedWithMap { get; private set; } = false;
        public bool IsCompletedMap { get; private set; } = false;

        public Action onCollideWithMapEvt;
        public Action onCompleteMapEvt;

        protected override void Awake()
        {
            base.Awake();
            _startPosition = transform.position;
            _startEulerAngles = transform.eulerAngles;
        }

        public override void SetCromossome(Cromossome cromossome)
        {
            base.SetCromossome(cromossome);
            spriteRenderer.color = new Color(
                (cromossome.GetGene(0) + 1f) / 2f,
                (cromossome.GetGene(1) + 1f) / 2f,
                (cromossome.GetGene(2) + 1f) / 2f
            );
        }

        protected override float[] CollectObservations()
        {
            raySensor.Sense();

            return new float[] {
                raySensor.Infos[0].distance,
                raySensor.Infos[1].distance,
                raySensor.Infos[2].distance,
                raySensor.Infos[3].distance,
                raySensor.Infos[4].distance,
                raySensor.Infos[5].distance,
                raySensor.Infos[6].distance,
                raySensor.Infos[7].distance,
            };
        }

        protected override void EvaluateOutput(float[] output)
        {
            Speed = Mathf.Clamp(output[0] * maxSpeed, 0, maxSpeed);
            Steering = output[1] * maxSteeringSpeed;
        }

        protected void FixedUpdate()
        {
            if (!active) return;

            myRigidbody2D.linearVelocity = transform.right * Speed;
            myRigidbody2D.MoveRotation(myRigidbody2D.rotation + Steering * Time.fixedDeltaTime);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Map")) return;

            Stop();
            IsCollidedWithMap = true;
            onCollideWithMapEvt?.Invoke();
        }

        private void ReachCheckpoint(int checkpoint)
        {
            if (Checkpoints.Contains(checkpoint)) return;

            Checkpoints.Add(checkpoint);

            if (Checkpoints.Count == 14)
            {
                Stop();
                IsCompletedMap = true;
                onCompleteMapEvt?.Invoke();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Checkpoint"))
            {
                var checkpointTxt = collision.gameObject.name.Replace("Checkpoint", "");
                var checkpoint = int.Parse(checkpointTxt);
                ReachCheckpoint(checkpoint);
            }
        }

        public override void Stop()
        {
            myRigidbody2D.linearVelocity = Vector2.zero;
            myRigidbody2D.rotation = 0;
            Speed = 0;
            Steering = 0;
        }

        public override void Respawn()
        {
            Stop();
            transform.position = _startPosition;
            transform.eulerAngles = _startEulerAngles;
            IsCompletedMap = false;
            IsCollidedWithMap = false;
            Checkpoints.Clear();
        }
    }
}