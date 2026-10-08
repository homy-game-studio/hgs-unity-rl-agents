using System;
using System.Collections.Generic;

namespace HGS.RLAgents.NeuralNetworks
{
    // A pipeline of layers. The genome is the concatenation of the layers' parameters.
    [Serializable]
    public class NeuralNetwork
    {
        public List<ILayer> Layers { get; set; }
        public List<List<float>> Activations { get; set; }

        public int ParameterCount
        {
            get
            {
                int count = 0;
                if (Layers != null) foreach (var layer in Layers) count += layer.ParameterCount;
                return count;
            }
        }

        public void AddLayer(ILayer layer)
        {
            if (Layers == null)
            {
                Layers = new List<ILayer>();
            }
            Layers.Add(layer);
        }

        public float[] GetParameters()
        {
            var parameters = new float[ParameterCount];
            int index = 0;
            foreach (var layer in Layers)
            {
                layer.GetParameters(parameters, index);
                index += layer.ParameterCount;
            }
            return parameters;
        }

        public void SetParameters(float[] parameters)
        {
            if (parameters.Length != ParameterCount)
                throw new ArgumentException($"Expected {ParameterCount} parameters but got {parameters.Length}.");

            int index = 0;
            foreach (var layer in Layers)
            {
                layer.SetParameters(parameters, index);
                index += layer.ParameterCount;
            }
        }

        // Clears the memory of the stateful layers (start of a new episode)
        public void ResetState()
        {
            if (Layers == null) return;

            foreach (var layer in Layers)
            {
                layer.ResetState();
            }
        }

        // Forward pass without recording the activations: for batch evaluation, not for drawing.
        // The returned array belongs to the last layer and is overwritten by the next call.
        public float[] Predict(float[] input)
        {
            float[] output = input;
            foreach (var layer in Layers)
            {
                output = layer.Forward(output);
            }
            return output;
        }

        public float[] FeedForward(float[] input)
        {
            Activations = new List<List<float>>();
            Activations.Add(new List<float>(input));
            float[] prevLayerOutput = input;
            foreach (var layer in Layers)
            {
                prevLayerOutput = layer.Forward(prevLayerOutput);
                Activations.Add(new List<float>(prevLayerOutput));
            }
            return prevLayerOutput;
        }
    }
}
