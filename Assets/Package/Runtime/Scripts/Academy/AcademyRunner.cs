using System;
using UnityEngine;

namespace HGS.RLAgents
{
    [Serializable]
    public class AcademyRunner
    {
        [SerializeField] int maxGenerations = 10;
        public float maxGenerationDuration = 5f;

        float _timer;

        public Action onReachTime;

        public bool IsRunning { get; set; }
        public bool IsReachedMaxGenerations => Generations >= maxGenerations;
        public int Generations { get; set; }
        public int MaxGenerations => maxGenerations;

        public void Restart()
        {
            IsRunning = true;
            _timer = 0;
        }

        public void Complete()
        {
            Generations++;
            IsRunning = false;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;
            if (_timer >= maxGenerationDuration)
            {
                IsRunning = false;
                onReachTime?.Invoke();
            }

            _timer += deltaTime;
        }
    }
}
