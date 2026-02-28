namespace HGS.RLAgents.Evolution
{
    public struct Genome
    {
        public float[] Genes { get; set; }

        public Genome(int size)
        {
            Genes = new float[size];
        }

        public void Seed(float strength)
        {
            for (int i = 0; i < Genes.Length; i++)
            {
                Genes[i] = Rand.Gaussian(0f, strength);
            }
        }

        public float GetGene(int index)
        {
            if (Genes.Length == 0 || Genes.Length < index) return 0;
            return Genes[index];
        }
    }
}
