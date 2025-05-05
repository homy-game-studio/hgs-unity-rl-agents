using System;
using System.Collections.Generic;
using System.Linq;

namespace HGS.RLAgents.NeuralNetworks
{
    [Serializable]
    public class NeuralNetwork
    {
        public List<NeuralNetworkLayer> layers;
        public int WeightCount => layers.Sum(layer => layer.WeightCount);

        public void Initialize(GenerationModelLayer[] modelLayers, int inputSize, int outputSize)
        {
            layers = new List<NeuralNetworkLayer>();
            var prevInputSize = inputSize;

            for (int i = 0; i < modelLayers.Length; i++)
            {
                layers.Add(new NeuralNetworkLayer
                {
                    inputSize = prevInputSize,
                    outputSize = i == modelLayers.Length
                        ? outputSize
                        : modelLayers[i].size,
                    activation = modelLayers[i].activation,
                });
                layers[i].Initialize();
                prevInputSize = modelLayers[i].size;
            }
        }

        public void SetWeights(float[] weights)
        {
            var index = 0;

            foreach (var layer in layers)
            {
                Array.Copy(weights, index, layer.weights, 0, layer.WeightCount);
                index += layer.WeightCount;
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