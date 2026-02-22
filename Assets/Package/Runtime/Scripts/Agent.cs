using System;
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
        public bool loadCromossomeOnAwake= false;

        [Header("Evaluation")]
        public float evaluateInterval = 0.15f;
        public bool active = false;

        private float _timer = 0;

        public float reward = 0;
        public int evaluationCount = 0;

        protected float[] _lastInput;
        protected float[] _lastOutput;

        private NeuralNetwork _neuralNetwork;

        public float[] LastInput => _lastInput;
        public float[] LastOutput => _lastOutput;

        public Action onEvaluationEnd;

        public int CromossomeSize => model.GetParametersCount();
        public NeuralNetwork NeuralNetwork => _neuralNetwork;

        protected virtual void Awake()
        {
            _neuralNetwork = NeuralNetworkUtility.CreateFromModel(model);
            if (loadCromossomeOnAwake)
            {
                var loadedCromossome = model.LoadCromossome();
                SetCromossome(loadedCromossome);
            }
        }

        protected abstract float[] CollectObservations();
        protected abstract void EvaluateOutput(float[] output);

        public virtual void SetCromossome(Cromossome cromossome)
        {
            this.cromossome = cromossome;
            _neuralNetwork.SetParameters(cromossome.genes);
        }

        protected virtual void FeedFoward()
        {
            _lastInput = CollectObservations();
            _lastOutput = _neuralNetwork.FeedForward(_lastInput);
            evaluationCount++;
            EvaluateOutput(_lastOutput);
            onEvaluationEnd?.Invoke();
        }

        public void ToggleRenderer(bool value)
        {
            var renderes = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderes.Length; i++)
            {
                renderes[i].enabled = value;
            }
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