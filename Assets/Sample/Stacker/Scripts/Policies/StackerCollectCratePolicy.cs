using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerCollectCratePolicy : Policy
    {
        [SerializeField] List<Vector2> cratePositions;
        [SerializeField] float generationsToChangePosition = 10;
        [SerializeField] StackerEnvironment env;

        int _index = -1;

        private StackerAgent Agent => env.agent;

        public override void EvaluateReward()
        {
            var reward = 0f;

            // Aproximar-se e/ou pegar caixa
            if (Agent.IsPickedCrate) reward += 2f;

            // Aproximar-se do checkpoint com uma caixa e/ou soltar
            reward += 3f * Agent.DeliveredCrates;
            reward += (10f - Agent.MinDistanceToCheckpoint) / 10f;

            // Penalidades
            if (Agent.IsCollidedWithMap) reward -= 20f;
            reward -= 0.25f * (Agent.TimeWithoutCrate / env.MaxEpochDuration);
            reward -= 0.1f * (Agent.IdleTime / env.MaxEpochDuration);

            Agent.reward = reward;
        }

        public override void StartEpoch()
        {
            base.StartEpoch();
            if (env.Epoch % generationsToChangePosition == 0)
            {
                _index++;
                if (_index >= cratePositions.Count) _index = 0;
            }
            env.RespawnCrate(0);
            env.SetCratePosition(0, (Vector2)env.transform.position + cratePositions[_index]);
        }

        public override void TransitionIn()
        {
            env.ToggleCheckpoint(true);
            env.HideCrates();
            env.ShowCrate(0);

            Agent.onDeliveryCrateEvt += env.CompleteEpoch;
            Agent.onCollideWithMapEvt += env.CompleteEpoch;
        }

        public override void TransitionOut()
        {
            env.ToggleCheckpoint(false);
            env.RespawnCrates();

            Agent.onDeliveryCrateEvt -= env.CompleteEpoch;
            Agent.onCollideWithMapEvt -= env.CompleteEpoch;
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