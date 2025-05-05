using System.Collections.Generic;
using HGS.RLAgents.Sensors;
using UnityEngine;

namespace HGS.RLAgents.HunterVsPreySample
{
    public class HunterAgent : Agent
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Rigidbody2D myRigidbody2D;
        [SerializeField] RaySensor raySensor;
        [SerializeField] float maxSpeed = 5f;
        [SerializeField] float maxSteeringSpeed = 45f;

        private List<int> checkpoints = new List<int>();

        Vector2 _startPosition;
        Vector3 _startEulerAngles;
        float _speed = 0;
        float _steering = 0;

        protected override void Awake()
        {
            base.Awake();
            _startPosition = transform.position;
            _startEulerAngles = transform.eulerAngles;
            spriteRenderer.color = Random.ColorHSV();
        }

        protected override float[] GetInput()
        {
            var sensorInput = raySensor.Infos;

            return new float[] {
                sensorInput[0].distance,
                sensorInput[1].distance,
                sensorInput[2].distance,
                sensorInput[3].distance,
                sensorInput[4].distance,
            };
        }

        protected override void EvaluateOutput(float[] output)
        {
            reward -= 0.01f;
            _speed = Mathf.Clamp(output[0] * maxSpeed, 0, maxSpeed);
            _steering = output[1] * maxSteeringSpeed;
        }

        protected void FixedUpdate()
        {
            if (!active) return;

            myRigidbody2D.linearVelocity = transform.right * _speed;
            myRigidbody2D.MoveRotation(myRigidbody2D.rotation + _steering * Time.fixedDeltaTime);
        }

        public override void Respawn()
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        //protected override void OnStartEpoch()
        //{
        //    myRigidbody2D.linearVelocity = Vector2.zero;
        //    myRigidbody2D.rotation = 0;
        //    transform.position = _startPosition;
        //    transform.eulerAngles = _startEulerAngles;
        //}

        //private void ReachCheckpoint(int checkpoint)
        //{
        //    if (checkpoints.Contains(checkpoint)) return;

        //    reward += checkpoint;
        //    checkpoints.Add(checkpoint);

        //    if(checkpoint == 14)
        //    {
        //        CompleteEpoch();
        //    }
        //}
    }
}