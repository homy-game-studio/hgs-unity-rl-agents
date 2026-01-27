using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HGS.RLAgents.Evolution
{
    public class Population
    {
        private List<Agent> _agents = new List<Agent>();
        private List<Cromossome> _bestCromossomes = new List<Cromossome>();
        private List<Cromossome> _cromossomes = new List<Cromossome>();

        int _crossoverPoint;
        float _mutationRate;
        float _mutationResetRate;
        float _mutationStrength;

        private List<Agent> _bestAgents = new List<Agent>();

        public float AverageBestReward => _bestAgents.Average(a => a.reward);

        public void Initialize()
        {
            foreach (var agent in _agents)
            {
                if (agent.loadCromossomeOnAwake) continue;

                var size = agent.CromossomeSize;
                var cromossome = new Cromossome(size);
                for (var i = 0; i < size; i++)
                {
                    cromossome.RandomizeGene(i);
                }
                agent.SetCromossome(cromossome);
            }
        }

        public void AddAgent(Agent agent) => _agents.Add(agent);

        public void Select()
        {
            _cromossomes.Clear();

            float selectionRate = _agents[0].model.selectionRate;

            int populationSize = _agents.Count;
            int eliteCount = Mathf.Max(2, Mathf.CeilToInt(populationSize * selectionRate));

            var bestAgents = _agents
                .OrderByDescending(agent => agent.reward)
                .Take(eliteCount)
                .ToList();

            var best = bestAgents[0];

            _crossoverPoint = best.model.crossoverPoint;
            _mutationRate = best.model.mutationRate;
            _mutationResetRate = best.model.mutationResetRate;
            _mutationStrength = best.model.mutationStrength;
            _bestAgents = bestAgents;

            _bestCromossomes = bestAgents
                .Select(agent => (Cromossome)agent.cromossome.Clone())
                .ToList();
        }

        public void SaveProgress()
        {
            var best = _bestAgents[0];
            best.model.SaveCromossome(best.cromossome);
        }

        public void Crossover()
        {
            int populationSize = _agents.Count;
            int eliteCount = _bestCromossomes.Count;

            for (int i = 0; i < populationSize; i++)
            {
                if (i == 0)
                {
                    // Keep the best cromossome
                    _cromossomes.Add((Cromossome)_bestCromossomes[0].Clone());
                    continue;
                }

                var parentA = _bestCromossomes[Random.Range(0, eliteCount)];
                var parentB = _bestCromossomes[Random.Range(0, eliteCount)];

                int point = Random.Range(0, parentA.genes.Length);

                var cromossome = Cromossome.Crossover(parentA, parentB, _crossoverPoint);
                _cromossomes.Add(cromossome);
            }
        }

        public void Mutate(float _mutationFactor)
        {
            var populationSize = _agents.Count();

            for (var i = 0; i < populationSize; i++)
            {
                if (i == 0)
                {
                    // Do not mutate the best cromossome
                    continue;
                }
                _cromossomes[i].Mutate(_mutationRate * _mutationFactor, _mutationResetRate * _mutationFactor, _mutationStrength);
            }
        }

        public void Replace()
        {
            var populationSize = _agents.Count();

            for (var i = 0; i < populationSize; i++)
            {
                _agents[i].SetCromossome(_cromossomes[i]);
            }
        }
    }
}
