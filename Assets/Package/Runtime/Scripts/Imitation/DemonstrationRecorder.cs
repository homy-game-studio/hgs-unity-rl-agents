using UnityEngine;

namespace HGS.RLAgents.Imitation
{
    // Records the agent's observations and the action applied at every decision while a human
    // drives it (agent in heuristic mode). Needs the agent to be active: it is enabled here.
    public class DemonstrationRecorder : MonoBehaviour
    {
        [SerializeField] Agent agent;

        [Header("Keys")]
        [Tooltip("Saves the episode and respawns the agent")]
        [SerializeField] KeyCode saveKey = KeyCode.Return;
        [Tooltip("Throws the episode away and respawns the agent (use it instead of the agent's own respawn key)")]
        [SerializeField] KeyCode discardKey = KeyCode.Backspace;

        [Tooltip("Episodes shorter than this many decisions are not saved")]
        [SerializeField] int minSteps = 20;

        Demonstration _current;
        int _saved;
        bool _warnedNoAction;
        string _message = "";

        void OnEnable()
        {
            agent.onEvaluationEnd += Capture;
            agent.active = true;
            StartEpisode();
        }

        void OnDisable()
        {
            if (agent != null) agent.onEvaluationEnd -= Capture;
        }

        void StartEpisode()
        {
            _current = new Demonstration
            {
                modelId = agent.model.id,
                inputSize = agent.model.inputSize,
                evaluateInterval = agent.evaluateInterval,
            };
        }

        void Capture()
        {
            var action = agent.GetAppliedAction();
            if (action == null)
            {
                if (!_warnedNoAction)
                {
                    _warnedNoAction = true;
                    Debug.LogError($"{agent.GetType().Name} does not implement GetAppliedAction(), nothing can be recorded.");
                }
                return;
            }

            _current.outputSize = action.Length;
            _current.inputs.Add((float[])agent.LastInput.Clone());
            _current.outputs.Add((float[])action.Clone());
        }

        void Update()
        {
            if (Input.GetKeyDown(saveKey)) Save();
            else if (Input.GetKeyDown(discardKey)) Discard();
        }

        void Save()
        {
            if (_current.Steps < minSteps)
            {
                _message = $"Too short ({_current.Steps} < {minSteps}), discarded";
                Next();
                return;
            }

            var path = DemonstrationStorage.Save(_current);
            _saved++;
            _message = $"Saved {_current.Steps} steps";
            Debug.Log($"[Demonstration] {path}");
            Next();
        }

        void Discard()
        {
            _message = "Discarded";
            Next();
        }

        void Next()
        {
            StartEpisode();
            agent.RespawnForDemonstration();
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 600, 24), $"Recording | steps {_current.Steps} | saved {_saved} | [{saveKey}] save  [{discardKey}] discard  {_message}");
        }
    }
}
