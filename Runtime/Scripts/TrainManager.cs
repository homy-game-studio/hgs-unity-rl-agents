using System.Collections.Generic;
using HGS.RLAgents.Agents;
using UnityEngine;

namespace HGS.RLAgents
{
    public class TrainManager : MonoBehaviour
    {
        [SerializeField] List<AgentEnvironment> environments = new List<AgentEnvironment>();
        [SerializeField, Range(0.5f, 30f)] float timeScale = 1.0f;

        private int _index = -1;

        private void Awake()
        {
            Next();
        }

        private void Update()
        {
            Time.timeScale = timeScale;
        }

        private void Next()
        {
            _index++;

            if (_index > 0)
            {
                environments[_index].betterModel = environments[_index - 1].betterModel;
            }

            environments[_index].onFinish += OnEnvironmentFinish;
            environments[_index].Run();
        }

        private void OnEnvironmentFinish(Agent betterAgent)
        {
            if (_index + 1 < environments.Count)
            {
                Next();
            }
            else
            {
                Debug.Log("Treino concluído!");
            }
        }
    }
}
