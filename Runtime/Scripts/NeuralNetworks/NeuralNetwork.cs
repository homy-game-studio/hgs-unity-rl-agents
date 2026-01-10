using System;
using System.Collections.Generic;

namespace HGS.RLAgents.NeuralNetworks
{
    [Serializable]
    public class NeuralNetwork
    {
        public List<NeuralNetworkLayer> Layers { get; set; }

        public void AddLayer(NeuralNetworkLayer layer)
        {
            if (Layers == null)
            {
                Layers = new List<NeuralNetworkLayer>();
            }
            Layers.Add(layer);
        }

        public float[] GetParameters()
        {
            var parameters = new List<float>();
            foreach (var layer in Layers)
            {
                parameters.AddRange(layer.GetWeights());
                parameters.AddRange(layer.GetBiases());
            }
            return parameters.ToArray();
        }

        public void SetParameters(float[] parameters)
        {
            int index = 0;
            foreach(var layer in Layers)
            {
                int weightCount = layer.WeightCount;
                float[] weights = new float[weightCount];

                Array.Copy(parameters, index, weights, 0, weightCount);
                layer.SetWeights(weights);
                index += weightCount;

                float[] biases = new float[(int)layer.Size];
                Array.Copy(parameters, index, biases, 0, (int)layer.Size);
                layer.SetBiases(biases);

                index += (int)layer.Size;
            }
        }

        public float[] FeedForward(float[] input)
        {
            float[] prevLayerOutput = input;
            foreach (var layer in Layers)
            {
                prevLayerOutput = layer.FeedForward(prevLayerOutput);
            }
            return prevLayerOutput;
        }
    }
}