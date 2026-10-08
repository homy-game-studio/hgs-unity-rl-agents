using System.Collections.Generic;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.Simulation;
using Newtonsoft.Json;
using UnityEngine;

namespace HGS.RLAgents.Training
{
    public class TrainingManager : MonoBehaviour
    {
        [SerializeField] List<TrainingPhase> phases;
        [SerializeField] EvolutionEngine evolution;
        [SerializeField] SimulationEngine simulation;
        [SerializeField] TrainingDebugger debugger;

        int currentPhaseIndex = 0;
        bool _shouldTick = false;

        void Start()
        {
            RunPhase(phases[currentPhaseIndex]);
        }

        void Update()
        {
            if (_shouldTick)
            {
                _shouldTick = false;
                Tick();
            }
        }

        public void RunPhase(TrainingPhase phase)
        {
            simulation.Clear();
            simulation.Spawn(phase.environmentPrefab, transform, phase.maxRenderers, phase.maxAgents, phase.environmentSpacing, OnEnvEpochFinish);

            var agentSettings = simulation.ExtractAgentSettings();

            for (int i = 0; i < agentSettings.Count; i++)
            {
                // Preserve old population and genomes
                if (evolution.HasPopulation(agentSettings[i].ModelId)) continue;

                var seed = LoadSeed(agentSettings[i]);

                evolution.AddPopulation(agentSettings[i].ModelId, phase.populationSize, (int id) =>
                {
                    return seed.HasValue
                        ? CreateSeededIndividual(id, seed.Value, phase.mutationRate, phase.mutationStrength)
                        : CreateIndividual(id, agentSettings[i].ModelParamsCount, phase.mutationStrength);
                });
            }

            evolution.Generation = 0;

            simulation.SetMaxDuration(phase.maxDuration);
            simulation.SetDifficulty(phase.difficulty);
            Time.timeScale = phase.timeScale;

            _shouldTick = true;
        }

        void Tick()
        {
            if (evolution.HasCompletedEvaluation)
            {
                RunEvolution();
                ApplyDifficultyRamp(phases[currentPhaseIndex]);
            }

            if (evolution.Generation >= phases[currentPhaseIndex].generations)
            {
                currentPhaseIndex++;

                if (currentPhaseIndex >= phases.Count)
                {
                    CompleteTraining();
                    return;
                }

                if (currentPhaseIndex < phases.Count)
                {
                    RunPhase(phases[currentPhaseIndex]);
                }
                return;
            }

            while (simulation.HasEnvironments && evolution.HasPendingEvaluations)
            {
                RunSimulation();
            }

            debugger.Update(simulation, evolution, currentPhaseIndex, phases);
        }

        // Every environment is free here (all individuals were evaluated), so the new difficulty
        // applies to the whole next generation
        void ApplyDifficultyRamp(TrainingPhase phase)
        {
            if (!phase.rampDifficulty) return;

            float progress = phase.generations > 0 ? Mathf.Clamp01(evolution.Generation / (float)phase.generations) : 1f;
            float difficulty = Mathf.Lerp(phase.difficulty, phase.difficultyEnd, progress);
            simulation.SetDifficulty(difficulty);
            Debug.Log($"[Training] phase '{phase.name}' generation {evolution.Generation}/{phase.generations} | difficulty {difficulty:F3}");
        }

        private void CompleteTraining()
        {
            Time.timeScale = 1f;
            Debug.Log("Training complete!");
        }

        public void RunEvolution()
        {
            var phase = phases[currentPhaseIndex];
            evolution.Evolve(
                phase.selectionRate,
                phase.crossoverRate,
                phase.mutationRate,
                phase.mutationStrength
            );
            evolution.Save(currentPhaseIndex);
        }

        private void RunSimulation()
        {
            SimulationEnvironment env = simulation.NextEnvironment();

            for (int i = 0; i < env.Agents.Length; i++)
            {
                var individual = evolution.NextIndividual(env.Agents[i].model.id);
                env.Agents[i].SetGenome(individual.Id, individual.Genome);
            }

            env.StartEpoch();
        }

        // Model.seedGenome, if it is set and matches the model. A mismatch is a warning, not a crash:
        // the population falls back to random weights.
        private Genome? LoadSeed(SimulationAgentSettings settings)
        {
            if (settings.SeedGenome == null) return null;

            var genome = JsonConvert.DeserializeObject<Genome>(settings.SeedGenome.text);
            if (genome.Genes == null || genome.Genes.Length != settings.ModelParamsCount)
            {
                Debug.LogWarning($"[Training] seed genome of '{settings.ModelId}' has {genome.Genes?.Length ?? 0} genes but the model needs {settings.ModelParamsCount}, using random weights.");
                return null;
            }

            Debug.Log($"[Training] population '{settings.ModelId}' starts from its seed genome");
            return genome;
        }

        // Individual 0 is the seed untouched, the others are mutated copies of it
        private Individual CreateSeededIndividual(int id, Genome seed, float mutationRate, float mutationStrength)
        {
            var genome = new Genome(seed.Genes.Length);
            System.Array.Copy(seed.Genes, genome.Genes, seed.Genes.Length);

            if (id > 0)
            {
                for (int i = 0; i < genome.Genes.Length; i++)
                {
                    if (Rand.Linear() < mutationRate) genome.Genes[i] += Rand.Gaussian(0f, mutationStrength);
                }
            }

            return new Individual
            {
                Id = id,
                Genome = genome,
                Fitness = 0,
                State = EvaluationState.Pending,
            };
        }

        private Individual CreateIndividual(int id, int gnomeSize, float strength)
        {
            Genome genome = new Genome(gnomeSize);
            genome.Seed(strength);

            return new Individual
            {
                Id = id,
                Genome = genome,
                Fitness = 0,
                State = EvaluationState.Pending,
            };
        }

        private void OnEnvEpochFinish(SimulationEnvironment simulationEnv)
        {
            var agents = simulationEnv.Agents;
            for (int i = 0; i < agents.Length; i++)
            {
                var agent = agents[i];
                evolution.Evaluate(agent.model.id, agent.GenomeId, agent.fitness);
            }

            simulation.AddEnvironment(simulationEnv);
            _shouldTick = true;
        }

        private void OnGUI()
        {
            if (debugger != null)
            {
                debugger.Draw();
            }
        }
    }
}
