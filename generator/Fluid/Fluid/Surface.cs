using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Fluid
{
    class Surface
    {
        public float[,] Data { get; }
        public int Size { get { return Data.GetLength(0); } }
        public float Max { get { return Data.Cast<float>().Max(); } }
        public float Min { get { return Data.Cast<float>().Min(); } }

        public Surface(int size)
        {
            Data = new float[size, size];
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Data[i, j] = 0;
                }
            }
        }

        // evaluate a wave at x and y
        private float EvaluateWave(float x, float y, float frequency, float amplitude, Vector2 direction)
        {
            return amplitude * (float)Math.Sin(frequency * (direction.X * x + direction.Y * y));
        }

        // apply a wave to the existing surface
        public void AddWave(float frequency, float amplitude, Vector2 direction)
        {
            direction = Vector2.Normalize(direction);
            for(int i = 0; i < Size; i++)
            {
                for(int j = 0; j < Size; j++)
                {
                    Data[i, j] += EvaluateWave(i, j, frequency, amplitude, direction);
                }
            }
        }

    }
}
