using UnityEngine;
using System;
using HGS.RLAgents.NeuralNetworks;
using HGS.RLAgents.Evolution;
using Newtonsoft.Json;

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
        public float mutationResetRate = 0.2f;
        public float mutationStrength = 0.1f;
        public float selectionRate = 0.2f;
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

        public void SaveCromossome(Cromossome cromossome)
        {
            var contents = JsonConvert.SerializeObject(cromossome);
            // cria a pasta caso nao exista
            if (!System.IO.Directory.Exists(Application.dataPath + "/Resources/cromossomes"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/cromossomes");
            }
            System.IO.File.WriteAllText(Application.dataPath + $"/Resources/cromossomes/{populationId}.json", contents);
            Debug.Log($"Cromossome saved for population {populationId}");
        }

        public Cromossome LoadCromossome()
        {
            var path = Application.dataPath + $"/Resources/cromossomes/{populationId}.json";
            if (System.IO.File.Exists(path))
            {
                var contents = System.IO.File.ReadAllText(path);
                var cromossome = JsonConvert.DeserializeObject<Cromossome>(contents);
                Debug.Log($"Cromossome loaded for population {populationId}");
                return cromossome;
            }
            else
            {
                Debug.LogWarning($"Cromossome file not found for population {populationId} at path: {path}");
                return new Cromossome();
            }
        }
    }
}