using System;

namespace HGS.RLAgents.NeuralNetworks
{
    [Serializable]
    public class NeuralNetworkLayer
    {
        public int inputSize;
        public int outputSize;
        public float[] weights;
        public EActivation activation;

        public int WeightCount => inputSize * outputSize;

        public void Initialize()
        {
            weights = new float[WeightCount];
        }

        public float[] FeedForward(float[] input)
        {
            var output = new float[outputSize];

            for (int j = 0; j < outputSize; j++)
            {
                float sum = 0f;
                for (int i = 0; i < inputSize; i++)
                {
                    int index = i * outputSize + j;
                    sum += input[i] * weights[index];
                }

                output[j] = NeuralNetworkActivation.Do(sum, activation);
            }

            return output;
        }
    }
}