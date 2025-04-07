using UnityEngine;

namespace HGS.RLAgents.Agents
{
    public abstract class Agent : MonoBehaviour
    {
        public Model model;
        public float interval = 0.15f;
        public bool active = false;
        private float _timer = 0;
        public float reward = 0;

        AgentEnvironment env;

        public void Initialize()
        {
            model.Initialize();
        }

        public virtual void Restart()
        {
            reward = 0;
        }

        protected abstract float[] GetInput();
        protected abstract void ProcessOutput(float[] output);

        protected virtual void FeedFoward()
        {
            var _input = GetInput();
            var _output = model.FeedForward(_input);
            ProcessOutput(_output);
        }

        protected virtual void Update()
        {
            if (!active) return;
            if (_timer == 0)
            {
                FeedFoward();
            }

            if (_timer >= interval)
            {
                _timer = 0;
            }
            else
            {
                _timer += Time.deltaTime;
            }
        }
    }
}