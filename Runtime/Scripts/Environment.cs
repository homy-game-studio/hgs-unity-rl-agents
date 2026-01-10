using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HGS.RLAgents
{
    public class Environment : MonoBehaviour
    {
        [SerializeField] EnvironmentPolicyWalker policyWalker;

        protected Academy academy;

        Policy _policy;

        List<Agent> _agents = new List<Agent>();

        public bool IsCompleted { get; set; }
        public int Epoch => academy.Generations;
        public float MaxEpochDuration => academy.MaxGenerationDuration;

        protected virtual void Awake()
        {
            academy = FindAnyObjectByType<Academy>();
            academy.AddEnvironment(this);

            _agents = transform.GetComponentsInChildren<Agent>().ToList();

            foreach (Agent agent in _agents)
            {
                academy.AddAgent(agent);
            }

            policyWalker.onPolicyChange += SetPolicy;
        }

        public void SetPolicy(Policy policy)
        {
            if (_policy != null) _policy.TransitionOut();
            _policy = policy;
            _policy.TransitionIn();

            academy.MaxGenerationDuration = _policy.maxGenerationTime;
        }

        public void CompleteEpoch()
        {
            if (IsCompleted) return;

            IsCompleted = true;
            academy.CompleteEnvironmentEpoch(this);

            foreach (var agent in _agents)
            {
                agent.Stop();
                agent.active = false;
            }
        }

        public void FinishEpoch()
        {
            IsCompleted = true;
            _policy.EvaluateReward();
            _policy.FinishEpoch();

            foreach (var agent in _agents)
            {
                agent.Stop();
                agent.active = false;
            }

            OnFinishEpoch();
        }

        public void StartEpoch()
        {
            foreach (var agent in _agents)
            {
                agent.active = true;
                agent.reward = 0;
                agent.evaluationCount = 0;
                agent.Respawn();
            }

            policyWalker.FindNext(academy);
            IsCompleted = false;
            _policy.StartEpoch();
            OnStartEpoch();
        }

        protected virtual void OnFinishEpoch() { }
        protected virtual void OnStartEpoch() { }
    }
}
