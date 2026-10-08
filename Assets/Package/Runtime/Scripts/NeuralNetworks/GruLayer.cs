using System;

namespace HGS.RLAgents.NeuralNetworks
{
    // Gated Recurrent Unit: a LSTM with one state vector and two gates.
    //   z = s(Wz x + Uz h + bz)   r = s(Wr x + Ur h + br)   n = tanh(Wn x + r * (Un h) + bn)
    //   h = (1 - z) * n + z * h
    // Parameters, per gate in the order z, r, n: W [size x input], U [size x size], b [size]
    public class GruLayer : ILayer
    {
        const int Gates = 3;
        const int Update = 0, Reset = 1, Candidate = 2;

        readonly int _inputSize;
        readonly int _size;
        readonly float[][] _w = new float[Gates][];
        readonly float[][] _u = new float[Gates][];
        readonly float[][] _b = new float[Gates][];

        readonly float[] _h;
        readonly float[] _newH;

        public int InputSize => _inputSize;
        public int OutputSize => _size;
        public int ParameterCount => CountParameters(_inputSize, _size);

        public static int CountParameters(int inputSize, int size) => Gates * (size * inputSize + size * size + size);

        public GruLayer(int inputSize, int size)
        {
            _inputSize = inputSize;
            _size = size;

            for (int g = 0; g < Gates; g++)
            {
                _w[g] = new float[size * inputSize];
                _u[g] = new float[size * size];
                _b[g] = new float[size];
            }

            _h = new float[size];
            _newH = new float[size];
        }

        public void GetParameters(float[] destination, int offset)
        {
            for (int g = 0; g < Gates; g++)
            {
                LayerMath.Write(_w[g], destination, ref offset);
                LayerMath.Write(_u[g], destination, ref offset);
                LayerMath.Write(_b[g], destination, ref offset);
            }
        }

        public void SetParameters(float[] source, int offset)
        {
            for (int g = 0; g < Gates; g++)
            {
                LayerMath.Read(source, ref offset, _w[g]);
                LayerMath.Read(source, ref offset, _u[g]);
                LayerMath.Read(source, ref offset, _b[g]);
            }
        }

        public float[] Forward(float[] input)
        {
            for (int j = 0; j < _size; j++)
            {
                var z = NeuralNetworkActivation.FeedForward(
                    _b[Update][j] + LayerMath.Dot(_w[Update], j * _inputSize, input) + LayerMath.Dot(_u[Update], j * _size, _h),
                    EActivation.Sigmoid);
                var r = NeuralNetworkActivation.FeedForward(
                    _b[Reset][j] + LayerMath.Dot(_w[Reset], j * _inputSize, input) + LayerMath.Dot(_u[Reset], j * _size, _h),
                    EActivation.Sigmoid);
                var n = NeuralNetworkActivation.FeedForward(
                    _b[Candidate][j] + LayerMath.Dot(_w[Candidate], j * _inputSize, input) + r * LayerMath.Dot(_u[Candidate], j * _size, _h),
                    EActivation.Tanh);

                _newH[j] = (1f - z) * n + z * _h[j];
            }

            Array.Copy(_newH, _h, _size);
            return _h;
        }

        public void ResetState() => Array.Clear(_h, 0, _size);
    }
}
