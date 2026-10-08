using System;

namespace HGS.RLAgents.NeuralNetworks
{
    // The only place that maps a ModelLayer to a concrete layer. Sizes are computed from static
    // helpers, so a model can count its parameters without allocating the layers.
    public static class NeuralNetworkFactory
    {
        public static NeuralNetwork CreateFromModel(Model model)
        {
            var prevInputSize = model.inputSize;
            var neuralNetwork = new NeuralNetwork();

            foreach (var spec in model.layers)
            {
                var layer = CreateLayer(prevInputSize, spec);
                neuralNetwork.AddLayer(layer);
                prevInputSize = layer.OutputSize;
            }

            return neuralNetwork;
        }

        public static ILayer CreateLayer(int inputSize, ModelLayer spec)
        {
            switch (spec.type)
            {
                case ELayerType.Dense: return new DenseLayer(inputSize, spec.size, spec.activation);
                case ELayerType.Elman: return new ElmanLayer(inputSize, spec.size, spec.activation);
                case ELayerType.LSTM: return new LstmLayer(inputSize, spec.size);
                case ELayerType.GRU: return new GruLayer(inputSize, spec.size);
                case ELayerType.FrameStack: return new FrameStackLayer(inputSize, spec.frames);
                default: throw new ArgumentOutOfRangeException(nameof(spec), spec.type, "Unknown layer type");
            }
        }

        public static int CountParameters(int inputSize, ModelLayer spec)
        {
            switch (spec.type)
            {
                case ELayerType.Dense: return DenseLayer.CountParameters(inputSize, spec.size);
                case ELayerType.Elman: return ElmanLayer.CountParameters(inputSize, spec.size);
                case ELayerType.LSTM: return LstmLayer.CountParameters(inputSize, spec.size);
                case ELayerType.GRU: return GruLayer.CountParameters(inputSize, spec.size);
                case ELayerType.FrameStack: return 0;
                default: throw new ArgumentOutOfRangeException(nameof(spec), spec.type, "Unknown layer type");
            }
        }

        public static int GetOutputSize(int inputSize, ModelLayer spec)
        {
            return spec.type == ELayerType.FrameStack ? inputSize * Math.Max(1, spec.frames) : spec.size;
        }
    }
}
