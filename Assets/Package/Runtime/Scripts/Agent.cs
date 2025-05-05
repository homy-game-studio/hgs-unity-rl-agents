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
        public GenerationModel model;

        [Header("Evaluation")]
        public float evaluateInterval = 0.15f;
        public bool active = false;

        private float _timer = 0;

        public float reward = 0;
        public int evaluationCount = 0;

        float[] _lastInput;
        float[] _lastOutput;

        private NeuralNetwork _neuralNetwork;

        public int CromossomeSize => _neuralNetwork.WeightCount + model.neuralNetworkPoint;

        protected virtual void Awake()
        {
            _neuralNetwork = new NeuralNetwork();
            _neuralNetwork.Initialize(model.layers, model.inputSize, model.OutputSize);
        }

        protected abstract float[] GetInput();
        protected abstract void EvaluateOutput(float[] output);

        public virtual void SetCromossome(Cromossome cromossome)
        {
            this.cromossome = cromossome;

            var weights = new float[_neuralNetwork.WeightCount];
            Array.Copy(cromossome.genes, model.neuralNetworkPoint, weights, 0, weights.Length);
            _neuralNetwork.SetWeights(weights);
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