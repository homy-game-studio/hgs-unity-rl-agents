using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerPolicy : Policy
    {
        [SerializeField] StackerEnvironment env;
        [SerializeField] int maxEvaluations = 200;

        private StackerAgent Agent => env.agent;

        public override void EvaluateReward()
        {
            var reward = 0f;

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
        }

        public override void TransitionOut()
        {
            env.RespawnCrates();

            Agent.onCollideWithMapEvt -= env.CompleteEpoch;
        }
    }
}