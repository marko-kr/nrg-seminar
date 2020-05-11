using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid
{

	class Program
	{

		static void SaveTest(Volume volume, string path)
		{
			int[,] slice = new int[volume.Size, volume.Size];
			for (int i = 0; i < volume.Size; i++)
			{
				for (int j = 0; j < volume.Size; j++)
				{
					slice[i, j] = (int)Math.Round(Utils.Map(volume.Data[i, j, 25], 80, 120, 0, 255));
				}
			}
			Bitmap image = Utils.IntToBitmap(slice);
			image.Save(path, ImageFormat.Png);
		}

		static void Main(string[] args)
		{
			Volume volume = new Volume(128, 100);
			float v = volume.Data.Cast<float>().Max();
			Console.WriteLine(string.Format("Hello {0}", v));
			volume.AddPerlinNoise(0.03f, 20);
			v = volume.Data.Cast<float>().Max();
			Console.WriteLine(string.Format("Hello {0}", v));
			SaveTest(volume, "C:/Users/Mareee/Desktop/wd/noise.png");
			for(int i = 0; i < 20; i++)
			{
				volume.Diffuse(5);
			}
			SaveTest(volume, "C:/Users/Mareee/Desktop/wd/defused.png");
			v = volume.Data.Cast<float>().Max();
			Console.WriteLine(string.Format("Hello {0}", v));

			Console.ReadLine();
		}
	}
}
