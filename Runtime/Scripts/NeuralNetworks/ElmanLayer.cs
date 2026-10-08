using System;

namespace HGS.RLAgents.NeuralNetworks
{
    // Simple recurrent layer: every neuron also reads the previous outputs of the whole layer.
    // Parameters: weights [size x input], recurrent weights [size x size], biases [size]
    public class ElmanLayer : ILayer
    {
        readonly int _inputSize;
        readonly int _size;
        readonly EActivation _activation;
        readonly float[] _weights;
        readonly float[] _recurrentWeights;
        readonly float[] _biases;
        readonly float[] _output;
        readonly float[] _state;

        public int InputSize => _inputSize;
        public int OutputSize => _size;
        public int ParameterCount => CountParameters(_inputSize, _size);

        public static int CountParameters(int inputSize, int size) => size * inputSize + size * size + size;

        public ElmanLayer(int inputSize, int size, EActivation activation)
        {
            _inputSize = inputSize;
            _size = size;
            _activation = activation;
            _weights = new float[size * inputSize];
            _recurrentWeights = new float[size * size];
            _biases = new float[size];
            _output = new float[size];
            _state = new float[size];
        }

        public void GetParameters(float[] destination, int offset)
        {
            LayerMath.Write(_weights, destination, ref offset);
            LayerMath.Write(_recurrentWeights, destination, ref offset);
            LayerMath.Write(_biases, destination, ref offset);
        }

        public void SetParameters(float[] source, int offset)
        {
            LayerMath.Read(source, ref offset, _weights);
            LayerMath.Read(source, ref offset, _recurrentWeights);
            LayerMath.Read(source, ref offset, _biases);
        }

        public float[] Forward(float[] input)
        {
            // Every neuron reads the previous outputs, so _state is only overwritten after all were computed
            for (int i = 0; i < _size; i++)
            {
                var sum = _biases[i]
                          + LayerMath.Dot(_weights, i * _inputSize, input)
                          + LayerMath.Dot(_recurrentWeights, i * _size, _state);
                _output[i] = NeuralNetworkActivation.FeedForward(sum, _activation);
            }

            Array.Copy(_output, _state, _size);
            return _output;
        }

        public void ResetState() => Array.Clear(_state, 0, _state.Length);
    }
}
