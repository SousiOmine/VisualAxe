using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VisualAxe.Models
{
	internal class ItemAnalysis
	{
		private static readonly string ItemsStorageName = "ItemsStorage";   //アイテムのファイルを格納しておくフォルダの名前

		public static async Task<Item> InstantAnalysisAsync(Item item)	//データベースに追加する直前の解析やファイル操作を行う
		{

			if (!Directory.Exists(Path.Combine(".", ItemsStorageName)))    //格納用フォルダが実行パス/ItemsStorageNameになければ作成
			{
				await Task.Run(() =>
				{
					Directory.CreateDirectory(Path.Combine(".", ItemsStorageName));
				});
			}

			string rand_folder = Guid.NewGuid().ToString().Substring(0, 32);    //GUIDを使ってランダムな16文字を用意
			if (!Directory.Exists(Path.Combine(".", ItemsStorageName, rand_folder)))    //格納用フォルダ/ItemsStorageName/rand_folderになければ作成
			{
				await Task.Run(() =>
				{
					Directory.CreateDirectory(Path.Combine(".", ItemsStorageName, rand_folder));
				});
			}

			//ファイルが存在していればItemsStorageフォルダにコピーを行う
			if (File.Exists(item.FilePath))
			{
				//ファイルをコピー
				string copied_path = Path.Combine(".", ItemsStorageName, rand_folder, Path.GetFileName(item.FilePath));
				await Task.Run(() =>
				{
					File.Copy(item.FilePath, copied_path);
				});
				item.FilePath = copied_path;

			}
			else if (!String.IsNullOrEmpty(item.Url))	//もしURLが定義されていれば
			{
				//もしURLが画像っぽければ画像ダウンロードを試行
				if (item.Url.Contains("png", StringComparison.OrdinalIgnoreCase) || item.Url.Contains("jpg", StringComparison.OrdinalIgnoreCase) || item.Url.Contains("jpeg", StringComparison.OrdinalIgnoreCase))
				{
					string download_path = Path.Combine(".", ItemsStorageName, rand_folder, "image.png");
					await Task.Run(async () =>
					{
						var client = new HttpClient();
						var responce = await client.GetAsync(item.Url);
						if (responce.IsSuccessStatusCode)	//ダウンロードに成功したら保存してFilePathに登録する
						{
							using var stream = await responce.Content.ReadAsStreamAsync();
							using var outStream = File.Create(download_path);
							stream.CopyTo(outStream);
							item.FilePath = download_path;
						}
					});
				}
			}

			return item;
		}

		public static async Task<Item> DeepAnalysisAsync(Item item)
		{
			Bitmap? mybmp = await Item.GetPreviewAsync(item, 200, false);
			if (mybmp is not null)
			{
				List<Color> colors = await GetMajorColorAsync(mybmp, 100);
				item.Colors = colors;
				mybmp.Dispose();
			}
			return item;
		}

		private static async Task<List<Color>> GetMajorColorAsync(Bitmap bmp, int res)
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

		public static int ColorDistance(Color color1, Color color2)
		{
			int aDiff = Math.Abs(color1.A - color2.A);
			int rDiff = Math.Abs(color1.R - color2.R);
			int gDiff = Math.Abs(color1.G - color2.G);
			int bDiff = Math.Abs(color1.B - color2.B);

			return aDiff + rDiff + gDiff + bDiff;
		}
	}
}
