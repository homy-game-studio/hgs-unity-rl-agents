using System;

namespace HGS.RLAgents.Evolution
{
    public static class GeneticAlgorithm
    {
        public static Individual[] Evolve(
            Individual[] individuals,
            float selectionRate,
            float crossoverRate,
            float mutateRate,
            float mutateStrength,
            out float avgEliteFitness,
            out float bestFitness,
            out Individual bestIndividual
            )
        {
            // Ordena por fitness (desc)
            Array.Sort(individuals, (a, b) => b.Fitness.CompareTo(a.Fitness));

            int populationSize = individuals.Length;
            int eliteCount = (int)(populationSize * selectionRate);

            if (eliteCount < 2) eliteCount = 2;

            // Avg fitness (debug purposes)
            avgEliteFitness = 0f;
            for (int i = 0; i < eliteCount; i++)
                avgEliteFitness += individuals[i].Fitness;
            avgEliteFitness /= eliteCount;
            bestFitness = individuals[0].Fitness;

            // Preserva elite
            var nextGen = new Individual[populationSize];
            bestIndividual = new Individual
            {
                Id = 0,
                Genome = Clone(individuals[0].Genome),
                Fitness = individuals[0].Fitness,
                State = EvaluationState.Pending
            };


            for (int i = 0; i < eliteCount; i++)
            {
                nextGen[i] = new Individual
                {
                    Id = i,
                    Genome = Clone(individuals[i].Genome),
                    Fitness = 0f,
                    State = EvaluationState.Pending
                };
            }

            // Preenche restante com crossover
            for (int i = eliteCount; i < populationSize; i++)
            {
                var parentA = individuals[Rand.Linear(eliteCount)];
                var parentB = individuals[Rand.Linear(eliteCount)];

                Genome childGenome;

                if (Rand.Linear() < crossoverRate)
                    childGenome = Crossover(parentA.Genome, parentB.Genome);
                else
                    childGenome = Clone(parentA.Genome);

                Mutate(childGenome, mutateRate, mutateStrength);

                nextGen[i] = new Individual
                {
                    Id = i,
                    Genome = childGenome,
                    Fitness = 0f,
                    State = EvaluationState.Pending,
                };
            }

            return nextGen;
        }

        private static Genome Clone(Genome source)
        {
            var clone = new Genome(source.Genes.Length);
            Array.Copy(source.Genes, clone.Genes, source.Genes.Length);
            return clone;
        }

        private static Genome Crossover(Genome a, Genome b)
        {
            int size = a.Genes.Length;
            var child = new Genome(size);

            for (int i = 0; i < size; i++)
                child.Genes[i] = Rand.Linear() > 0.5
                    ? a.Genes[i]
                    : b.Genes[i];

            return child;
        }

        private static void Mutate(Genome genome, float rate, float strength)
        {
            for (int i = 0; i < genome.Genes.Length; i++)
            {
                if (Rand.Linear() < rate)
                    genome.Genes[i] += Rand.Gaussian(0f, strength);
            }
        }
    }
}
