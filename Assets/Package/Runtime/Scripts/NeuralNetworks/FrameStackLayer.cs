using System;

namespace HGS.RLAgents.NeuralNetworks
{
    // No parameters: outputs the last `frames` inputs side by side (newest first), zero-filled at the
    // start of an episode. The layers after it see the recent past directly.
    public class FrameStackLayer : ILayer
    {
        readonly int _inputSize;
        readonly int _frames;
        readonly float[] _stack;

        public int InputSize => _inputSize;
        public int OutputSize => _inputSize * _frames;
        public int ParameterCount => 0;

        public FrameStackLayer(int inputSize, int frames)
        {
            _inputSize = inputSize;
            _frames = Math.Max(1, frames);
            _stack = new float[_inputSize * _frames];
        }

        public void GetParameters(float[] destination, int offset) { }
        public void SetParameters(float[] source, int offset) { }

        public float[] Forward(float[] input)
        {
            // Shift the older frames one slot back, then write the new one in front
            Array.Copy(_stack, 0, _stack, _inputSize, _stack.Length - _inputSize);
            Array.Copy(input, 0, _stack, 0, _inputSize);
            return _stack;
        }

        public void ResetState() => Array.Clear(_stack, 0, _stack.Length);
    }
}
