using UnityEngine;

namespace HGS.RLAgents.Debuging
{
    public class AgentDebugger : MonoBehaviour
    {
        [SerializeField] Academy academy;
        [SerializeField] float updateInterval = 0.5f;
        [SerializeField] Agent agent;
        [SerializeField] NeuralNetworkDrawer neuralNetworkDrawer;

        private float _timer = 0;

        void Awake()
        {
            neuralNetworkDrawer.Init();
        }

        private void FixedUpdate()
        {
            if (_timer > updateInterval)
            {
                _timer = 0;
                Generate();
            }

            _timer += Time.fixedDeltaTime;
        }

        void Generate()
        {
            neuralNetworkDrawer.Generate(agent.NeuralNetwork);
        }

        void OnGUI()
        {
            GUI.DrawTexture(new Rect(10, 10, neuralNetworkDrawer.Texture.width, neuralNetworkDrawer.Texture.height), neuralNetworkDrawer.Texture);
        }
    }
}
