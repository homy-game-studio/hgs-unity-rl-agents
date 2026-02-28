namespace HGS.RLAgents.NeuralNetworks
{
    public static class NeuralNetworkFactory
    {
        public static NeuralNetwork CreateFromModel(Model model)
        {
            var prevInputSize = model.inputSize;
            var neuralNetwork = new NeuralNetwork();

            foreach (var layer in model.layers)
            {
                var nnLayer = new NeuralNetworkLayer(prevInputSize, layer.size, layer.activation);
                neuralNetwork.AddLayer(nnLayer);
                prevInputSize = layer.size;
            }

            return neuralNetwork;
        }
    }
}
