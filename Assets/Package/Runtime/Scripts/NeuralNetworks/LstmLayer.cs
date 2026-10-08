using System;

namespace HGS.RLAgents.NeuralNetworks
{
    // Long Short-Term Memory. Gates (sigmoid) decide what the cell state c forgets, stores and shows:
    //   f = s(Wf x + Uf h + bf)   i = s(Wi x + Ui h + bi)   g = tanh(Wg x + Ug h + bg)   o = s(Wo x + Uo h + bo)
    //   c = f * c + i * g         h = o * tanh(c)
    // Parameters, per gate in the order f, i, g, o: W [size x input], U [size x size], b [size]
    public class LstmLayer : ILayer
    {
        const int Gates = 4;
        const int Forget = 0, Input = 1, Candidate = 2, Output = 3;

        readonly int _inputSize;
        readonly int _size;
        readonly float[][] _w = new float[Gates][];
        readonly float[][] _u = new float[Gates][];
        readonly float[][] _b = new float[Gates][];

        readonly float[] _h;
        readonly float[] _c;
        readonly float[] _newH;
        readonly float[] _newC;

        public int InputSize => _inputSize;
        public int OutputSize => _size;
        public int ParameterCount => CountParameters(_inputSize, _size);

        public static int CountParameters(int inputSize, int size) => Gates * (size * inputSize + size * size + size);

        public LstmLayer(int inputSize, int size)
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
            _c = new float[size];
            _newH = new float[size];
            _newC = new float[size];
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

        float Pre(int gate, int unit, float[] input)
        {
            return _b[gate][unit]
                   + LayerMath.Dot(_w[gate], unit * _inputSize, input)
                   + LayerMath.Dot(_u[gate], unit * _size, _h);
        }

        public float[] Forward(float[] input)
        {
            // All units read the previous h, so the new state is only committed after the loop
            for (int j = 0; j < _size; j++)
            {
                var f = NeuralNetworkActivation.FeedForward(Pre(Forget, j, input), EActivation.Sigmoid);
                var i = NeuralNetworkActivation.FeedForward(Pre(Input, j, input), EActivation.Sigmoid);
                var g = NeuralNetworkActivation.FeedForward(Pre(Candidate, j, input), EActivation.Tanh);
                var o = NeuralNetworkActivation.FeedForward(Pre(Output, j, input), EActivation.Sigmoid);

                _newC[j] = f * _c[j] + i * g;
                _newH[j] = o * NeuralNetworkActivation.FeedForward(_newC[j], EActivation.Tanh);
            }

            Array.Copy(_newC, _c, _size);
            Array.Copy(_newH, _h, _size);
            return _h;
        }

        public void ResetState()
        {
            Array.Clear(_h, 0, _size);
            Array.Clear(_c, 0, _size);
        }
    }
}
