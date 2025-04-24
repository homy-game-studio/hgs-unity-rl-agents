using System;
using UnityEngine;

namespace HGS.RLAgents
{
    [Serializable]
    public class AcademyRunner
    {
        [SerializeField] int maxEpochs = 10;
        [SerializeField] float maxEpochDuration = 5f;

        float _timer;

        public Action onReachTime;

        public bool IsRunning { get; set; }
        public bool IsReachedMaxEpochs => Epoch >= maxEpochs;
        public int Epoch { get; set; }

        public void Restart()
        {
            IsRunning = true;
            _timer = 0;
        }

        public void Complete()
        {
            Epoch++;
            IsRunning = false;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;
            if (_timer >= maxEpochDuration)
            {
                IsRunning = false;
                onReachTime?.Invoke();
            }

            _timer += deltaTime;
        }
    }
}
