using UnityEngine;
using System;
using HGS.RLAgents.NeuralNetworks;

namespace HGS.RLAgents
{
    [Serializable]
    public class ModelLayer
    {
        public int size;
        public EActivation activation;
    }

    [CreateAssetMenu(fileName = "Model", menuName = "HGS/RLAgents/Model")]
    public class Model : ScriptableObject
    {
        public string populationId = "";
        public float mutationRate = 0.2f;
        public float mutationStrength = 0.1f;
        public int crossoverPoint = 0;
        public int neuralNetworkPoint = 3;
        public ModelLayer[] layers;
        public int inputSize;

        public int GetParametersCount()
        {
            int count = 0;
            int previousSize = inputSize;

            foreach (var layer in layers)
            {
                // (pesos + bias) por neurônio
                count += previousSize * layer.size + layer.size;
                previousSize = layer.size;
            }

            return count;
        }
    }
}