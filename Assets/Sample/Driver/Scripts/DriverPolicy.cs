using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    public class DriverPolicy : Policy
    {
        [SerializeField] Environment env;
        [SerializeField] DriverAgent agent;

        void Awake()
        {
            agent.onCollideWithMapEvt += env.CompleteEpoch;
            agent.onCompleteMapEvt += env.CompleteEpoch;
        }

        public override void StartEpoch() { }
        public override void FinishEpoch() { }

        public override void EvaluateReward()
        {
            float reward = 0;
            float maxEvaluations = env.MaxEpochDuration / agent.evaluateInterval;

            reward += agent.Checkpoints.Count;
            reward -= 0.1f * (agent.evaluationCount / maxEvaluations);

            if (agent.IsCollidedWithMap)
            {
                reward -= 5f;
            }

            if (agent.IsCompletedMap)
            {
                reward += 4;
            }

            agent.reward = reward;
        }
    }
}