using System;
using System.Collections.Generic;

namespace HGS.RLAgents.NeuralNetworks
{
    [Serializable]
    public class NeuralNetworkLayer
    {
        private int _inputSize;
        private int _size;
        private NeuralNetworkPerceptron[] _perceptrons;
        private float[] _output;

        public int WeightCount => _inputSize * _size;
        public float Size => _size;

        public NeuralNetworkLayer(int inputSize, int size, EActivation activation)
        {
            _inputSize = inputSize;
            _size = size;
            _output = new float[size];
            _perceptrons = new NeuralNetworkPerceptron[size];

            for (int i = 0; i < _size; i++)
            {
                _perceptrons[i] = new NeuralNetworkPerceptron
                {
                    Activation = activation,
                    Weights = new float[inputSize],
                };
            }
        }

        public void SetWeights(float[] value)
        {
            int index = 0;
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _inputSize; j++)
                {
                    _perceptrons[i].Weights[j] = value[index++];
                }
            }
        }

        public void SetBiases(float[] value)
        {
            for (int i = 0; i < _size; i++)
            {
                _perceptrons[i].Bias = value[i];
            }
        }

        public List<float> GetWeights()
        {
            var weights = new List<float>();
            foreach (var perceptron in _perceptrons)
            {
                weights.AddRange(perceptron.Weights);
            }
            return weights;
        }

        public List<float> GetBiases()
        {
            var biases = new List<float>();
            foreach (var perceptron in _perceptrons)
            {
                biases.Add(perceptron.Bias);
            }
            return biases;
        }

        public float[] FeedForward(float[] input)
        {
            for (int i = 0; i < _size; i++)
            {
                _output[i] = _perceptrons[i].FeedForward(input);
            }

            return _output;
        }
    }
}