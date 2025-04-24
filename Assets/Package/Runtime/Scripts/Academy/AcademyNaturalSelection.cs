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
            var bestModel = Model.Instantiate(bestAgent.model);
            bestModel.name = bestAgent.name;

            foreach (Agent agent in agents)
            {
                agent.model = Model.Instantiate(bestModel);
                agent.model.RandomizeWeights(agent.learningRate);
                agent.reward = 0;
                agent.evaluationCount = 0;
            }
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
