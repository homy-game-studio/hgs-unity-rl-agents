using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerPickCratePolicy : Policy
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
            Agent.onPickCrateEvt += env.CompleteEpoch;
            Agent.onCollideWithMapEvt += env.CompleteEpoch;
        }

        public override void TransitionOut()
        {
            env.RespawnCrates();

            Agent.onPickCrateEvt -= env.CompleteEpoch;
            Agent.onCollideWithMapEvt -= env.CompleteEpoch;
        }
    }
}