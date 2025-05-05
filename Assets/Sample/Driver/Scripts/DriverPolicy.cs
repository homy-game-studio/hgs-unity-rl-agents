using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    public class DriverPolicy : Policy
    {
        [SerializeField] Environment env;
        [SerializeField] DriverAgent agent;
        [SerializeField] int maxEvaluations = 200;

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

            reward += agent.Checkpoints.Count;
            reward -= (float)agent.evaluationCount / (float)maxEvaluations;

            if (agent.IsCollidedWithMap)
            {
                reward -= 1f;
            }

            if (agent.IsCompletedMap)
            {
                reward += 1;
            }

            agent.reward = reward;
        }
    }
}