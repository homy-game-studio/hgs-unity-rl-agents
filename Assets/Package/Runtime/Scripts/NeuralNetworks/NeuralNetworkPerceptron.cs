using System;
using HGS.RLAgents.NeuralNetworks;

namespace HGS.RLAgents
{
    [Serializable]
    public class NeuralNetworkPerceptron
    {
        public float[] Weights { get; set; }
        public float Bias { get; set; }
        public EActivation Activation { get; set; }

        private float Sum(float[] input)
        {
            var sum = 0f;
            for (int i = 0; i < input.Length; i++)
            {
                sum += input[i] * Weights[i];
            }
            return sum;
        }

        public float FeedForward(float[] input)
        {
            var sum = Sum(input);
            sum += Bias;
            return NeuralNetworkActivation.FeedForward(sum, Activation);
        }
    }
}
