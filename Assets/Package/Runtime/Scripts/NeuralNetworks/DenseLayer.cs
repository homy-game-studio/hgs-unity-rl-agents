namespace HGS.RLAgents.NeuralNetworks
{
    // Fully connected layer, no memory. Parameters: weights [size x input], biases [size]
    public class DenseLayer : ILayer
    {
        readonly int _inputSize;
        readonly int _size;
        readonly EActivation _activation;
        readonly float[] _weights;
        readonly float[] _biases;
        readonly float[] _output;

        public int InputSize => _inputSize;
        public int OutputSize => _size;
        public int ParameterCount => CountParameters(_inputSize, _size);

        public static int CountParameters(int inputSize, int size) => size * inputSize + size;

        public DenseLayer(int inputSize, int size, EActivation activation)
        {
            _inputSize = inputSize;
            _size = size;
            _activation = activation;
            _weights = new float[size * inputSize];
            _biases = new float[size];
            _output = new float[size];
        }

        public void GetParameters(float[] destination, int offset)
        {
            LayerMath.Write(_weights, destination, ref offset);
            LayerMath.Write(_biases, destination, ref offset);
        }

        public void SetParameters(float[] source, int offset)
        {
            LayerMath.Read(source, ref offset, _weights);
            LayerMath.Read(source, ref offset, _biases);
        }

        public float[] Forward(float[] input)
        {
            for (int i = 0; i < _size; i++)
            {
                var sum = _biases[i] + LayerMath.Dot(_weights, i * _inputSize, input);
                _output[i] = NeuralNetworkActivation.FeedForward(sum, _activation);
            }
            return _output;
        }

        public void ResetState() { }
    }
}
