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
		// Creates a slice from the middle of the volume and maps all values in a single byte range
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
					slice[j, i] = (int)Math.Round(Utils.Map(data[i, j, depth], min, max, 0, 255));
				}
			}
			return slice;
		}

		// Creates a slice from a surface object and maps all values in a single byte range
		static int[,] CreateSlice(Surface surface)
		{
			int[,] slice = new int[surface.Size, surface.Size];
			float min = surface.Min;
			float max = surface.Max;
			for (int i = 0; i < surface.Size; i++)
			{
				for (int j = 0; j < surface.Size; j++)
				{
					slice[j, i] = (int)Math.Round(Utils.Map(surface.Data[i, j], min, max, 0, 255));
				}
			}
			return slice;
		}


		static void Main(string[] args)
		{
			// Parameters, could be extened to be read from command line, param file or GUI
			int volumeSize = 128;
			int noiseSeed = 42;
			float waterDensity = 100;
			float airDensity = 0;
			int floorThinckness = 5;
			float floorDesnity = 200;
			float noiseAmplitude = 25;
			float noiseFrequency = 0.03f;
			int diffusionSteps = 3;

			// Pad the volume size by two
			// This is done to avoid edge value problems with sobel
			int paddedSize = volumeSize + 2;
			Volume volume = new Volume(paddedSize, waterDensity);
			Console.WriteLine("Generating noise...");
			volume.AddPerlinNoise(noiseFrequency, noiseAmplitude, noiseSeed);
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(volume.Data)), "noise.png");
			for(int i = 0; i < diffusionSteps; i++)
			{
				Console.WriteLine("Diffussing...");
				volume.Diffuse(1);
			}
			volume.AddFloor(floorThinckness, floorDesnity);
			Surface surface = new Surface(paddedSize);

			surface.AddWave(0.05f, 1.5f, new Vector2(0.1f, 1));
			surface.AddWave(0.15f, 2, new Vector2(-0.1f, 1));
			surface.AddWave(0.08f, 1.2f, new Vector2(1, 1));

			volume.ApplySurface(surface, airDensity);
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(surface)), "surface.png");
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(volume.Data)), "slice.png");
			Console.WriteLine("Exporting...");
			volume.Export("output", true);
			Console.Write("Done...");
			Console.Read();
		}
	}
}
