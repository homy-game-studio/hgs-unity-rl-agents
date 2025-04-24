using UnityEngine;

namespace HGS.RLAgents
{
    public abstract class Agent : MonoBehaviour
    {
        [Header("Evaluate Settings")]
        public Model model;
        public float evaluateInterval = 0.15f;
        public bool active = false;

        private float _timer = 0;

        public float reward = 0;
        public float learningRate = 0;
        public int evaluationCount = 0;


        Academy _academy;

        protected virtual void Awake()
        {
            _academy = FindAnyObjectByType<Academy>();
            _academy.AddAgent(this);

            var modelName = model.name;

            model = Model.Instantiate(model);
            model.name = modelName;
            model.Initialize();
        }

        protected abstract float[] GetInput();
        protected abstract void EvaluateOutput(float[] output);

        protected virtual void FeedFoward()
        {
            var _input = GetInput();
            var _output = model.FeedForward(_input);
            evaluationCount++;
            EvaluateOutput(_output);
        }

        protected virtual void Update()
        {
            if (!active) return;
            if (_timer == 0)
            {
                FeedFoward();
            }

            if (_timer >= evaluateInterval)
            {
                _timer = 0;
            }
            else
            {
                _timer += Time.deltaTime;
            }
        }
    }
}