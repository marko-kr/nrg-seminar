using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace Fluid
{
    class Volume
    {
        public float [,,] Data { get; }
        public int Size { get { return Data.GetLength(0); } }
        public float[,,] BaseData { get; }
        public float BaseValue { get; }
        public float Max { get { return Data.Cast<float>().Max(); } }
        public float Min { get { return Data.Cast<float>().Min(); } }

        public Volume(int size, float value = 0)
        {
            Data = new float[size, size, size];
            BaseValue = value;
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
            BaseData = (float[,,]) Data.Clone(); 
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
            float a = diff * (float)Math.Pow(Size - 2, 3);
            for(int iter = 0; iter < iterations; iter++)
            {
                for (int i = 1; i < Size-1; i++)
                {
                    for (int j = 1; j < Size-1; j++)
                    {
                        for (int k = 1; k < Size-1; k++)
                        {
                            float v = a * (Data[i - 1, j, k] + Data[i + 1, j, k] + Data[i, j - 1, k] + Data[i, j + 1, k] + Data[i, j, k - 1] + Data[i, j, k + 1]);
                            Data[i, j, k] = (BaseData[i, j, k] + v) / (1 + 6 * a);
                        }
                    }
                }
                ResolveEdges();
            }
        }

        public void AddFloor(int thickness, float value)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int k = 0; k < Size; k++)
                {
                    for (int j = 0; j < thickness; j++)
                    {
                        Data[i, j, k] = value;
                    }
                }
            }
        }

        public void ApplySurface(Surface surface, float airValue)
        {
            int max = (int)Math.Ceiling(surface.Max);
            int min = (int)Math.Floor(surface.Min);
            int baseHeight = Size - 1 - max;
            if (baseHeight-min < 0)
            {
                throw new Exception("Surface amplitude exceeds volume");
            }
            for (int i = 0; i < Size; i++)
            {
                for (int k = 0; k < Size; k++)
                {
                    int surfaceHeight = (int)Math.Round(baseHeight + surface.Data[i, k]);
                    for (int j = surfaceHeight; j < Size; j++)
                    {
                        Data[i, j, k] = airValue;
                    }
                }
            }
        }

        

        public void Export(string path, bool includeGradient = false)
        {
            float max = Max;
            float min = Min;
            int reduced = Size - 2;
            int outputSize = reduced * reduced * reduced;

            Vector3[,,] gradient = null; 
            if(includeGradient) 
            { 
                outputSize *= 4;
                gradient = Utils.SobelGradient(Data);
            }
            
            byte[] output = new byte[outputSize];
            for (int k = 1; k < Size - 1; k++)
            {
                for (int j = 1; j < Size - 1; j++)
                {
                    for (int i = 1; i < Size - 1; i++)
                    {
                        int outputIndex = (i - 1) + reduced * ((j - 1) + reduced * (k - 1));
                        if (includeGradient) 
                        { 
                            outputIndex *= 4;
                            output[outputIndex + 1] = (byte)(int)Math.Round(Utils.Map(gradient[i - 1, j - 1, k - 1].X, min, max, 0, 255));
                            output[outputIndex + 2] = (byte)(int)Math.Round(Utils.Map(gradient[i - 1, j - 1, k - 1].Y, min, max, 0, 255));
                            output[outputIndex + 3] = (byte)(int)Math.Round(Utils.Map(gradient[i - 1, j - 1, k - 1].Z, min, max, 0, 255));
                        }
                        output[outputIndex] = (byte)(int)Math.Round(Utils.Map(Data[i, j, k], min, max, 0, 255));
                    }
                }
            }
            File.WriteAllBytes(path, output);
        }
    }
}
