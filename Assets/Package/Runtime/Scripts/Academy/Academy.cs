using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HGS.RLAgents
{
    public class RewardHistoryEntry
    {
        public int Epoch;
        public string AgentName;
        public float Reward;
    }

    public class Academy : MonoBehaviour
    {
        [SerializeField] AcademyNaturalSelection _naturalSelection;
        [SerializeField] AcademyRunner _runner;        

        private List<Environment> _environments = new List<Environment>();
        private List<Agent> agents = new List<Agent>();

        private void Awake()
        {
            _runner.onReachTime += FinishEpoch;
        }

        private void Update()
        {
            _runner.Tick(Time.deltaTime);
        }

        private void Start()
        {
            StartEpoch();
        }

        public void CompleteEnvironmentEpoch(Environment env)
        {
            var allEnvCompleted = _environments.All(env => env.IsCompleted);
            if (allEnvCompleted) FinishEpoch();
        }

        public void AddAgent(Agent agent)
        {
            agents.Add(agent);
        }

        public void AddEnvironment(Environment env)
        {
            _environments.Add(env);
        }

        private void StartEpoch()
        {
            _naturalSelection.Apply(agents);
            _runner.Restart();
            for (int i = 0; i < _environments.Count; i++)
            {
                _environments[i].StartEpoch();
            }
        }

        private void FinishEpoch()
        {
            _runner.Complete();

            for (int i = 0; i < _environments.Count; i++)
            {
                _environments[i].FinishEpoch();
            }

            if (!_runner.IsReachedMaxEpochs)
            {
                StartEpoch();
            }
        }
    }
}
