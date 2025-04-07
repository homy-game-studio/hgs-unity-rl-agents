using UnityEngine;

namespace HGS.RLAgents.NeuralNetworks
{
    public static class Activation
    {
        public static float Sigmoid(float x)
        {
            return 1f / (1f + Mathf.Exp(-x));
        }

        public static float Tanh(float x)
        {
            return (float)System.Math.Tanh(x);
        }

        public static float ReLU(float x)
        {
            return Mathf.Max(0f, x);
        }
    }
}