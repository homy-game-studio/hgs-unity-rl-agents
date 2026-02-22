using UnityEngine;

namespace HGS.RLAgents.Debuging
{
    public class AgentDebugger : MonoBehaviour
    {
        [SerializeField] Academy academy;
        [SerializeField] float updateInterval = 0.5f;
        [SerializeField] Agent agent;
        [SerializeField] NeuralNetworkDrawer neuralNetworkDrawer;
        [SerializeField] bool showInputs;
        [SerializeField] bool showOutputs;
        [SerializeField] Color fontColor;
        [SerializeField] float fontSize;

        private float _timer = 0;

        Texture circleTexture;

        void Awake()
        {
            neuralNetworkDrawer.Init();
            circleTexture = GenerateCircleTexture();
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

        Texture GenerateCircleTexture()
        {
            var texture = new Texture2D(60, 60, TextureFormat.RGBA32, false);
            var trickness = 2;
            texture.filterMode = FilterMode.Point;

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    float dx = x - texture.width / 2;
                    float dy = y - texture.height / 2;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance < texture.width / 2 && distance > texture.width / 2 - trickness)
                        texture.SetPixel(x, y, Color.yellow);
                    else
                        texture.SetPixel(x, y, Color.clear);
                }
            }

            texture.Apply();
            return texture;
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

        void OnGUI()
        {
            GUI.BeginGroup(new Rect(10, 10, 520, Screen.height));
            GUILayout.BeginVertical();

            Rect rect = GUILayoutUtility.GetRect(
                neuralNetworkDrawer.Texture.width,
                neuralNetworkDrawer.Texture.height,
                GUILayout.Width(neuralNetworkDrawer.Texture.width),
                GUILayout.Height(neuralNetworkDrawer.Texture.height),
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false)
            );

            GUI.DrawTexture(rect, neuralNetworkDrawer.Texture, ScaleMode.ScaleToFit, false);
            DrawTitle("ACADEMY");
            DrawField("Generation", academy.Generations);
            // Amostra dos ultimos 10 rewards
            for (int i = 0; i < 10; i++)
            {
                if (academy.AvgRewards.ContainsKey(agent.model.populationId) && academy.AvgRewards[agent.model.populationId].Count > i)
                {
                    DrawField($"Reward {academy.Generations - i}", academy.AvgRewards[agent.model.populationId][academy.AvgRewards[agent.model.populationId].Count - 1 - i]);
                }
            }
            DrawTitle("BETTER AGENT");
            DrawField("Active", agent.active);
            DrawField("Evaluations", agent.evaluationCount);
            if(showInputs)
            {
                DrawTitle("INPUTS");
                for (int i = 0; i < agent.LastInput.Length; i++)
                {
                    DrawField($"Input {i}", agent.LastInput[i]);
                }
            }
            if(showOutputs) {
                DrawTitle("OUTPUTS");
                for (int i = 0; i < agent.LastOutput.Length; i++)
                {
                    DrawField($"Output {i}", agent.LastOutput[i]);
                }
            }

            GUILayout.EndVertical();
            GUI.EndGroup();

            if (agent == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(agent.transform.position);

            if (screenPos.z > 0)
            {
                Rect agentRect = new Rect(
                    screenPos.x - circleTexture.width / 2,
                    Screen.height - screenPos.y - circleTexture.height / 2,
                    circleTexture.width,
                    circleTexture.height
                );

                GUI.color = Color.yellow;
                GUI.DrawTexture(agentRect, circleTexture);
            }
        }
    }
}