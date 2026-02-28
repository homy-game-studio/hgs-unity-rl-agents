using System;
using System.Collections.Generic;
using System.Linq;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.Simulation;
using UnityEngine;

namespace HGS.RLAgents.Training
{
    class TrainingDebuggerPopulation
    {
        public int evaluatedIndividuals;
        public Dictionary<int, float> avgEliteFitnessHistory;
        public Dictionary<int, float> bestFitnessHistory;

        public void SetGenerationData(int generation, float avgEliteFitness, float bestFitness)
        {
            // adiciona de traz para frente e garante que só existam 5
            avgEliteFitnessHistory[generation] = avgEliteFitness;
            if (avgEliteFitnessHistory.Count > 10)
                avgEliteFitnessHistory.Remove(avgEliteFitnessHistory.Keys.Min());
            bestFitnessHistory[generation] = bestFitness;
            if (bestFitnessHistory.Count > 10)
                bestFitnessHistory.Remove(bestFitnessHistory.Keys.Min());
        }

        public void SetEvaluatedIndividuals(int count)
        {
            evaluatedIndividuals = count;
        }
    }

    [Serializable]
    public class TrainingDebugger
    {
        string[] populationIds;
        Dictionary<string, TrainingDebuggerPopulation> populations;
        int populationSize = 0;
        int generation = 0;
        int generations = 0;
        int phasesCount = 0;
        int currentPhase = 0;

        float fontSize = 24;
        Color fontColor = Color.black;

        TrainingDebugger()
        {
            populationIds = new string[0];
            populations = new Dictionary<string, TrainingDebuggerPopulation>();
        }

        public void Update(SimulationEngine simulation, EvolutionEngine evolution, int currentPhaseIndex, List<TrainingPhase> phases)
        {
            var isPhaseChanged = currentPhaseIndex != currentPhase;
            var isGenerationChanged = generation != evolution.Generation;
            currentPhase = currentPhaseIndex;
            phasesCount = phases.Count;
            var phase = phases[currentPhaseIndex];
            populationSize = phase.populationSize;
            generation = evolution.Generation;
            generations = phase.generations;
            populationIds = evolution.populations.Keys.ToArray();

            if (isPhaseChanged)
            {
                populations.Clear();
            }

            for (int i = 0; i < populationIds.Length; i++)
            {
                var pId = populationIds[i];

                if (!populations.ContainsKey(pId))
                {
                    populations[pId] = new TrainingDebuggerPopulation
                    {
                        evaluatedIndividuals = 0,
                        avgEliteFitnessHistory = new Dictionary<int, float>(),
                        bestFitnessHistory = new Dictionary<int, float>(),
                    };
                }

                populations[pId].SetEvaluatedIndividuals(evolution.populations[pId].EvaluatedCount);

                if (isGenerationChanged)
                {
                    var avgEliteFitness = evolution.populations[pId].AvgEliteFitness;
                    var bestFitness = evolution.populations[pId].BestFitness;
                    populations[pId].SetGenerationData(generation, avgEliteFitness, bestFitness);
                }
            }
        }

        void DrawField(string title, object value)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = (int)fontSize;
            style.normal.textColor = fontColor;
            style.fontSize = (int)fontSize;

            GUILayout.Label($"{title}: {value}", style);
        }

        void DrawTitle(string title)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = (int)(fontSize * 1.2f);
            style.normal.textColor = fontColor;
            style.normal.background = Texture2D.whiteTexture;
            style.fontSize = (int)(fontSize * 1.2f);

            GUILayout.Label(title, style);
        }

        void DrawPopulation(string populationId)
        {
            var population = populations[populationId];
            GUILayout.BeginVertical();

            DrawTitle(populationId);
            DrawField("Evaluated", $"{population.evaluatedIndividuals}/{populationSize}");

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            DrawTitle("Best Fitness");
            foreach (var data in population.bestFitnessHistory)
            {
                DrawField($"Gen {data.Key}", data.Value);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            DrawTitle("Avg Elite Fitness");
            foreach (var data in population.avgEliteFitnessHistory)
            {
                DrawField($"Gen {data.Key}", data.Value);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        public void Draw()
        {
            GUI.BeginGroup(new Rect(10, 10, 520, Screen.height));

            GUILayout.BeginVertical();

            DrawField("Phase", $"{currentPhase}/{phasesCount}");
            DrawField("Generation", $"{generation}/{generations}");

            GUILayout.BeginHorizontal();

            for (int i = 0; i < populationIds.Length; i++)
            {
                DrawPopulation(populationIds[i]);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.EndGroup();
        }
    }
}
