using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Fluid
{
	public static class Utils
	{
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

		public static void SaveImage(Bitmap image, string path)
		{
			image.Save(path, ImageFormat.Png);
		}
	}
}
