using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace Fluid
{
	public static class Utils
	{
		// function for mapping float values
		public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
		{
			return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
		}

		public static int Clamp(int x, int min, int max)
		{
			if (x < min) x = min;
			if (x > max) x = max;
			return x;
		}

		// Convert 2d int array to a bitmap
		public static Bitmap IntToBitmap(int[,] data)
		{
			int width = data.GetLength(0);
			int height = data.GetLength(1);
			int stride = width * 4;

			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					byte value = (byte)Clamp(data[x, y], 0, 255);
					byte[] bgra = new byte[] { value, value, value, 255 };
					data[x, y] = BitConverter.ToInt32(bgra, 0);
				}
			}

			// Copy into bitmap
			Bitmap bitmap;
			unsafe
			{
				fixed (int* intPtr = &data[0, 0])
				{
					bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppRgb, new IntPtr(intPtr));
				}
			}
			return bitmap;
		}

		// Save bitmap to png file
		public static void SaveImage(Bitmap image, string path)
		{
			image.Save(path, ImageFormat.Png);
		}

		// Sample sobel gradient at a 3d position
        public static Vector3 SampleSobelGradient(float[, ,] data, int x, int y, int z)
        {
            float[,,] kernelX = new float[3, 3, 3] { { {1, 2, 1}, {2, 4, 2}, {1, 2, 1}},
                                                        { {0, 0, 0}, {0, 0, 0}, {0, 0, 0}},
                                                        { {-1, -2, -1}, {-2, -4, -2}, {-1, -2, -1}}};

            float[,,] kernelY = new float[3, 3, 3] { { {1, 2, 1}, {0, 0, 0}, {-1, -2, -1}},
                                                        { {2, 4, 2}, {0, 0, 0}, {-2, -4, -2}},
                                                        { {1, 2, 1}, {0, 0, 0}, {-1, -2, -1}}};

            float[,,] kernelZ = new float[3, 3, 3] { { {1, 0, -1}, {2, 0, -2}, {1, 0, -1}},
                                                        { {2, 0, -2},{4, 0, -4},{2, 0, -2}},
                                                        { {1, 0, -1},{2, 0, -2},{1, 0, -1}}};

            float dx = 0;
            float dy = 0;
            float dz = 0;
            for (int k = 0; k < 3; k++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float value = data[x + i - 1, y + j - 1, z + k - 1];
                        dx += value * kernelX[i, j, k];
                        dy += value * kernelY[i, j, k];
                        dz += value * kernelZ[i, j, k];
                    }
                }
            }
            return new Vector3(dx/9, dy/9, dz/9);
        }

		// Calculate sobel for a certain volume
		public static Vector3[,,] SobelGradient(float[, ,] data)
		{
			int sizeX = data.GetLength(0);
			int sizeY = data.GetLength(1);
			int sizeZ = data.GetLength(2);

			Vector3[,,] gradient = new Vector3[sizeX, sizeY, sizeZ];

			// The outut gradient is smaller by 2 in each dimension (due to sobel kernels requiring neighbors)
			for(int k = 1; k < sizeZ - 1; k++)
			{
				for (int j = 1; j < sizeY - 1; j++)
				{
					for (int i = 1; i < sizeX - 1; i++)
					{
						gradient[i, j, k] = SampleSobelGradient(data, i, j, k);

						//Console.WriteLine(String.Format("{0}, {1}, {2}", gradient[i, j, k].X, gradient[i, j, k].Y, gradient[i, j, k].Z));
					}
				}
			}
			return gradient;
		}

		// Calculate magnitude of gradient volume
		public static float[,,] GradientMagnitude(Vector3[, ,] data)
		{
			int sizeX = data.GetLength(0);
			int sizeY = data.GetLength(1);
			int sizeZ = data.GetLength(2);
			float[,,] magnitude = new float[sizeX, sizeY, sizeZ];

			for (int k = 1; k < sizeZ - 1; k++)
			{
				for (int j = 1; j < sizeY - 1; j++)
				{
					for (int i = 1; i < sizeX - 1; i++)
					{
						Vector3 gradValue = data[i, j, k];
						magnitude[i, j, k] = gradValue.Length();
					}
				}
			}
			return magnitude;
		}
	}
}
