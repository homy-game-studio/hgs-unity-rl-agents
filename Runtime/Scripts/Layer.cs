using System;

namespace HGS.RLAgents
{
  [Serializable]
  public class Layer
  {
    public int InputSize { get; set; }
    public int OutputSize { get; set; }
    public float[,] Weights { get; set; }
    public float[] Biases { get; set; }

    public void Initialize()
    {
      Biases = new float[OutputSize];
      Weights = new float[InputSize, OutputSize];

      for (int i = 0; i < OutputSize; i++)
      {
        Biases[i] = UnityEngine.Random.Range(-1f, 1f);
      }

      for (int i = 0; i < InputSize; i++)
      {
        for (int j = 0; j < OutputSize; j++)
        {
          Weights[i, j] = UnityEngine.Random.Range(-1f, 1f);
        }
      }
    }

    public void UpdateWeightsAndBiases(float[] deltaWeights, float[] deltaBiases)
    {
      for (int i = 0; i < OutputSize; i++)
      {
        for (int j = 0; j < InputSize; j++)
        {
          Weights[j, i] += deltaWeights[i * InputSize + j];
        }
        Biases[i] += deltaBiases[i];
      }
    }

    public float[] CalculateGradient(float[] prevLayerGradient, float[] prevLayerOutput)
    {
      float[] gradient = new float[InputSize];
      for (int i = 0; i < InputSize; i++)
      {
        float sum = 0f;
        for (int j = 0; j < OutputSize; j++)
        {
          sum += prevLayerGradient[j] * Weights[i, j];
        }
        gradient[i] = sum * Sigmoid.Derivative(prevLayerOutput[i]);
      }
      return gradient;
    }

    public float[] FeedForward(float[] input)
    {
      var output = new float[OutputSize];

      for (int i = 0; i < OutputSize; i++)
      {
        var sum = Biases[i];
        for (int j = 0; j < InputSize; j++)
        {
          sum += input[j] * Weights[j, i];
        }
        output[i] = Sigmoid.Activate(sum);
      }

      return output;
    }
  }
}