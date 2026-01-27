using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerDeliveryCratePolicy : Policy
    {
        [SerializeField] StackerEnvironment env;

        private StackerAgent Agent => env.agent;

        public override void EvaluateReward()
        {
            var reward = 0f;

            reward += 3f * Agent.DeliveredCrates;

            // Penalidades
            if (Agent.IsCollidedWithMap) reward -= 20f;
            reward -= 0.01f * Agent.evaluationCount;
            reward -= 0.01f * Agent.CollisionCount;

            Agent.reward = reward;
        }

        public override void StartEpoch()
        {
            env.RespawnCrates();
        }

        public override void TransitionIn()
        {
            Agent.onCollideWithMapEvt += env.CompleteEpoch;
            Agent.onDeliveryCrateEvt += OnAgentDeliveryCrate;
        }

        public override void TransitionOut()
        {
            env.RespawnCrates();

            Agent.onCollideWithMapEvt -= env.CompleteEpoch;
            Agent.onDeliveryCrateEvt -= OnAgentDeliveryCrate;
        }

        private void OnAgentDeliveryCrate()
        {
            if (env.agent.DeliveredCrates >= env.CrateCount)
            {
                env.CompleteEpoch();
            }
        }
    }
}