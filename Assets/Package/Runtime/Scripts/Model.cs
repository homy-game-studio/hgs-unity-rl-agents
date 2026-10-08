using UnityEngine;
using System;
using HGS.RLAgents.NeuralNetworks;

namespace HGS.RLAgents
{
    // Values are serialized in the model assets: append new types, never reorder
    public enum ELayerType
    {
        Dense = 0,
        Elman = 1,
        LSTM = 2,
        GRU = 3,
        FrameStack = 4,
    }

    [Serializable]
    public class ModelLayer
    {
        public ELayerType type;

        [Tooltip("Neurons (Dense, Elman) or memory units (LSTM, GRU). Ignored by FrameStack.")]
        public int size;

        [Tooltip("Used by Dense and Elman. LSTM and GRU have fixed gate activations. Prefer Tanh on Elman.")]
        public EActivation activation;

        [Tooltip("FrameStack only: how many of the last inputs are kept (output = input size * frames)")]
        public int frames = 4;
    }

    [CreateAssetMenu(fileName = "Model", menuName = "HGS/RLAgents/Model")]
    public class Model : ScriptableObject
    {
        public string id;
        public int neuralNetworkPoint = 3;
        public ModelLayer[] layers;
        public int inputSize;

        [Tooltip("Optional starting genome (e.g. from ImitationTrainer). A new population starts as copies of it, mutated, instead of random weights. Must match this model's parameter count.")]
        public TextAsset seedGenome;

        public int GetParametersCount()
        {
            int count = 0;
            int previousSize = inputSize;

            foreach (var layer in layers)
            {
                count += NeuralNetworkFactory.CountParameters(previousSize, layer);
                previousSize = NeuralNetworkFactory.GetOutputSize(previousSize, layer);
            }

            return count;
        }
    }
}