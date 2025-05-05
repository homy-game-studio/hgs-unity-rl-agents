using UnityEngine;
using Newtonsoft.Json;
using HGS.RLAgents.NeuralNetworks;
using System;

namespace HGS.RLAgents
{
    [Serializable]
    public class GenerationModelLayer
    {
        public int size;
        public EActivation activation;
    }

    [CreateAssetMenu(fileName = "Generation", menuName = "HGS/RLAgents/GenerationModel")]
    public class GenerationModel : ScriptableObject
    {
        public string populationId = "";
        public float mutationProbability = 0.2f;
        public int crossoverPoint = 0;
        public int neuralNetworkPoint = 3;
        public GenerationModelLayer[] layers;
        public TextAsset modelAsset;
        public int inputSize;

        public int OutputSize => layers[layers.Length - 1].size;

        //public virtual void SaveModel(string filePath)
        //{
        //    try
        //    {
        //        var directoryPath = Path.GetDirectoryName(filePath);
        //        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        //        var serializedModel = JsonConvert.SerializeObject(neuralNetwork);
        //        File.WriteAllText(filePath, serializedModel);
        //        Debug.Log("Model saved in: " + filePath);
        //    }
        //    catch (IOException e)
        //    {
        //        Debug.LogError("Failed to save model: " + e.Message);
        //    }
        //}

        public virtual void Load()
        {
            var neuralNetwork = modelAsset
              ? JsonConvert.DeserializeObject<NeuralNetwork>(modelAsset.text)
              : new NeuralNetwork();
        }
    }
}