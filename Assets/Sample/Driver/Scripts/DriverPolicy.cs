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

            reward += agent.Checkpoints.Count;
            reward -= 0.01f * agent.evaluationCount;

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