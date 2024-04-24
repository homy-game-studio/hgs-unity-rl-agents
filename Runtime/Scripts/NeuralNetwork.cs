using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents
{
    public class NeuralNetwork
    {
        public List<Layer> layers { get; set; }

        public void Initialize(int[] layerSizes)
        {
            layers = new List<Layer>();

            for (int i = 0; i < layerSizes.Length - 1; i++)
            {
                layers.Add(new Layer
                {
                    InputSize = layerSizes[i],
                    OutputSize = layerSizes[i + 1]
                });
                layers[i].Initialize();
            }
            Debug.Log($"Initialized NeuralNetwork with {layers.Count} layers");
        }

        public void Backpropagation(float[] input, float[] targetOutput, float learningRate)
        {
            // Forward pass
            var prevLayerOutput = input;
            var layerOutputs = new List<float[]>();

            foreach (var layer in layers)
            {
                prevLayerOutput = layer.FeedForward(prevLayerOutput);
                layerOutputs.Add(prevLayerOutput);
            }

            // Backward pass
            var outputGradient = new float[targetOutput.Length];

            for (int i = 0; i < targetOutput.Length; i++)
            {
                var error = targetOutput[i] - prevLayerOutput[i];
                outputGradient[i] = error * Sigmoid.Derivative(prevLayerOutput[i]);
            }

            // Update weights and biases starting from output layer
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var layer = layers[i];
                float[] nextLayerOutput = i > 0 ? layerOutputs[i - 1] : input; // Use input for first layer

                var deltaWeights = new float[layer.InputSize * layer.OutputSize];
                var deltaBiases = new float[layer.OutputSize];

                // Calculate delta weights and biases
                for (int j = 0; j < layer.OutputSize; j++)
                {
                    for (int k = 0; k < layer.InputSize; k++)
                    {
                        int idx = j * layer.InputSize + k;
                        deltaWeights[idx] = learningRate * outputGradient[j] * nextLayerOutput[k];
                    }

                    deltaBiases[j] = learningRate * outputGradient[j];
                }

                // Update weights and biases
                layer.UpdateWeightsAndBiases(deltaWeights, deltaBiases);

                // Calculate gradient for the next layer
                if (i > 0)
                {
                    outputGradient = layer.CalculateGradient(outputGradient, nextLayerOutput);
                }
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