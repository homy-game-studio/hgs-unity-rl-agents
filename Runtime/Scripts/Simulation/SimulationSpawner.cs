using System;
using UnityEngine;

namespace HGS.RLAgents.Simulation
{
    public class SimulationSpawner
    {
        public SimulationEnvironment[] Spawn(GameObject envPrefab, Transform container, int numberOfRenderers, int numberOfInstances, float spaceBetween, Action<SimulationEnvironment> onFinishEpoch)
        {
            SimulationEnvironment[] environments = new SimulationEnvironment[numberOfInstances];
            Vector3 basePosition = envPrefab.transform.position;
            Quaternion rotation = envPrefab.transform.rotation;

            for (int i = 0; i < numberOfInstances; i++)
            {
                var position = basePosition + Vector3.right * (i * spaceBetween);
                var instance = GameObject.Instantiate(envPrefab, position, rotation, container);
                instance.SetActive(true);
                var env = instance.GetComponent<SimulationEnvironment>();
                env.onFinishEpoch += () => onFinishEpoch(env);
                env.ToggleRenderer(i < numberOfRenderers);
                environments[i] = env;
            }

            return environments;
        }
    }
}
