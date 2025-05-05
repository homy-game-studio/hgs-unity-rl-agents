using UnityEngine;

namespace HGS.RLAgents
{
    public abstract class Policy : MonoBehaviour
    {
        public abstract void EvaluateReward();
        public virtual void TransitionIn() { }
        public virtual void TransitionOut() { }
        public virtual void StartEpoch() { }
        public virtual void FinishEpoch() { }
    }
}
