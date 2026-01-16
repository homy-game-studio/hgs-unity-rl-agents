using System;
using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents.Evolution
{
    [Serializable]
    public class Generation
    {
        private Dictionary<string, Population> populations = new Dictionary<string, Population>();

        public void AddAgent(Agent agent)
        {
            var populationId = agent.model.populationId;
            if (!populations.ContainsKey(populationId))
            {
                var population = new Population();
                populations.Add(populationId, population);
            }

            populations[populationId].AddAgent(agent);
        }

        public void Initialize()
        {
            foreach (var population in populations.Values)
            {
                population.Initialize();
            }
        }

        public void Tick(int generation, int maxGenerations)
        {
            foreach (var population in populations)
            {
                population.Value.Select();
                population.Value.SaveProgress();
                population.Value.Crossover();
                population.Value.Mutate();
                population.Value.Replace();
                Debug.Log($"{generation}/{maxGenerations} - Population: {population.Key}, AvgReward: {population.Value.AverageBestReward}");
            }
        }
    }
}
