using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace HGS.RLAgents.Imitation
{
    // One episode driven by a human: what the agent observed and the action that was applied, one entry
    // per decision. Order matters (recurrent layers are replayed from the first step).
    [Serializable]
    public class Demonstration
    {
        public string modelId;
        public int inputSize;
        public int outputSize;
        public float evaluateInterval;
        public List<float[]> inputs = new List<float[]>();
        public List<float[]> outputs = new List<float[]>();

        public int Steps => inputs.Count;
    }

    // Editor-time storage, one JSON per episode so a bad one can be deleted by hand
    public static class DemonstrationStorage
    {
        public static string Folder(string modelId) => Path.Combine("Assets", "Resources", "Demonstrations", modelId);

        public static string Save(Demonstration demonstration)
        {
            var folder = Folder(demonstration.modelId);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}_steps_{demonstration.Steps}.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(demonstration, Formatting.None));
            return path;
        }

        public static List<Demonstration> LoadAll(string modelId)
        {
            var demonstrations = new List<Demonstration>();
            var folder = Folder(modelId);
            if (!Directory.Exists(folder)) return demonstrations;

            foreach (var path in Directory.GetFiles(folder, "*.json"))
            {
                var demonstration = JsonConvert.DeserializeObject<Demonstration>(File.ReadAllText(path));
                if (demonstration != null && demonstration.Steps > 0) demonstrations.Add(demonstration);
            }
            return demonstrations;
        }
    }
}
