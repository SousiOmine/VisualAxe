using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VisualAxe.Models
{
	public class Item
	{
		public int Id { get; set; }
		public DateTime AddedDate { get; set; }
		public string? Title { get; set; }
		public string? FilePath { get; set; }
		public string? Memo { get; set; }
		public string? Url { get; set; }
		public List<Color>? Colors { get; set; }
		public string? Index { get; set; }

		private static LiteDatabase db_context = new("./data.db");

		public static async Task<List<Item>> GetAllItemsAsync()
		{
			var items = new List<Item>();
			items = db_context.GetCollection<Item>("items").FindAll().ToList();
			items.Sort((x, y) => y.AddedDate.CompareTo(x.AddedDate));	//追加日時順にソート
			return items;
		}

		public static async Task<Item?> GetItemAsync(int id)
		{	
			var items = db_context.GetCollection<Item>("items");
			
			Item item = items.FindById(id);
			return item;
		}

		public static Task<List<Item>> SearchAsync(SearchInfo info)
		{
			var all_items = new List<Item>();
			all_items = db_context.GetCollection<Item>("items").FindAll().ToList();
			var word_results = new List<Item>();	//文字による検索結果が格納されるリスト

			if(!String.IsNullOrEmpty(info.word))	//もし検索文字列が空でなければ文字で絞り込み
			{
				foreach (var item in all_items)
				{
					if (!String.IsNullOrEmpty(item.Title))
					{
						if (item.Title.Contains(info.word, StringComparison.OrdinalIgnoreCase))
						{
							word_results.Add(item);
							continue;
						}
					}
					if (!String.IsNullOrEmpty(item.Memo))
					{
						if (item.Memo.Contains(info.word, StringComparison.OrdinalIgnoreCase))
						{
							word_results.Add(item);
							continue;
						}
					}
					if (!String.IsNullOrEmpty(item.Url))
					{
						if (item.Url.Contains(info.word, StringComparison.OrdinalIgnoreCase))
						{
							word_results.Add(item);
							continue;
						}
					}
					if (!String.IsNullOrEmpty(item.Index))
					{
						if (item.Index.Contains(info.word, StringComparison.OrdinalIgnoreCase))
						{
							word_results.Add(item);
							continue;
						}
					}
				}
			}
			else        //もし検索文字列が空ならぜんぶ入れる
			{
				foreach (var item in all_items)
				{
					word_results.Add(item);
				}
			}

			//文字で絞り込んだ後、色でさらに絞る
			var color_results = new List<Item>();
			if(info.color is not null)
			{
				//ここに色での絞り込みを書く
				foreach (var item in word_results)
				{
					if(item.Colors is null) continue;
					for (int i = 0; i < Math.Min(item.Colors.Count, 5); i++)
					{
						if (ItemAnalysis.ColorDistance((Color)item.Colors[i], (Color)info.color) <= 100)
						{
							color_results.Add(item);
							break;
						}
					}
				}
			}
			else
			{
				foreach (var item in word_results)
				{
					color_results.Add(item);
				}
			}

			var results = color_results;
			results.Sort((x, y) => y.AddedDate.CompareTo(x.AddedDate)); //追加日時順にソート

			return Task.FromResult(results);
		}

		public static async Task<Bitmap?> GetPreviewAsync(Item item, int Width, bool getIcon)
		{
			Bitmap? bitmap = null;
			if (File.Exists(item.FilePath))
			{
				switch (Path.GetExtension(item.FilePath))
				{
					case ".png":
					case ".jpeg":
					case ".jpg":

						try
						{
							bitmap = await Task.Run(() => new Bitmap(item.FilePath));
						}
						catch (Exception)
						{
							break;
						}
						if(bitmap is not null)
						{
							bitmap = ResizeBitmap(bitmap, Width);
						}


						break;

					case ".pdf":
						if (!getIcon) break; 
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/picture_as_pdf_FILL0_wght400_GRAD0_opsz24.png")));
						bitmap = ResizeBitmap(bitmap, Width);
						break;

					case ".zip":
						if (!getIcon) break;
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/folder_zip_FILL1_wght400_GRAD0_opsz24.png")));
						bitmap = ResizeBitmap(bitmap, Width);
						break;

					case ".mp4":
					case ".mov":
					case ".ts":
					case ".mkv":
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/movie_FILL1_wght400_GRAD0_opsz24.png")));
						bitmap = ResizeBitmap(bitmap, Width);
						break;

					case ".cs":
					case ".vb":
					case ".py":
					case ".java":
					case ".rb":
					case ".c":
					case ".cpp":
					case ".xml":
					case ".xaml":
					case ".axaml":
						if (!getIcon) break;
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/code_FILL1_wght400_GRAD0_opsz24.png")));
						bitmap = ResizeBitmap(bitmap, Width);
						break;

					case ".html":
						if (!getIcon) break;
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/html_FILL1_wght400_GRAD0_opsz24.png")));
						bitmap = ResizeBitmap(bitmap, Width);
						break;

					default:
						if (!getIcon) break;
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/draft_FILL1_wght400_GRAD0_opsz24.png")));
						bitmap = ResizeBitmap(bitmap, Width);
						break;  // 何もしない
				}
			}
			else if (Directory.Exists(item.FilePath))
			{

			}

			else if (!String.IsNullOrEmpty(item.Url))
			{
				bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/link_FILL1_wght400_GRAD0_opsz24.png")));
				bitmap = ResizeBitmap(bitmap, Width);
			}

			return bitmap;

			Bitmap ResizeBitmap(Bitmap bmp, int width)
			{
				double scale = (double)width / bmp.PixelSize.Width;
				PixelSize pixelSize = new((int)(bmp.PixelSize.Width * scale), (int)(bmp.PixelSize.Height * scale));
				bmp = bmp.CreateScaledBitmap(pixelSize, BitmapInterpolationMode.LowQuality);
				return bmp;
			}
		}

		public async Task<int> AddToDBAsync()
		{
			this.AddedDate = DateTime.Now;

			await ItemAnalysis.InstantAnalysisAsync(this);

			var items = db_context.GetCollection<Item>("items");
			int returnId = items.Insert(this);
			return returnId;
		}

		public async Task MakeIndexAsync()
		{
			await ItemAnalysis.DeepAnalysisAsync(this);
			await this.UpdateDBAsync();
		}

		public async Task UpdateDBAsync()
		{
			var items = db_context.GetCollection<Item>("items");
			items.Update(this);
		}

		public async Task DeleteFromDBAsync()
		{
			if (this.FilePath != null && this.FilePath != "" && File.Exists(this.FilePath)) //もしファイルパスが定義されており、かつファイルが存在する場合
			{
				await Task.Run(() =>
				{
					File.Delete(this.FilePath);
				});
			}

			var items = db_context.GetCollection<Item>("items");
			items.Delete(this.Id);
		}

		
	}
}
