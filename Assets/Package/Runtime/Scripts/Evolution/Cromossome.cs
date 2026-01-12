using System;
using UnityEngine;

namespace HGS.RLAgents.Evolution
{
    [Serializable]
    public struct Cromossome : ICloneable
    {
        public float[] genes;

        public Cromossome(int size)
        {
            genes = new float[size];
        }

        public float GetGene(int index)
        {
            if (genes.Length == 0 || genes.Length < index) return 0;
            return genes[index];
        }

        public void RandomizeGene(int index)
        {
            genes[index] = UnityEngine.Random.Range(-1f, 1f);
        }

        public void Mutate(float rate, float resetRate, float strength)
        {
            var size = genes.Length;

            for (int i = 0; i < size; i++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < rate)
                {
                    genes[i] = genes[i] + UnityEngine.Random.Range(-strength, strength);
                }
                if (UnityEngine.Random.Range(0f, 1f) < resetRate)
                {
                    genes[i] = UnityEngine.Random.Range(-1f, 1f);
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
