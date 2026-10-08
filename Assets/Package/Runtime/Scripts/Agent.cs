using System;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.NeuralNetworks;
using UnityEngine;
using Newtonsoft.Json;

namespace HGS.RLAgents
{
    // Runs before the physics scripts so a decision is applied in the same fixed step
    [DefaultExecutionOrder(-10)]
    public abstract class Agent : MonoBehaviour
    {
        [Header("Evolution")]
        public Model model;
        public TextAsset genome;

        [Header("Evaluation")]
        public float evaluateInterval = 0.15f;
        public bool active = false;

        private int _stepCounter = 0;
        private bool _wasActive = false;

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

        public abstract void Stop();
        public abstract void Respawn();

        // The action currently applied, in the same space as the network output (what a human is doing
        // in heuristic mode). Needed by DemonstrationRecorder; null = this agent cannot be recorded.
        public virtual float[] GetAppliedAction() => null;

        // Where a new demonstration starts. Override to vary the start pose.
        public virtual void RespawnForDemonstration() => Respawn();

        // Kept so subclasses can keep overriding it (input, visuals); decisions run in FixedUpdate
        protected virtual void Update() { }

        // Deciding once every N physics steps (not N seconds of frame time) makes a genome
        // behave the same regardless of the frame rate.
        protected virtual void FixedUpdate()
        {
            if (!active)
            {
                _wasActive = false;
                return;
            }

            if (!_wasActive)
            {
                _wasActive = true;
                _stepCounter = 0;

                // A new episode must not inherit the memory of the previous one
                _neuralNetwork.ResetState();
            }

            if (_stepCounter == 0)
            {
                FeedFoward();
            }

            var stepsPerDecision = Mathf.Max(1, Mathf.RoundToInt(evaluateInterval / Time.fixedDeltaTime));
            _stepCounter = (_stepCounter + 1) % stepsPerDecision;
        }
    }
}