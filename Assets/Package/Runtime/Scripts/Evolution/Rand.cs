using System;

namespace HGS.RLAgents.Evolution
{
    public class Rand
    {
        private static Random _random = new Random();

        public static float Linear()
        {
            return (float)_random.NextDouble();
        }

        public static float Linear(float min, float max)
        {
            return (float)(min + _random.NextDouble() * (max - min));
        }

        public static int Linear(int max)
        {
            return _random.Next(max);
        }

        public static float Gaussian(float mean, float stdDev)
        {
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();

            double randStdNormal =
                Math.Sqrt(-2.0f * Math.Log(u1)) *
                Math.Sin(2.0f * Math.PI * u2);

            return (float)(mean + stdDev * randStdNormal);
        }
    }
}
