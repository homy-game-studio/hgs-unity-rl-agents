using System;
using System.Collections.Generic;

namespace HGS.RLAgents.NeuralNetworks
{
    [Serializable]
    public class NeuralNetwork
    {
        public List<Layer> layers;

        public void Initialize(int[] layerSizes)
        {
            layers = new List<Layer>();

            for (int i = 0; i < layerSizes.Length - 1; i++)
            {
                layers.Add(new Layer
                {
                    inputSize = layerSizes[i],
                    outputSize = layerSizes[i + 1]
                });
                layers[i].Initialize();
            }
        }

        public void RandomizeWeights(float factor)
        {
            foreach (var layer in layers)
            {
                layer.RandomizeWeights(factor);
            }
        }

        public float[] FeedForward(float[] input)
        {
            float[] prevLayerOutput = input;
            foreach (var layer in layers)
            {
                prevLayerOutput = layer.FeedForward(prevLayerOutput);
            }
            return prevLayerOutput;
        }
    }
}