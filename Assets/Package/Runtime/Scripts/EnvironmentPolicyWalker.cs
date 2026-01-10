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
