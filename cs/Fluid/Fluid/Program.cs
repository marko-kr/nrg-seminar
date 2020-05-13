using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Fluid
{

	class Program
	{
		static int[,] CreateSlice(float[,,] data)
		{
			int sizeX = data.GetLength(0);
			int sizeY = data.GetLength(1);
			int sizeZ = data.GetLength(2);
			int[,] slice = new int[sizeX, sizeY];
			int depth = (int)sizeZ / 2;
			float min = data.Cast<float>().Min();
			float max = data.Cast<float>().Max();
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					slice[i, j] = (int)Math.Round(Utils.Map(data[i, j, depth], min, max, 0, 255));
				}
			}
			return slice;
		}

		static int[,] CreateSlice(Surface surface)
		{
			int[,] slice = new int[surface.Size, surface.Size];
			float min = surface.Min;
			float max = surface.Max;
			for (int i = 0; i < surface.Size; i++)
			{
				for (int j = 0; j < surface.Size; j++)
				{
					slice[i, j] = (int)Math.Round(Utils.Map(surface.Data[i, j], min, max, 0, 255));
				}
			}
			return slice;
		}


		static void Main(string[] args)
		{
			int volumeSize = 130;
			Volume volume = new Volume(volumeSize, 100);
			Console.WriteLine("Generating noise...");
			volume.AddPerlinNoise(0.03f, 20);
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(volume.Data)), "C:/Users/Mareee/Desktop/wd/noise.png");
			for(int i = 0; i < 3; i++)
			{
				Console.WriteLine("Diffussing...");
				volume.Diffuse(1);
			}
			volume.AddFloor(5, 255);
			Surface surface = new Surface(volumeSize);
			surface.AddWave(0.05f, 1.5f, new Vector2(0.1f, 1));
			surface.AddWave(0.15f, 2, new Vector2(-0.1f, 1));
			surface.AddWave(0.08f, 1.2f, new Vector2(1, 1));
			volume.ApplySurface(surface, 0);
			//Vector3[,,] gradient = Utils.SobelGradient(volume.Data);
			//float[,,] gradMagnitude = Utils.GradientMagnitude(gradient);
			//Utils.SaveImage(Utils.IntToBitmap(CreateSlice(gradMagnitude)), "C:/Users/Mareee/Desktop/wd/gradient.png");
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(surface)), "C:/Users/Mareee/Desktop/wd/surface.png");
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(volume.Data)), "C:/Users/Mareee/Desktop/wd/diffused.png");
			Console.WriteLine("Exporting...");
			volume.Export("C:/Users/Mareee/Desktop/wd/output", true);
			Console.Write("Done...");
			Console.Read();
		}
	}
}
