using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace HGS.RLAgents
{
  [CreateAssetMenu(fileName = "Brain", menuName = "HGS/RLAgents/Brain")]
  public class Brain : ScriptableObject
  {
    public int[] layers;
    public float explorationRate = 0.1f;
    public float learningRate = 0.1f;
    public float discountFactor = 0.9f;
    public bool trainMode = true;
    public TextAsset modelAsset;
    public float averageReward = 0;

    DeepQLearning qLearning;

    private int outputSize;

    public virtual void SaveModel(string filePath)
    {
      // Escrever a string JSON no arquivo
      try
      {
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        var serializedModel = JsonConvert.SerializeObject(qLearning.NeuralNetwork);
        File.WriteAllText(filePath, serializedModel);
        Debug.Log("Model saved in: " + filePath);
      }
      catch (IOException e)
      {
        Debug.LogError("Failed to save model: " + e.Message);
      }
    }

    public virtual void Initialize()
    {
      var neuralNetwork = modelAsset
        ? JsonConvert.DeserializeObject<NeuralNetwork>(modelAsset.text)
        : new NeuralNetwork();

      qLearning = new DeepQLearning(neuralNetwork);

      if (!modelAsset) qLearning.Initialize(layers);

      outputSize = layers[layers.Length - 1];
    }

    void OnEnable()
    {
      Initialize();
    }

    public virtual int FeedForward(float[] state)
    {
      var epsilon = trainMode ? explorationRate : 0f;
      return qLearning.FeedForward(state, epsilon, outputSize);
    }

    public virtual void UpdateQ(float[] state, int action, float reward, float[] nextState)
    {
      averageReward = (averageReward + reward) / 2f;
      qLearning.UpdateQ(state, action, reward, nextState, discountFactor, learningRate);
    }
  }
}