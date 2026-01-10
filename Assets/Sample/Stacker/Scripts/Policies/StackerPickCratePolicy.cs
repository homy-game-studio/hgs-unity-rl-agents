using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerPickCratePolicy : Policy
    {
        [SerializeField] List<Vector2> cratePositions;
        [SerializeField] StackerEnvironment env;

        private StackerAgent Agent => env.agent;

        public override void EvaluateReward()
        {
            var reward = 0f;

            // Aproximar-se
            reward += Agent.PickedCrateCount;

            // Penalidades
            if (Agent.IsCollidedWithMap) reward -= 20f;
            reward -= 0.1f * (Agent.IdleTime / env.MaxEpochDuration);
            reward -= 0.1f * Agent.AvgTimeToPickCrate;
            reward -= 0.1f * (Agent.TimeWithoutCrate / env.MaxEpochDuration);

            Agent.reward = reward;
        }

        public override void StartEpoch()
        {
            base.StartEpoch();
            env.RespawnCrate(0);
            env.SetCratePosition(0, (Vector2)env.transform.position + cratePositions[0]);
        }

        public override void TransitionIn()
        {
            env.ToggleCheckpoint(false);
            env.HideCrates();
            env.ShowCrate(0);
            Agent.onPickCrateEvt += OnAgentPickCrate;
            Agent.onCollideWithMapEvt += env.CompleteEpoch;
        }

        public override void TransitionOut()
        {
            env.ToggleCheckpoint(true);
            env.RespawnCrates();

            Agent.onPickCrateEvt += OnAgentPickCrate;
            Agent.onCollideWithMapEvt -= env.CompleteEpoch;
        }

        private void OnAgentPickCrate()
        {
            var index = Agent.PickedCrateCount % cratePositions.Count;
            env.RespawnCrate(0);
            env.SetCratePosition(0, (Vector2)env.transform.position + cratePositions[index]);
            Agent.Drop();
        }

        private void OnDrawGizmos()
        {
            foreach (var pos in cratePositions)
            {
                Gizmos.DrawWireCube(pos, Vector3.one);
            }
        }
    }
}