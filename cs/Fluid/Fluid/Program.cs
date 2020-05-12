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
		static int[,] CreateSlice(Volume volume)
		{
			int[,] slice = new int[volume.Size, volume.Size];
			int depth = (int)volume.Size / 2;
			float min = volume.Min;
			float max = volume.Max;
			for (int i = 0; i < volume.Size; i++)
			{
				for (int j = 0; j < volume.Size; j++)
				{
					slice[i, j] = (int)Math.Round(Utils.Map(volume.Data[i, j, depth], min, max, 0, 255));
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
			int volumeSize = 128;
			Volume volume = new Volume(volumeSize, 100);
			float v = volume.Data.Cast<float>().Max();
			Console.WriteLine(string.Format("{0}", v));
			volume.AddPerlinNoise(0.03f, 20);
			v = volume.Max;
			Console.WriteLine(string.Format("{0}", v));
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(volume)), "C:/Users/Mareee/Desktop/wd/noise.png");
			for(int i = 0; i < 0; i++)
			{
				volume.Diffuse(1);
			}
			volume.AddFloor(5, 255);
			Surface surface = new Surface(volumeSize);
			surface.AddWave(0.05f, 1.5f, new Vector2(0.1f, 1));
			surface.AddWave(0.15f, 2, new Vector2(-0.1f, 1));
			surface.AddWave(0.08f, 1.2f, new Vector2(1, 1));
			volume.ApplySurface(surface, 0);
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(surface)), "C:/Users/Mareee/Desktop/wd/surface.png");
			Utils.SaveImage(Utils.IntToBitmap(CreateSlice(volume)), "C:/Users/Mareee/Desktop/wd/diffused.png");
			v = volume.Data.Cast<float>().Max();
			Console.WriteLine(string.Format("{0}", v));
			volume.Export("C:/Users/Mareee/Desktop/wd/output");
			Console.ReadLine();
		}
	}
}
