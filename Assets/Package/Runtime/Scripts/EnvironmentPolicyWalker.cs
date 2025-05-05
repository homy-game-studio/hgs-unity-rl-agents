using System;
using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents
{
    [Serializable]
    public class EnvironmentPolicyDefinition
    {
        public int startEpoch;
        public Policy policy;
    }

    [Serializable]
    public class EnvironmentPolicyWalker
    {
        [SerializeField] List<EnvironmentPolicyDefinition> policies;

        public Action<Policy> onPolicyChange;

        public void Begin()
        {
            var policyDefinition = policies[0];
            onPolicyChange?.Invoke(policyDefinition.policy);
        }

        public void FindNext(Academy academy)
        {
            var policyDefinition = policies.Find(p => p.startEpoch == academy.Generations);

            if (policyDefinition != null)
            {
                onPolicyChange?.Invoke(policyDefinition.policy);
            }
        }
    }
}
