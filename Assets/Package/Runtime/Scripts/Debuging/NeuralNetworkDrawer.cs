using System;
using HGS.RLAgents.NeuralNetworks;
using UnityEngine;

namespace HGS.RLAgents.Debuging
{
    [Serializable]
    public class NeuralNetworkDrawer
    {
        [Header("Neuron")]
        [SerializeField] int neuronRadius = 6;
        [SerializeField] int synapseThickness = 2;
        [SerializeField] int layerSpacing = 90;
        [SerializeField] int neuronSpacing = 40;

        [Header("Colors")]
        [SerializeField] Color backgroundColor = Color.black;

        public Texture2D Texture { get; private set; }

        public void Init(int size = 512)
        {
            Texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Texture.filterMode = FilterMode.Point;
        }

        Color Heatmap(float t)
        {
            if (t <= 0.01f)
                return new Color(0.1f, 0.1f, 0.1f);

            if (t < 0.4f)
                return Color.Lerp(
                    new Color(0.15f, 0.2f, 0.4f),
                    new Color(0.2f, 0.6f, 1f),
                    t / 0.4f
                );

            if (t < 0.8f)
                return Color.Lerp(
                    new Color(0.2f, 0.6f, 1f),
                    new Color(0.3f, 1f, 0.3f),
                    (t - 0.4f) / 0.4f
                );

            return Color.Lerp(
                new Color(0.3f, 1f, 0.3f),
                new Color(1f, 0.3f, 0.3f),
                (t - 0.8f) / 0.2f
            );
        }

        public void Generate(NeuralNetwork net)
        {
            Texture.Reinitialize(Texture.width, Texture.height);
            Clear(backgroundColor);

            int layerCount = net.Activations.Count;

            float totalWidth = (layerCount - 1) * layerSpacing;
            float startX = Texture.width * 0.5f - totalWidth * 0.5f;

            for (int l = 0; l < layerCount; l++)
            {
                var layer = net.Activations[l];

                float totalHeight = (layer.Count - 1) * neuronSpacing;
                float startY = Texture.height * 0.5f - totalHeight * 0.5f;

                for (int n = 0; n < layer.Count; n++)
                {
                    int cx = Mathf.RoundToInt(startX + l * layerSpacing);
                    int cy = Mathf.RoundToInt(startY + n * neuronSpacing);

                    Color color = Heatmap(layer[n]);

                    // Sinapses
                    if (l > 0)
                    {
                        var prevLayer = net.Activations[l - 1];
                        float prevHeight = (prevLayer.Count - 1) * neuronSpacing;
                        float prevStartY = Texture.height * 0.5f - prevHeight * 0.5f;
                        int px = Mathf.RoundToInt(startX + (l - 1) * layerSpacing);

                        for (int p = 0; p < prevLayer.Count; p++)
                        {
                            int py = Mathf.RoundToInt(prevStartY + p * neuronSpacing);
                            DrawSynapse(px, py, cx, cy, color);
                        }
                    }

                    DrawNeuron(cx, cy, color);
                }
            }

            Texture.Apply();
        }

        void Clear(Color color)
        {
            var pixels = Texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            Texture.SetPixels(pixels);
        }

        void DrawNeuron(int cx, int cy, Color color)
        {
            int r2 = neuronRadius * neuronRadius;

            for (int x = -neuronRadius; x <= neuronRadius; x++)
                for (int y = -neuronRadius; y <= neuronRadius; y++)
                {
                    if (x * x + y * y > r2) continue;

                    int px = cx + x;
                    int py = cy + y;
                    if (px < 0 || py < 0 || px >= Texture.width || py >= Texture.height)
                        continue;

                    Texture.SetPixel(px, py, color);
                }
        }

        void DrawSynapse(int ax, int ay, int bx, int by, Color color)
        {
            Vector2 dir = new Vector2(bx - ax, by - ay);
            float length = dir.magnitude;
            dir.Normalize();
            Vector2 normal = new Vector2(-dir.y, dir.x);

            for (float l = 0; l <= length; l += 1f)
            {
                Vector2 p = new Vector2(ax, ay) + dir * l;
                for (int t = -synapseThickness; t <= synapseThickness; t++)
                {
                    Vector2 pt = p + normal * t;
                    int x = Mathf.RoundToInt(pt.x);
                    int y = Mathf.RoundToInt(pt.y);

                    if (x < 0 || y < 0 || x >= Texture.width || y >= Texture.height)
                        continue;

                    Texture.SetPixel(x, y, color);
                }
            }
        }
    }
}