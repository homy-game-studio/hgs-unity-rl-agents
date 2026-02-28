using System.Collections.Generic;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.Simulation;
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

                evolution.AddPopulation(agentSettings[i].ModelId, phase.populationSize, (int id) =>
                {
                    return CreateIndividual(id, agentSettings[i].ModelParamsCount, phase.mutationStrength);
                });
            }

            evolution.Generation = 0;

            simulation.SetMaxDuration(phase.maxDuration);

            _shouldTick = true;
        }

        void Tick()
        {
            if (evolution.HasCompletedEvaluation)
            {
                RunEvolution();
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

        private void CompleteTraining()
        {
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
