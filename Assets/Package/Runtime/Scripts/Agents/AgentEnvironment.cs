using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HGS.RLAgents.Agents
{
    public class AgentEnvironment : MonoBehaviour
    {
        [SerializeField] GameObject agentPrefab;
        [SerializeField] Transform agentSpawnPoint;
        [SerializeField] float learningRate = 0.1f;
        [SerializeField] int agentsByEpoch = 10;
        [SerializeField] int epochs = 10;
        [SerializeField] float maxEpochDuration = 5f;

        protected List<Agent> agents = new List<Agent>();

        private int _completedAgents = 0;
        public int epoch = 0;
        float _timer;

        public Model betterModel = null;

        public bool IsComplete => epoch == epochs;
        public bool IsRunning { get; private set; } = false;

        public Action<Agent> onFinish;

        public void CompleteAgentEpoch()
        {
            _completedAgents++;
            if (_completedAgents == agentsByEpoch)
            {
                FinishEpoch();
            }
        }

        public virtual void Run()
        {
            IsRunning = true;
            StartEpoch();
        }

        public void Finish(Agent betterAgent)
        {
            Clear();
            IsRunning = false;
            onFinish?.Invoke(betterAgent);
        }

        public void Clear()
        {
            var agentsToRemove = new Queue<Agent>(agents);

            while (agentsToRemove.Count > 0)
            {
                var agent = agentsToRemove.Dequeue();
                RemoveAgent(agent);
            }
        }

        private void Update()
        {
            if (!IsRunning) return;

            if (_timer >= maxEpochDuration)
            {
                FinishEpoch();
            }

            _timer += Time.deltaTime;
        }

        public virtual void FinishEpoch()
        {
            _completedAgents = 0;
            var betterAgent = FindBetterAgent();
            var betterReward = betterAgent.reward;

            if (betterReward > 0)
            {
                betterModel = Model.Instantiate(betterAgent.model);
                betterModel.name = "Model-reward-" + betterReward;
            }

            Clear();

            Debug.Log($"Epoch: {epoch}, Reward: {betterReward}");

            if (epoch >= epochs)
            {
                Finish(betterAgent);
            }
            else
            {
                StartEpoch();
            }

            _timer = 0;
        }

        public void StartEpoch()
        {
            var hasBetterModel = betterModel != null;
            var lenght = hasBetterModel ? agentsByEpoch - 1 : agentsByEpoch;

            if (hasBetterModel) AddAgent(betterModel, false);

            for (int i = 0; i < lenght; i++)
            {
                if (hasBetterModel)
                {
                    AddAgent(betterModel);
                }
                else
                {
                    AddAgent();
                }
            }

            epoch++;
        }

        public Agent FindBetterAgent()
        {
            return agents
                .OrderByDescending(agent => agent.reward)
                .FirstOrDefault();
        }

        public Agent SpawnAgent()
        {
            var go = Instantiate(agentPrefab);
            var agent = go.GetComponent<Agent>();
            var clonnedModel = Model.Instantiate(agent.model);

            clonnedModel.name = $"Model e-{epoch}";

            agent.transform.position = agentSpawnPoint.position;
            agent.transform.eulerAngles = agentSpawnPoint.eulerAngles;
            agent.transform.localScale = agentSpawnPoint.localScale;
            agent.name = $"[{name}-{epoch}] {agentPrefab.name}";
            agent.model = clonnedModel;
            agent.active = true;
            agent.Environment = this;

            agent.Initialize();
            agent.Restart();

            return agent;
        }

        public Agent AddAgent()
        {
            var agent = SpawnAgent();

            agent.Initialize();
            agent.model.RandomizeWeights();
            agents.Add(agent);

            return agent;
        }

        public Agent AddAgent(Model baseModel, bool randomize = true)
        {
            var agent = SpawnAgent();

            agent.model = Model.Instantiate(baseModel);
            if (randomize) agent.model.RandomizeWeights(learningRate);
            agents.Add(agent);

            return agent;
        }

        public void RemoveAgent(Agent agent)
        {
            agent.active = false;
            agents.Remove(agent);
            Destroy(agent.gameObject);
        }
    }
}
