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
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        Data[i, j, k] = value;
                    }
                }
            }
            BaseData = (float[,,]) Data.Clone(); 
        }

        // Offsets the volume values with perlin noise
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

        //Resolves the edges needed for the diffusion step
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

        // Diffuses the volume using an iterative solver based on Gauss-Seidel relaxation
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

        // Adds the floor to the volume
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

        // Applies a surface object to the volume to form a surface
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

        
        // Exports the volume in raw data, can export the gradient as well
        public void Export(string path, bool includeGradient = true)
        {
            // get the max mand min values used for mapping values to fit in a single byte
            float max = Max;
            float min = Min;
            int reduced = Size - 2;
            int outputSize = reduced * reduced * reduced;

            Vector3[,,] gradient = null;
            float gradientDominant = 0;

            // calculate the gradient and get the maximum absolute value for mapping purposes
            if(includeGradient) 
            { 
                outputSize *= 4;
                gradient = Utils.SobelGradient(Data);
                float gradientMin = gradient[0, 0, 0].X;
                float gradientMax = gradient[0, 0, 0].X;
                for (int k = 0; k < gradient.GetLength(2);k++)
                {
                    for (int j = 0; j < gradient.GetLength(1); j++)
                    {
                        for (int i = 0; i < gradient.GetLength(0); i++)
                        {
                            Vector3 direction = gradient[i, j, k];
                            if(direction.X < gradientMin) { gradientMin = direction.X; }
                            if (direction.X > gradientMax) { gradientMax = direction.X; }
                            if (direction.Y < gradientMin) { gradientMin = direction.Y; }
                            if (direction.Y > gradientMax) { gradientMax = direction.Y; }
                            if (direction.Z < gradientMin) { gradientMin = direction.Z; }
                            if (direction.Z > gradientMax) { gradientMax = direction.Z; }
                        }
                    }
                }
                gradientDominant = Math.Max(Math.Abs(gradientMin), Math.Abs(gradientMax));
             }
            
            // sequentially map the volume and gradient data to fit into a singel byte
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
                            output[outputIndex + 1] = (byte)(int)Math.Round(Utils.Map(gradient[i - 1, j - 1, k - 1].X, -gradientDominant, gradientDominant, 0, 255));
                            output[outputIndex + 2] = (byte)(int)Math.Round(Utils.Map(gradient[i - 1, j - 1, k - 1].Y, -gradientDominant, gradientDominant, 0, 255));
                            output[outputIndex + 3] = (byte)(int)Math.Round(Utils.Map(gradient[i - 1, j - 1, k - 1].Z, -gradientDominant, gradientDominant, 0, 255));
                        }
                        output[outputIndex] = (byte)(int)Math.Round(Utils.Map(Data[i, j, k], min, max, 0, 255));
                    }
                }
            }
            // weite byte array to file
            File.WriteAllBytes(path, output);
        }
    }
}
