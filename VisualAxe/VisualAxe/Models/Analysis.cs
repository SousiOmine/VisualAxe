using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualAxe.Models
{
	internal class Analysis
	{
		public static async Task<List<Color>> GetMajorColorAsync(Bitmap bmp, int res)
		{
			List<Color> result_colors = new List<Color>();

			await Task.Run(() =>
			{
				double scale = (double)res / bmp.PixelSize.Width;
				PixelSize pixelSize = new PixelSize((int)(bmp.PixelSize.Width * scale), (int)(bmp.PixelSize.Height * scale));
				bmp = bmp.CreateScaledBitmap(pixelSize);

				var skbitmap = new SKBitmap();

				using (var memoryStream = new MemoryStream())
				{
					bmp.Save(memoryStream);
					memoryStream.Position = 0;

					using (var skiaStream = new SKManagedStream(memoryStream))
					{
						skbitmap = SKBitmap.Decode(skiaStream);
					}
				}

				Dictionary<SKColor, int> colorCount = new Dictionary<SKColor, int>();



				for (int y = 0; y < skbitmap.Height; y++)
				{
					for (int x = 0; x < skbitmap.Width; x++)
					{
						SKColor color = skbitmap.GetPixel(x, y);
						if (!colorCount.ContainsKey(color))
						{
							colorCount[color] = 1;
						}
						else
						{
							colorCount[color]++;
						}
					}
				}

				Dictionary<SKColor, int> distinctColors = new Dictionary<SKColor, int>();

				foreach (var c in colorCount)
				{
					//近い色は削除しカウントを残ったほうに加算する
					SKColor color = c.Key;
					int count = c.Value;

					bool isDistinct = true;
					foreach (var excitingColor in distinctColors.Keys)
					{
						if (Math.Abs(color.Red - excitingColor.Red) < 40 && Math.Abs(color.Green - excitingColor.Green) < 40 && Math.Abs(color.Blue - excitingColor.Blue) < 40)
						{
							distinctColors[excitingColor] += count;
							isDistinct = false;
							break;
						}
					}
					if (isDistinct)
					{
						distinctColors.Add(color, count);
					}
				}

				foreach (var c in distinctColors.OrderBy(c => c.Value).Reverse())
				{
					Color color = new Color(c.Key.Alpha, c.Key.Red, c.Key.Green, c.Key.Blue);
					result_colors.Add(color);
				}
			});

			// 画像の解像度をresに合わせて下げる
			

			return result_colors;
		}
	}
}
