using System.Collections.Generic;
using System.Linq;

namespace HGS.RLAgents.Evolution
{
    public class Population
    {
        private List<Agent> _agents = new List<Agent>();
        private List<Cromossome> _bestCromossomes = new List<Cromossome>();
        private List<Cromossome> _cromossomes = new List<Cromossome>();

        int _crossoverPoint;
        float _mutationProbability;
        float _bestReward;

        public float BestReward => _bestReward;

        public void Initialize()
        {
            foreach (var agent in _agents)
            {
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

            var _bestAgents = _agents
                .OrderByDescending(agent => agent.reward)
                .Take(2)
                .ToList();

            _crossoverPoint = _bestAgents[0].model.crossoverPoint;
            _mutationProbability = _bestAgents[0].model.mutationProbability;
            _bestReward = _bestAgents[0].reward;

            _bestCromossomes = _bestAgents
                .Select(agent => (Cromossome)agent.cromossome.Clone())
                .ToList();
        }

        public void Crossover()
        {
            var populationSize = _agents.Count();
            var parentA = _bestCromossomes[0];
            var parentB = _bestCromossomes[1];

            for (var i = 0; i < populationSize; i++)
            {
                var cromossome = Cromossome.Crossover(parentA, parentB, _crossoverPoint);
                _cromossomes.Add(cromossome);
            }
        }

        public void Mutate()
        {
            var populationSize = _agents.Count();

            for (var i = 0; i < populationSize; i++)
            {
                _cromossomes[i].Mutate(_mutationProbability);
            }
        }

        public void Replace()
        {
            var populationSize = _agents.Count();

            _agents[0].SetCromossome(_bestCromossomes[0]);
            _agents[1].SetCromossome(_bestCromossomes[1]);

            for (var i = 2; i < populationSize; i++)
            {
                _agents[i].SetCromossome(_cromossomes[i]);
            }

        }
    }
}
