namespace HGS.RLAgents.NeuralNetworks
{
    // One stage of the network. A layer owns its parameter layout: the network only slices
    // the genome by ParameterCount, so a new layer type never touches the network or the genome code.
    public interface ILayer
    {
        int InputSize { get; }
        int OutputSize { get; }
        int ParameterCount { get; }

        // Writes/reads exactly ParameterCount values starting at offset
        void GetParameters(float[] destination, int offset);
        void SetParameters(float[] source, int offset);

        // The returned array is owned by the layer and overwritten by the next call
        float[] Forward(float[] input);

        // Forgets the previous steps (call at the start of every episode)
        void ResetState();
    }

    internal static class LayerMath
    {
        // Dot product of vector with the matrix row that starts at offset (row-major)
        public static float Dot(float[] matrix, int offset, float[] vector)
        {
            var sum = 0f;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += matrix[offset + i] * vector[i];
            }
            return sum;
        }

        public static void Write(float[] values, float[] destination, ref int offset)
        {
            System.Array.Copy(values, 0, destination, offset, values.Length);
            offset += values.Length;
        }

        public static void Read(float[] source, ref int offset, float[] values)
        {
            System.Array.Copy(source, offset, values, 0, values.Length);
            offset += values.Length;
        }
    }
}
