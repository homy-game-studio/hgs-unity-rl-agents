using System;
using System.Collections.Generic;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.Sensors;
using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    public class DriverAgent : Agent
    {
        [SerializeField] MeshRenderer[] bodyParts;
        [SerializeField] RaySensor raySensor;
        [SerializeField] DriverPhysics driverPhysics;
        [SerializeField] int totalCheckpoints = 10;

        Vector3 _startPosition;
        Quaternion _startRotation;

        public List<int> Checkpoints { get; private set; } = new List<int>();
        public bool IsCollidedWithMap { get; private set; } = false;
        public bool IsCompletedMap { get; private set; } = false;

        public Action onCollideWithMapEvt;
        public Action onCompleteMapEvt;

        protected override void Awake()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            base.Awake();
        }

        public override void SetGenome(int id, Genome genome)
        {
            base.SetGenome(id, genome);
            var color = new Color(
                (genome.GetGene(0) + 1f) / 2f,
                (genome.GetGene(1) + 1f) / 2f,
                (genome.GetGene(2) + 1f) / 2f
            );

            for (int i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].material.color = color;
            }
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
                driverPhysics.ForwardVelocity
            };
        }

        protected override void EvaluateOutput(float[] output)
        {
            if (output[0] > 0)
            {
                driverPhysics.aceleration = output[0];
                driverPhysics.breaking = 0;
            }
            else
            {
                driverPhysics.aceleration = 0;
                driverPhysics.breaking = output[0] * -1f;
            }

            driverPhysics.steer = output[1];
        }

        private void OnCollisionEnter(Collision collision)
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

            if (Checkpoints.Count == totalCheckpoints)
            {
                Stop();
                IsCompletedMap = true;
                onCompleteMapEvt?.Invoke();
            }
        }

        private void OnTriggerEnter(Collider collision)
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
            driverPhysics.Stop();
        }

        public override void Respawn()
        {
            Stop();
            driverPhysics.aceleration = 0;
            driverPhysics.breaking = 0;
            driverPhysics.steer = 0;
            transform.position = _startPosition;
            transform.rotation = _startRotation;
            IsCompletedMap = false;
            IsCollidedWithMap = false;
            Checkpoints.Clear();
        }
    }
}