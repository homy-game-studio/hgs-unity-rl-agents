using System;
using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerPickCratePolicy : Policy
    {
        [SerializeField] StackerEnvironment env;

        private StackerAgent Agent => env.agent;

        public override void EvaluateReward()
        {
            var reward = 0f;

            // Aproximar-se
            reward += Agent.PickedCrateCount;

            // Penalidades
            if (Agent.IsCollidedWithMap) reward -= 20f;
            reward -= 0.1f * (Agent.IdleTime / env.MaxEpochDuration);
            reward -= 0.1f * Agent.AvgTimeToPickCrate;
            reward -= 0.1f * (Agent.TimeWithoutCrate / env.MaxEpochDuration);

            Agent.reward = reward;
        }

        public override void StartEpoch()
        {
            base.StartEpoch();
            env.RespawnCrates();
        }

        public override void TransitionIn()
        {
            Agent.onPickCrateEvt += OnAgentPickCrate;
            Agent.onCollideWithMapEvt += env.CompleteEpoch;
        }

        public override void TransitionOut()
        {
            env.RespawnCrates();

            Agent.onPickCrateEvt -= OnAgentPickCrate;
            Agent.onCollideWithMapEvt -= env.CompleteEpoch;
        }

        private void OnAgentPickCrate(Transform crate)
        {
            Agent.Drop();
            crate.gameObject.SetActive(false);
            if (env.agent.PickedCrateCount >= env.CrateCount)
            {
                env.CompleteEpoch();
            }
        }
    }
}