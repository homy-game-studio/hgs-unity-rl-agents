using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using HGS.RLAgents.NeuralNetworks;

namespace HGS.RLAgents.Agents
{
    [CreateAssetMenu(fileName = "Model", menuName = "HGS/RLAgents/Model")]
    public class Model : ScriptableObject
    {
        public int[] layers;
        public TextAsset modelAsset;
        public NeuralNetwork neuralNetwork;

        public void Initialize()
        {
            neuralNetwork = new NeuralNetwork();
            neuralNetwork.Initialize(layers);
        }

        public void RandomizeWeights(float factor = 1)
        {
            neuralNetwork.RandomizeWeights(factor);
        }

        public virtual void SaveModel(string filePath)
        {
            try
            {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                var serializedModel = JsonConvert.SerializeObject(neuralNetwork);
                File.WriteAllText(filePath, serializedModel);
                Debug.Log("Model saved in: " + filePath);
            }
            catch (IOException e)
            {
                Debug.LogError("Failed to save model: " + e.Message);
            }
        }

        public virtual void Load()
        {
            var neuralNetwork = modelAsset
              ? JsonConvert.DeserializeObject<NeuralNetwork>(modelAsset.text)
              : new NeuralNetwork();
        }

        public virtual float[] FeedForward(float[] input)
        {
            return neuralNetwork.FeedForward(input);
        }
    }
}