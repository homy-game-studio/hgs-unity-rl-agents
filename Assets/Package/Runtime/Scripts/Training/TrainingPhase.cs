using HGS.RLAgents.Simulation;
using UnityEngine;

namespace HGS.RLAgents.Training
{
    [System.Serializable]
    public class TrainingPhase
    {
        public string name;

        [Header("Evolution")]
        public int generations = 100;
        public int populationSize = 1000;
        public float selectionRate = 0.01f;
        public float crossoverRate = 0.5f;

        [Header("Mutation")]
        public float mutationRate = 0.1f;
        public float mutationStrength = 0.1f;

        [Header("Simulation")]
        public GameObject environmentPrefab;
        public float environmentSpacing = 0;
        public float maxDuration = 10f;
        public int maxRenderers = 40;
        public int maxAgents = 400;
    }
}
