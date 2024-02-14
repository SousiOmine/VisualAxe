using Avalonia;
using Avalonia.Media.Imaging;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
		public string? Index { get; set; }

		private static LiteDatabase db_context = new("./data.db");
		private static readonly string ItemsStorageName = "ItemsStorage";	//アイテムのファイルを格納しておくフォルダ

		public static async Task<List<Item>> GetAllItems()
		{
			var items = new List<Item>();
			items = db_context.GetCollection<Item>("items").FindAll().ToList();
			items.Sort((x, y) => y.AddedDate.CompareTo(x.AddedDate));	//追加日時順にソート
			return items;
		}

		public static async Task<List<Item>> SearchString(string s)
		{
			var items = new List<Item>();
			items = db_context.GetCollection<Item>("items").FindAll().ToList();
			var results = new List<Item>();
			foreach (var item in items)
			{
				if (!String.IsNullOrWhiteSpace(item.Title))
				{
					if (item.Title.Contains(s))
					{
						results.Add(item);
						continue;
					}
				}
				if (!String.IsNullOrWhiteSpace(item.Memo))
				{
					if (item.Memo.Contains(s))
					{
						results.Add(item);
						continue;
					}
				}
				if (!String.IsNullOrWhiteSpace(item.Url))
				{
					if (item.Url.Contains(s))
					{
						results.Add(item);
						continue;
					}
				}
				if (!String.IsNullOrWhiteSpace(item.Index))
				{
					if (item.Index.Contains(s))
					{
						results.Add(item);
						continue;
					}
				}
			}
			results.Sort((x, y) => y.AddedDate.CompareTo(x.AddedDate)); //追加日時順にソート

			return results;
		}

		public static async Task<List<Item>> Search(SearchInfo info)
		{
			var all_items = new List<Item>();
			all_items = db_context.GetCollection<Item>("items").FindAll().ToList();
			var word_results = new List<Item>();

			if(!String.IsNullOrEmpty(info.word))	//もし検索文字列が空でなければ文字で絞り込み
			{
				foreach (var item in all_items)
				{
					if (!String.IsNullOrEmpty(item.Title))
					{
						if (item.Title.Contains(info.word))
						{
							word_results.Add(item);
							continue;
						}
					}
					if (!String.IsNullOrEmpty(item.Memo))
					{
						if (item.Memo.Contains(info.word))
						{
							word_results.Add(item);
							continue;
						}
					}
					if (!String.IsNullOrEmpty(item.Url))
					{
						if (item.Url.Contains(info.word))
						{
							word_results.Add(item);
							continue;
						}
					}
					if (!String.IsNullOrEmpty(item.Index))
					{
						if (item.Index.Contains(info.word))
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

			var color_results = new List<Item>();
			if(info.color is not null)
			{
				//ここに色での絞り込みを書く
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

			return results;
		}

		public static async Task<Bitmap?> GetPreviewAsync(Item item, int Width)
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
							double scale = (double)Width / bitmap.PixelSize.Width;
							PixelSize pixelSize = new((int)(bitmap.PixelSize.Width * scale), (int)(bitmap.PixelSize.Height * scale));
							bitmap = bitmap.CreateScaledBitmap(pixelSize, BitmapInterpolationMode.LowQuality);
						}


						break;

					default:
						break;  // 何もしない
				}
			}
			else if (Directory.Exists(item.FilePath))
			{

			}

			return bitmap;
		}

		public async Task<int> AddToDB()
		{
			this.AddedDate = DateTime.Now;
			if (this.FilePath != null && this.FilePath != "" && File.Exists(this.FilePath))	//もしファイルパスが定義されており、かつファイルが存在する場合
			{
				await Task.Run(() => {
					if (!Directory.Exists("." + Path.DirectorySeparatorChar + ItemsStorageName))    //格納用フォルダがなければ作成
					{
						Directory.CreateDirectory("." + Path.DirectorySeparatorChar + ItemsStorageName);
					}
				});
				string rand_folder = Guid.NewGuid().ToString().Substring(0, 16);    //GUIDを使ってランダムな16文字を用意
				await Task.Run(() => {
					Directory.CreateDirectory("." + Path.DirectorySeparatorChar + ItemsStorageName + Path.DirectorySeparatorChar + rand_folder);
				});
				string copied_path = "." + Path.DirectorySeparatorChar + ItemsStorageName + Path.DirectorySeparatorChar + rand_folder + Path.DirectorySeparatorChar + Path.GetFileName(this.FilePath);
				await Task.Run(() => {
					File.Copy(this.FilePath, copied_path);
				});
				this.FilePath = copied_path;
			}

			var items = db_context.GetCollection<Item>("items");
			int returnId = items.Insert(this);
			return returnId;
		}

		public async Task Analysis()
		{
			//await Task.Delay(5000);
			this.Index = "AnalysisOK";
			this.UpdateDB();
		}

		public async void UpdateDB()
		{
			var items = db_context.GetCollection<Item>("items");
			items.Update(this);
		}

		public async Task DeleteFromDB()
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
