using System;
using UnityEngine;

namespace HGS.RLAgents.Simulation
{
    public abstract class SimulationEnvironment : MonoBehaviour
    {
        [HideInInspector]
        public float maxEpochDuration;


        protected float elapsedTime;

        protected bool isFinished;

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

        public abstract void EvaluateFitness();

        protected virtual void Update()
        {
            if (!isFinished)
            {
                elapsedTime += Time.deltaTime;
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
