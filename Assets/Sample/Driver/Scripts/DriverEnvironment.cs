using HGS.RLAgents.Simulation;
using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    public class DriverEnvironment : SimulationEnvironment
    {
        [SerializeField] DriverAgent agent;

        void Awake()
        {
            agent.onCollideWithMapEvt += FinishEpoch;
            agent.onCompleteMapEvt += FinishEpoch;
        }

        public override void EvaluateFitness()
        {
            float fitness = 0;

            fitness += agent.Checkpoints.Count;
            fitness -= 0.1f * (elapsedTime / maxEpochDuration);

            if (agent.IsCollidedWithMap)
            {
                fitness -= 5f;
            }

            if (agent.IsCompletedMap)
            {
                fitness += 4f;
            }

            agent.fitness = fitness;
        }
    }
}