using System;
using UnityEngine;

namespace HGS.RLAgents.Simulation
{
    public abstract class SimulationEnvironment : MonoBehaviour
    {
        [HideInInspector]
        public float maxEpochDuration;


        protected float elapsedTime;

        // Idle until StartEpoch: an environment waiting in the queue must not count time
        protected bool isFinished = true;

        Agent[] _agents;
        public Agent[] Agents
        {
            get
            {
                if (_agents == null || _agents.Length == 0)
                {
                    _agents = transform.GetComponentsInChildren<Agent>();
                }
                return _agents;
            }
        }

        public Action onFinishEpoch;

        // Curriculum hook: 0 = easiest, 1 = full difficulty. Applied on the next StartEpoch.
        public float Difficulty { get; private set; } = 1f;

        public virtual void SetDifficulty(float value)
        {
            Difficulty = Mathf.Clamp01(value);
        }

        public abstract void EvaluateFitness();

        // Fixed step keeps the epoch length identical (in physics steps) for every evaluation
        protected virtual void FixedUpdate()
        {
            if (!isFinished)
            {
                elapsedTime += Time.fixedDeltaTime;
                if (elapsedTime >= maxEpochDuration)
                {
                    FinishEpoch();
                }
            }
        }

        public void FinishEpoch()
        {
            if (isFinished) return;
            isFinished = true;

            foreach (var agent in Agents)
            {
                agent.active = false;
                agent.Stop();
            }

            EvaluateFitness();
            OnFinishEpoch();
            elapsedTime = 0;
            onFinishEpoch?.Invoke();
        }

        public void StartEpoch()
        {
            isFinished = false;
            elapsedTime = 0;

            foreach (var agent in Agents)
            {
                agent.active = true;
                agent.fitness = 0;
                agent.evaluationCount = 0;
                agent.Respawn();
            }
            OnStartEpoch();
        }

        public void ToggleRenderer(bool value)
        {
            var renderes = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderes.Length; i++)
            {
                renderes[i].enabled = value;
            }
        }

        protected virtual void OnFinishEpoch() { }
        protected virtual void OnStartEpoch() { }
    }
}
