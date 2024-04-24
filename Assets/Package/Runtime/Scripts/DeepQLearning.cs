using UnityEngine;

namespace HGS.RLAgents
{
  public class DeepQLearning
  {
    public NeuralNetwork NeuralNetwork { get; set; }

    public DeepQLearning(NeuralNetwork neuralNetwork)
    {
      NeuralNetwork = neuralNetwork;
    }

    public void Initialize(int[] layerSizes)
    {
      NeuralNetwork.Initialize(layerSizes);
    }

    public void UpdateQ(float[] state, int action, float reward, float[] nextState, float gamma, float alpha)
    {
      var qValuesNextState = NeuralNetwork.FeedForward(nextState);
      var maxNextQValue = Mathf.Max(qValuesNextState);
      var targetQValue = reward + gamma * maxNextQValue;
      var targetOutput = qValuesNextState;

      targetOutput[action] = targetQValue;

      NeuralNetwork.Backpropagation(state, targetOutput, alpha);
    }

    public int MaxQAction(float[] values)
    {
      var maxQ = float.MinValue;
      var batterAction = 0;
      for (int i = 0; i < values.Length; i++)
      {
        if (values[i] > maxQ)
        {
          maxQ = values[i];
          batterAction = i;
        }
      }
      return batterAction;
    }

    public int FeedForward(float[] state, float epsilon, int outputSize)
    {
      if (Random.Range(0f, 1f) < epsilon)
      {
        return Random.Range(0, outputSize);
      }

      var output = NeuralNetwork.FeedForward(state);

      return MaxQAction(output);
    }
  }
}