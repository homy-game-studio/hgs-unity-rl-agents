using UnityEngine;

namespace HGS.RLAgents.NeuralNetworks
{
    public enum EActivation
    {
        Sigmoid,
        Tanh,
        ReLU
    }

    public static class NeuralNetworkActivation
    {
        public static float FeedForward(float x, EActivation activation)
        {
            switch (activation)
            {
                case EActivation.Sigmoid: return 1f / (1f + Mathf.Exp(-x));
                case EActivation.Tanh: return (float)System.Math.Tanh(x);
                case EActivation.ReLU: return Mathf.Max(0f, x);
                default: return x;
            }
        }
    }
}