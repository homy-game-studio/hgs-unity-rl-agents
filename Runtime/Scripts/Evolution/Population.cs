using System;
using System.Linq;

namespace HGS.RLAgents.Evolution
{
    public class Population
    {
        public Individual[] Individuals { get; private set; }
        public float AvgEliteFitness { get; private set; }
        public float BestFitness { get; private set; }
        public Individual BestIndividual { get; private set; }

        public bool HasPendingEvaluations => Individuals.Any(i => i.State == EvaluationState.Pending);
        public bool HasCompletedEvaluation => Individuals.All(i => i.State == EvaluationState.Evaluated);
        public int EvaluatedCount => Individuals.Count(i => i.IsEvaluated);

        public Population(
           int size,
           Func<int, Individual> factory)
        {
            Individuals = new Individual[size];

            for (int i = 0; i < size; i++)
                Individuals[i] = factory(i);
        }

        public void Evaluate(int individualId, float fitness)
        {
            Individuals[individualId].State = EvaluationState.Evaluated;
            Individuals[individualId].Fitness = fitness;
        }

        public Individual FindUnevaluatedIndividual()
        {
            for (int i = 0; i < Individuals.Length; i++)
            {
                if (Individuals[i].State == EvaluationState.Pending)
                {
                    Individuals[i].State = EvaluationState.Running;
                    return Individuals[i];
                }
            }

            return default;
        }

        public void Evolve(
            float selectionRate,
            float crossoverRate,
            float mutationRate,
            float mutationStrength)
        {
            float avgEliteFitness = 0;
            float bestFitness = 0;
            Individual bestIndividual = new Individual();
            Individuals = GeneticAlgorithm.Evolve(
                Individuals,
                selectionRate,
                crossoverRate,
                mutationRate,
                mutationStrength,
                out avgEliteFitness,
                out bestFitness,
                out bestIndividual
            );
            AvgEliteFitness = avgEliteFitness;
            BestFitness = bestFitness;
            BestIndividual = bestIndividual;
        }
    }
}
