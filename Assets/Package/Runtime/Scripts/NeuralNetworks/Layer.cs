using System;

namespace HGS.RLAgents.NeuralNetworks
{
    [Serializable]
    public class Layer
    {
        public int inputSize;
        public int outputSize;
        public float[] weights;

        public void Initialize()
        {
            weights = new float[inputSize * outputSize];
        }

        public void RandomizeWeights(float factor)
        {
            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < outputSize; j++)
                {
                    int index = i * outputSize + j;
                    weights[index] = weights[index] + UnityEngine.Random.Range(-factor, factor);
                }
            }
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

                output[j] = Activation.Tanh(sum);
            }

            return output;
        }
    }
}