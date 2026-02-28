using System;
using System.Collections.Generic;
using UnityEngine;

namespace HGS.RLAgents.Simulation
{
    public class SimulationAgentSettings
    {
        public string ModelId { get; set; }
        public int ModelParamsCount { get; set; }
    }

    [Serializable]
    public class SimulationEngine
    {
        SimulationSpawner _spawner;
        private Queue<SimulationEnvironment> _availableEnvironments;

        public bool HasEnvironments => _availableEnvironments.Count > 0;

        public int EnvCount => _availableEnvironments.Count;

        SimulationEngine()
        {
            _spawner = new SimulationSpawner();
            _availableEnvironments = new Queue<SimulationEnvironment>();
        }

        public List<SimulationAgentSettings> ExtractAgentSettings()
        {
            var agentSettings = new List<SimulationAgentSettings>();
            var sample = _availableEnvironments.Peek();

            for (int i = 0; i < sample.Agents.Length; i++)
            {
                var agentId = sample.Agents[i].model.id;
                var agentParamsCount = sample.Agents[i].model.GetParametersCount();
                agentSettings.Add(new SimulationAgentSettings
                {
                    ModelId = agentId,
                    ModelParamsCount = agentParamsCount,
                });
            }

            return agentSettings;
        }
 

        public void Spawn(GameObject envPrefab, Transform container, int numberOfRenderers, int numberOfInstances, float spaceBetween, Action<SimulationEnvironment> onFinishEpoch)
        {
            SimulationEnvironment[] simulationEnvironments = _spawner.Spawn(envPrefab, container, numberOfRenderers, numberOfInstances, spaceBetween, onFinishEpoch);
            _availableEnvironments = new Queue<SimulationEnvironment>(simulationEnvironments);
        }

        public void Clear()
        {
            while (_availableEnvironments.Count > 0)
            {
                var env = _availableEnvironments.Dequeue();
                GameObject.Destroy(env.gameObject);
            }
        }

        public void SetMaxDuration(float value)
        {
            foreach (var env in _availableEnvironments)
            {
                env.maxEpochDuration = value;
            }
        }

        public void AddEnvironment(SimulationEnvironment env)
        {
            _availableEnvironments.Enqueue(env);
        }

        public SimulationEnvironment NextEnvironment()
        {
            if (_availableEnvironments.Count == 0) return null;
            return _availableEnvironments.Dequeue();
        }
    }
}
