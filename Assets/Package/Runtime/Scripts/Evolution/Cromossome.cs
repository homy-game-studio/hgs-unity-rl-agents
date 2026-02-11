using System;
using UnityEngine;

namespace HGS.RLAgents.Evolution
{
    public enum CromossomeMutationMode
    {
        Random,
        Sigma
    }

    [Serializable]
    public struct Cromossome : ICloneable
    {
        public float[] genes;

        public Cromossome(int size)
        {
            genes = new float[size];
        }

        static float RandomGaussian(float mean, float stdDev)
        {
            float u1 = 1f - UnityEngine.Random.value;
            float u2 = 1f - UnityEngine.Random.value;

            float randStdNormal =
                Mathf.Sqrt(-2f * Mathf.Log(u1)) *
                Mathf.Sin(2f * Mathf.PI * u2);

            return mean + stdDev * randStdNormal;
        }

        public float GetGene(int index)
        {
            if (genes.Length == 0 || genes.Length < index) return 0;
            return genes[index];
        }

        public void Init(int index, float strength, CromossomeMutationMode mode)
        {
            switch (mode)
            {
                case CromossomeMutationMode.Random:
                    genes[index] = UnityEngine.Random.Range(-strength, strength);
                    break;

                case CromossomeMutationMode.Sigma:
                    genes[index] = RandomGaussian(0f, strength);
                    break;
            }
        }

        public void Mutate(float rate, float strength, CromossomeMutationMode mode)
        {
            var size = genes.Length;

            for (int i = 0; i < size; i++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < rate)
                {
                    switch (mode)
                    {
                        case CromossomeMutationMode.Random:
                            genes[i] += UnityEngine.Random.Range(-strength, strength);
                            break;
                        case CromossomeMutationMode.Sigma:
                            genes[i] += RandomGaussian(0f, strength * (Mathf.Abs(genes[i]) + 1f));
                            break;
                    }
                }
            }
        }

        public object Clone()
        {
            var clone = new Cromossome(genes.Length);
            genes.CopyTo(clone.genes, 0);
            return clone;
        }

        public static Cromossome Crossover(Cromossome a, Cromossome b, int point)
        {
            var size = a.genes.Length;
            var cromossome = new Cromossome(size);

            for (int i = 0; i < size; i++)
            {
                if (i >= point)
                {
                    cromossome.genes[i] = UnityEngine.Random.Range(0f, 1f) > 0.5f
                        ? a.GetGene(i)
                        : b.GetGene(i);
                }
                else
                {
                    cromossome.genes[i] = a.GetGene(i);
                }
            }

            return cromossome;
        }
    }
}
