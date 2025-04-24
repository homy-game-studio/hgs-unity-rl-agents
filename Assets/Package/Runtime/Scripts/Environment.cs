using UnityEngine;

namespace HGS.RLAgents
{
    public abstract class Environment : MonoBehaviour
    {
        public float learningRate = 0.1f;

        protected Academy academy;

        public bool IsCompleted { get; set; }

        protected abstract void EvaluateReward();

        protected virtual void Awake()
        {
            academy = FindAnyObjectByType<Academy>();
            academy.AddEnvironment(this);
        }

        protected void CompleteEpoch()
        {
            if (IsCompleted) return;

            IsCompleted = true;
            academy.CompleteEnvironmentEpoch(this);
        }

        public void FinishEpoch()
        {
            IsCompleted = true;
            EvaluateReward();
            OnFinishEpoch();
        }

        public void StartEpoch()
        {
            IsCompleted = false;
            OnStartEpoch();
        }

        protected virtual void OnFinishEpoch() { }
        protected virtual void OnStartEpoch() { }
    }
}
