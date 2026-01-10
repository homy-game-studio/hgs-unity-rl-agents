using System.Collections.Generic;
using System.Linq;
using HGS.RLAgents.Evolution;
using UnityEngine;

namespace HGS.RLAgents
{
    public class Academy : MonoBehaviour
    {
        [SerializeField] Generation _generation;
        [SerializeField] AcademyRunner _runner;
        [SerializeField] AcademyEnvironmentReplication _environmentReplication;
        [SerializeField, Range(1f, 15f)] float timescale = 1;

        private List<Environment> _environments = new List<Environment>();

        public int Generations => _runner.Generations;
        public int MaxGenerations => _runner.MaxGenerations;
        public float MaxGenerationDuration
        {
            get => _runner.maxGenerationDuration;
            set => _runner.maxGenerationDuration = value;
        }

        private void Awake()
        {
            Time.timeScale = timescale;
            _environmentReplication.Spawn(transform);
            _runner.onReachTime += FinishGeneration;
        }

        private void Update()
        {
            _runner.Tick(Time.deltaTime);
        }

        private void Start()
        {
            Initialize();
            StartGeneration();
        }

        public void CompleteEnvironmentEpoch(Environment env)
        {
            var allEnvCompleted = _environments.All(env => env.IsCompleted);
            if (allEnvCompleted) FinishGeneration();
        }

        public void AddAgent(Agent agent)
        {
            _generation.AddAgent(agent);
        }

        public void AddEnvironment(Environment env)
        {
            _environments.Add(env);
        }

        private void Initialize()
        {
            _generation.Initialize();
        }

        private void StartGeneration()
        {
            _runner.Restart();
            for (int i = 0; i < _environments.Count; i++)
            {
                _environments[i].StartEpoch();
            }
        }

        private void FinishGeneration()
        {
            _runner.Complete();

            for (int i = 0; i < _environments.Count; i++)
            {
                _environments[i].FinishEpoch();
            }

            if (!_runner.IsReachedMaxGenerations)
            {
                _generation.Tick(Generations, MaxGenerations);
                StartGeneration();
            }
        }
    }
}
