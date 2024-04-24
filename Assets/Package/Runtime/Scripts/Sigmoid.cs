using UnityEngine;

namespace HGS.RLAgents
{
  public static class Sigmoid
  {
    public static float Activate(float x)
    {
      return 1f / (1f + Mathf.Exp(-x));
    }

    public static float Derivative(float x)
    {
      return x * (1f - x);
    }
  }
}