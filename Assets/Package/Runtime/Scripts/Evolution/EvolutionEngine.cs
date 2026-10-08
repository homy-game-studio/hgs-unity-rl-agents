using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace HGS.RLAgents.Evolution
{
    [Serializable]
    public class EvolutionEngine
    {
        public Dictionary<string, Population> populations;
        public int Generation { get; set; }

        public bool HasPendingEvaluations =>
            populations.Values.Any(population => population.HasPendingEvaluations);
        public bool HasCompletedEvaluation =>
            populations.Values.All(population => population.HasCompletedEvaluation);

        EvolutionEngine()
        {
            populations = new Dictionary<string, Population>();
            Generation = 0;
        }

        public void Save(int phase)
        {
            var keys = populations.Keys;
            foreach (var key in keys)
            {
                var timestamp = DateTime.Now;
                var generation = Generation;
                var populationId = key;
                var bestIndividual = populations[key].BestIndividual;
                var bestFitness = bestIndividual.Fitness;
                var fileName = $"{timestamp:yyyy-MM-dd_HH-mm-ss}_phase_{phase}_gen_{generation}_fitness_{bestFitness}.json";
                var json = JsonConvert.SerializeObject(bestIndividual.Genome, Formatting.None);
                var folder = $"Resources/Genomes/{populationId}";
                // makeDir if not exists
                var folderPath = Path.Combine("Assets", folder);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                File.WriteAllText(Path.Combine(folderPath, fileName), json);
            }
        }

        public bool HasPopulation(string id)
        {
            return populations.ContainsKey(id);
        }

        public void AddPopulation(string id, int size, Func<int, Individual> individualFactory)
        {
            populations.Add(id, new Population(size, individualFactory));
        }

        public Individual NextIndividual(string populationId)
        {
            return populations[populationId].FindUnevaluatedIndividual();
        }

        public void Evaluate(string populationId, int individualId, float fitness)
        {
            populations[populationId].Evaluate(individualId, fitness);
        }

        public void Evolve(float selectionRate, float crossoverRate, float mutationRate, float mutationStrength)
        {
            var keys = populations.Keys;
            foreach (var key in keys)
            {
                populations[key].Evolve(
                    selectionRate,
                    crossoverRate,
                    mutationRate,
                    mutationStrength
                );
            }
            Generation++;
        }
    }
}
