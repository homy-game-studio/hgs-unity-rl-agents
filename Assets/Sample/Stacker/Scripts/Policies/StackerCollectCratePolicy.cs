using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerCollectCratePolicy : Policy
    {
        [SerializeField] StackerEnvironment env;
        [SerializeField] int maxEvaluations = 200;

        private StackerAgent Agent => env.agent;

        public override void EvaluateReward()
        {
            var reward = 0f;

            // Aproximar-se e/ou pegar caixa
            if (Agent.IsPickedCrate) reward += 1f;
            reward += 0.2f * (1f - Agent.MinDistanceToCrate);

            // Aproximar-se do checkpoint com uma caixa e/ou soltar
            reward += 0.5f * (10f - Agent.MinDistanceToCheckpoint);
            reward += 2f * Agent.CollectedCrates;

            // Penalidades
            if (Agent.IsCollidedWithMap) reward -= 20f;
            reward -= 0.1f * (float)Agent.evaluationCount / maxEvaluations;
            reward -= 0.1f * (Agent.IdleTime / env.MaxEpochDuration);

            Agent.reward = reward;
        }

        public override void StartEpoch()
        {
            env.RespawnCrates();
        }

        public override void TransitionIn()
        {
            Agent.onCollideWithMapEvt += env.CompleteEpoch;
            Agent.onCollectCrateEvt += env.CompleteEpoch;
        }

        public override void TransitionOut()
        {
            env.RespawnCrates();

            Agent.onCollideWithMapEvt -= env.CompleteEpoch;
            Agent.onCollectCrateEvt -= env.CompleteEpoch;
        }
    }
}