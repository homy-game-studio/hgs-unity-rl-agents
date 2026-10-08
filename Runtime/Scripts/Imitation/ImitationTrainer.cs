using System.Collections;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.NeuralNetworks;
using Newtonsoft.Json;
using UnityEngine;

namespace HGS.RLAgents.Imitation
{
    // Behavior cloning with the genetic algorithm: fitness = -(mean squared error between the network
    // output and the recorded human action), replaying every demonstration in order (recurrent layers
    // start each episode from a clean state). No simulation, so it is fast. The best genome is saved
    // in Resources/Genomes/<model id>, ready to be assigned to Model.seedGenome.
    // Put it alone in a scene and press Play.
    public class ImitationTrainer : MonoBehaviour
    {
        [SerializeField] Model model;

        [Header("Population")]
        [SerializeField] int populationSize = 500;
        [SerializeField] int generations = 300;
        [SerializeField, Range(0f, 1f)] float selectionRate = 0.05f;
        [SerializeField, Range(0f, 1f)] float crossoverRate = 0.25f;
        [SerializeField, Range(0f, 1f)] float mutationRate = 0.05f;
        [SerializeField] float mutationStrength = 0.1f;
        [Tooltip("Standard deviation of the random starting weights")]
        [SerializeField] float initialStrength = 0.3f;

        [Header("Start / Output")]
        [Tooltip("Optional: continue from this genome (one copy untouched, the rest mutated) instead of random weights")]
        [SerializeField] TextAsset resumeFrom;
        [SerializeField] int saveEvery = 25;

        System.Collections.Generic.List<Demonstration> _demonstrations;
        NeuralNetwork[] _networks;
        Individual[] _individuals;
        int _generation;
        float _bestLoss = float.PositiveInfinity;
        string _status = "Loading demonstrations...";

        // The evaluation runs in the background so the editor stays responsive; two cores are left free
        readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        int _threads;
        bool _stopRequested;

        void OnDestroy() => _cancel.Cancel();

        IEnumerator Start()
        {
            _threads = Mathf.Max(1, SystemInfo.processorCount - 2);
            _demonstrations = DemonstrationStorage.LoadAll(model.id);
            if (!ValidateDemonstrations()) yield break;

            int steps = 0;
            foreach (var demonstration in _demonstrations) steps += demonstration.Steps;
            Debug.Log($"[Imitation] {_demonstrations.Count} demonstrations, {steps} steps, {model.GetParametersCount()} parameters, population {populationSize}");

            _networks = new NeuralNetwork[populationSize];
            for (int i = 0; i < populationSize; i++) _networks[i] = NeuralNetworkFactory.CreateFromModel(model);

            _individuals = CreateInitialPopulation();
            Individual best = default;

            for (_generation = 1; _generation <= generations && !_stopRequested; _generation++)
            {
                _status = $"Generation {_generation}/{generations}, evaluating... (best loss so far {_bestLoss:F5})";

                var evaluation = Task.Run(Evaluate);
                while (!evaluation.IsCompleted) yield return null;

                if (evaluation.IsFaulted)
                {
                    Debug.LogException(evaluation.Exception);
                    yield break;
                }
                if (evaluation.IsCanceled) yield break;

                _individuals = GeneticAlgorithm.Evolve(
                    _individuals, selectionRate, crossoverRate, mutationRate, mutationStrength,
                    out _, out var bestFitness, out best);

                _bestLoss = -bestFitness;
                _status = $"Generation {_generation}/{generations} | best loss {_bestLoss:F5}";
                Debug.Log($"[Imitation] {_status}");

                if (saveEvery > 0 && _generation % saveEvery == 0) Save(best);
            }

            if (best.Genome.Genes != null) Save(best);
            _status = $"Done | best loss {_bestLoss:F5}";
        }

        bool ValidateDemonstrations()
        {
            if (_demonstrations.Count == 0)
            {
                _status = $"No demonstrations in {DemonstrationStorage.Folder(model.id)}";
                Debug.LogError($"[Imitation] {_status}");
                return false;
            }

            int outputSize = model.inputSize;
            foreach (var layer in model.layers) outputSize = NeuralNetworkFactory.GetOutputSize(outputSize, layer);

            foreach (var demonstration in _demonstrations)
            {
                if (demonstration.inputSize != model.inputSize || demonstration.outputSize != outputSize)
                {
                    _status = $"Demonstration sizes ({demonstration.inputSize} -> {demonstration.outputSize}) do not match the model ({model.inputSize} -> {outputSize}). Delete the old recordings.";
                    Debug.LogError($"[Imitation] {_status}");
                    return false;
                }
            }
            return true;
        }

        Individual[] CreateInitialPopulation()
        {
            Genome? resume = null;
            if (resumeFrom != null)
            {
                var genome = JsonConvert.DeserializeObject<Genome>(resumeFrom.text);
                if (genome.Genes != null && genome.Genes.Length == model.GetParametersCount()) resume = genome;
                else Debug.LogWarning("[Imitation] resumeFrom does not match the model, starting from random weights.");
            }

            var individuals = new Individual[populationSize];
            for (int i = 0; i < populationSize; i++)
            {
                var genome = new Genome(model.GetParametersCount());
                if (resume.HasValue)
                {
                    System.Array.Copy(resume.Value.Genes, genome.Genes, genome.Genes.Length);
                    if (i > 0)
                    {
                        for (int g = 0; g < genome.Genes.Length; g++)
                            if (Rand.Linear() < mutationRate) genome.Genes[g] += Rand.Gaussian(0f, mutationStrength);
                    }
                }
                else
                {
                    genome.Seed(initialStrength);
                }

                individuals[i] = new Individual { Id = i, Genome = genome, State = EvaluationState.Pending };
            }
            return individuals;
        }

        // Each individual has its own network, so the replays are independent and run in parallel
        void Evaluate()
        {
            var individuals = _individuals;
            var options = new ParallelOptions { MaxDegreeOfParallelism = _threads, CancellationToken = _cancel.Token };
            Parallel.For(0, individuals.Length, options, i =>
            {
                _networks[i].SetParameters(individuals[i].Genome.Genes);
                individuals[i].Fitness = -Loss(_networks[i]);
                individuals[i].State = EvaluationState.Evaluated;
            });
        }

        float Loss(NeuralNetwork network)
        {
            double sum = 0;
            int count = 0;

            foreach (var demonstration in _demonstrations)
            {
                network.ResetState();
                for (int t = 0; t < demonstration.Steps; t++)
                {
                    var output = network.Predict(demonstration.inputs[t]);
                    var target = demonstration.outputs[t];
                    for (int k = 0; k < target.Length; k++)
                    {
                        var difference = output[k] - target[k];
                        sum += difference * difference;
                        count++;
                    }
                }
            }
            return (float)(sum / count);
        }

        void Save(Individual best)
        {
            var folder = Path.Combine("Assets", "Resources", "Genomes", model.id);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var loss = (-best.Fitness).ToString("F5", CultureInfo.InvariantCulture);
            var path = Path.Combine(folder, $"imitation_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}_gen_{_generation}_loss_{loss}.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(best.Genome, Formatting.None));
            Debug.Log($"[Imitation] saved {path}");
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 800, 24), $"[Imitation] {_status}");

            if (!_stopRequested && GUI.Button(new Rect(10, 40, 220, 28), "Stop after this generation and save"))
                _stopRequested = true;
        }
    }
}
