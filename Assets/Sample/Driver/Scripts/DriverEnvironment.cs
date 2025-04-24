using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    public class DriverEnvironment : Environment
    {
        [SerializeField] DriverAgent driverAgent;
        [SerializeField] int maxEvaluations = 200;

        protected override void Awake()
        {
            base.Awake();

            driverAgent.onCollideWithMapEvt += CompleteEpoch;
            driverAgent.onCompleteMapEvt += CompleteEpoch;
        }

        protected override void EvaluateReward()
        {
            float reward = 0;

            reward += driverAgent.Checkpoints.Count;
            reward -= (float)driverAgent.evaluationCount / (float)maxEvaluations;

            if (driverAgent.IsCollidedWithMap)
            {
                reward -= 1f;
            }

            if (driverAgent.IsCompletedMap)
            {
                reward += 1;
            }

            driverAgent.reward = reward;
        }

        protected override void OnStartEpoch()
        {
            driverAgent.active = true;
            driverAgent.Respawn();
        }

        protected override void OnFinishEpoch()
        {
            driverAgent.active = false;
        }
    }
}