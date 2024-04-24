using UnityEngine;

namespace HGS.RLAgents
{
  public abstract class Agent : MonoBehaviour
  {
    public Brain brain;
    [SerializeField] float decisionInterval = 0.15f;
    [SerializeField] int epochs = 10;
    [SerializeField] protected float maxEpochDuration = 5f;

    private float _epochTimer = 0;
    private float _timer = 0;
    private float[] _initialState;
    private int _action;

    protected abstract void Restart();
    protected abstract float[] GetState();
    protected abstract float GetReward();
    protected abstract void ExecuteAction(int action);

    protected virtual void FinishEpoch()
    {
      _epochTimer = 0f;
      Restart();
    }

    protected virtual void StartDecision()
    {
      _initialState = GetState();
      _action = brain.FeedForward(_initialState);
      ExecuteAction(_action);
    }

    protected virtual void EndDecision()
    {
      if (!brain.trainMode) return;

      var nextState = GetState();
      var reward = GetReward();
      brain.UpdateQ(_initialState, _action, reward, nextState);
    }

    protected virtual void Update()
    {
      if (_timer == 0)
      {
        StartDecision();
      }

      if (_timer >= decisionInterval)
      {
        EndDecision();
        _timer = 0;
      }
      else
      {
        _timer += Time.deltaTime;
      }

      if (brain.trainMode)
      {
        _epochTimer += Time.deltaTime;
        if (_epochTimer > maxEpochDuration)
        {
          FinishEpoch();
        }
      }
    }
  }
}