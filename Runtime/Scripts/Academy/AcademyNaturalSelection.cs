using System;
using System.Collections.Generic;
using System.Linq;

namespace HGS.RLAgents
{
    [Serializable]
    public class AcademyNaturalSelection
    {
        private void Mutate(IEnumerable<Agent> agents, Agent bestAgent)
        {
        }

        private Agent SelectBestAgent(IEnumerable<Agent> agents)
        {
            return agents
                .OrderByDescending(agent => agent.reward)
                .First();
        }

        public void Apply(List<Agent> agents)
        {
            var groups = agents.GroupBy(agent => agent.GetType());

            foreach (var group in groups)
            {
                var agentType = group.Key.Name;
                var bestAgent = SelectBestAgent(group);

                UnityEngine.Debug.Log($"Best agent of type {agentType} ({group.Count()} agents): {bestAgent.name} with reward {bestAgent.reward:F3}");

                Mutate(group, bestAgent);
            }
        }
    }
}
