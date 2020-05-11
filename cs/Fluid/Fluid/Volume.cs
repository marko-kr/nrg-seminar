using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid
{
    class Volume
    {
        public float [,,] Data { get; }
        public int Size { get { return Data.GetLength(0); } }

        public Volume(int size, float value = 0)
        {
            Data = new float[size, size, size];
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    for (int k = 0; k < Size; k++)
                    {
                        Data[i, j, k] = value;
                    }
                }
            }
        }

        public void AddPerlinNoise(float frequency, float offset = 1, int seed = 1337)
        {
            FastNoise noiseGenerator = new FastNoise(seed);
            noiseGenerator.SetFrequency(frequency);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    for (int k = 0; k < Size; k++)
                    {
                        Data[i, j, k] += offset * noiseGenerator.GetPerlin(i, j, k);
                    }
                }
            }
        }

        private void ResolveEdges()
        {
            int n = Size;
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    Data[i, j, 0] = Data[i, j, 1];
                    Data[i, j, n - 1] = Data[i, j, n - 2];
                }
            }
            for (int k = 1; k < n - 1; k++)
            {
                for (int i = 1; i < n - 1; i++)
                {
                    Data[i, 0, k] = Data[i, 1, k];
                    Data[i, n - 1, k] = Data[i, n - 2, k];
                }
            }
            for (int k = 1; k < n - 1; k++)
            {
                for (int j = 1; j < n - 1; j++)
                {
                    Data[0, j, k] = Data[1, j, k];
                    Data[n - 1, j, k] = Data[n - 2, j, k];
                }
            }
            // Data[0, 0, 0] = Data[1, 0, 0] + Data[0, 1, 0] + Data[0, 0, 1]
        }

        public void Diffuse(float diff, int iterations = 15)
        {
            float[,,] prev = Data.Clone() as float[,,];
            float a = diff * (float)Math.Pow(Size - 2, 3);
            for(int iter = 0; iter < iterations; iter++)
            {
                for (int i = 1; i < Size-1; i++)
                {
                    for (int j = 1; j < Size-1; j++)
                    {
                        for (int k = 1; k < Size-1; k++)
                        {
                            Data[i, j, k] = (prev[i, j, k] + a * (Data[i - 1, j, k] + Data[i + 1, j, k] + Data[i, j - 1, k] + Data[i, j + 1, k] + Data[i, j, k - 1] + Data[i, j, k + 1])) / (1 + 6 * a);
                        }
                    }
                }
                ResolveEdges();
                Console.WriteLine(string.Format("{0}", iter));
            }
        }
 
    }
}
