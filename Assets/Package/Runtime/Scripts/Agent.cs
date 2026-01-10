using HGS.RLAgents.Evolution;
using HGS.RLAgents.NeuralNetworks;
using UnityEngine;

namespace HGS.RLAgents
{
    public abstract class Agent : MonoBehaviour
    {
        [Header("Evolution")]
        public Cromossome cromossome;
        public Model model;

        [Header("Evaluation")]
        public float evaluateInterval = 0.15f;
        public bool active = false;

        private float _timer = 0;

        public float reward = 0;
        public int evaluationCount = 0;

        float[] _lastInput;
        float[] _lastOutput;

        public int CromossomeSize => model.GetParametersCount();

        private NeuralNetwork _neuralNetwork;

        protected virtual void Awake()
        {
            _neuralNetwork = NeuralNetworkUtility.CreateFromModel(model);
        }

        protected abstract float[] GetInput();
        protected abstract void EvaluateOutput(float[] output);

        public virtual void SetCromossome(Cromossome cromossome)
        {
            this.cromossome = cromossome;
            _neuralNetwork.SetParameters(cromossome.genes);
        }

        protected virtual void FeedFoward()
        {
            _lastInput = GetInput();
            _lastOutput = _neuralNetwork.FeedForward(_lastInput);
            evaluationCount++;
            EvaluateOutput(_lastOutput);
        }

        public abstract void Stop();
        public abstract void Respawn();

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