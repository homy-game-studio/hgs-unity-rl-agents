using System;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.NeuralNetworks;
using UnityEngine;
using Newtonsoft.Json;

namespace HGS.RLAgents
{
    public abstract class Agent : MonoBehaviour
    {
        [Header("Evolution")]
        public Model model;
        public TextAsset genome;

        [Header("Evaluation")]
        public float evaluateInterval = 0.15f;
        public bool active = false;

        private float _timer = 0;

        public float fitness = 0;
        public int evaluationCount = 0;

        protected float[] _lastInput;
        protected float[] _lastOutput;

        private NeuralNetwork _neuralNetwork;
        private int _genomeId;

        public float[] LastInput => _lastInput;
        public float[] LastOutput => _lastOutput;

        public Action onEvaluationEnd;

        public int ParametersCount => model.GetParametersCount();
        public NeuralNetwork NeuralNetwork => _neuralNetwork;
        public int GenomeId => _genomeId;

        protected virtual void Awake()
        {
            _neuralNetwork = NeuralNetworkFactory.CreateFromModel(model);

            if (genome)
            {
                var genomeData = JsonConvert.DeserializeObject<Genome>(genome.text);
                SetGenome(-1, genomeData);
                active = true;
            }
        }

        protected abstract float[] CollectObservations();
        protected abstract void EvaluateOutput(float[] output);

        public virtual void SetGenome(int genomeId, Genome genome)
        {
            if (genome.Genes.Length == ParametersCount)
            {
                _genomeId = genomeId;
                _neuralNetwork.SetParameters(genome.Genes);
            }
            else
            {
                Debug.LogWarning($"Genome parameters count ({genome.Genes.Length}) does not match model parameters count ({ParametersCount}).");
            }
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