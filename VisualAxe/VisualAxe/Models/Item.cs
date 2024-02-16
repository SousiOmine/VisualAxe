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
		private static readonly string ItemsStorageName = "ItemsStorage";	//アイテムのファイルを格納しておくフォルダ

		public static async Task<List<Item>> GetAllItems()
		{
			var items = new List<Item>();
			items = db_context.GetCollection<Item>("items").FindAll().ToList();
			items.Sort((x, y) => y.AddedDate.CompareTo(x.AddedDate));	//追加日時順にソート
			return items;
		}

		public static async Task<Item?> GetItem(int id)
		{	
			var items = db_context.GetCollection<Item>("items");
			
			Item item = items.FindById(id);
			return item;
		}

		public static Task<List<Item>> Search(SearchInfo info)
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
						if (Analysis.ColorDistance((Color)item.Colors[i], (Color)info.color) <= 100)
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
							double picscale = (double)Width / bitmap.PixelSize.Width;
							PixelSize picpixelSize = new((int)(bitmap.PixelSize.Width * picscale), (int)(bitmap.PixelSize.Height * picscale));
							bitmap = bitmap.CreateScaledBitmap(picpixelSize, BitmapInterpolationMode.LowQuality);
						}


						break;

					case ".pdf":
						if (!getIcon) break; 
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/picture_as_pdf_FILL0_wght400_GRAD0_opsz24.png")));
						double pdfscale = (double)Width / bitmap.PixelSize.Width;
						PixelSize pdfpixelSize = new((int)(bitmap.PixelSize.Width * pdfscale), (int)(bitmap.PixelSize.Height * pdfscale));
						bitmap = bitmap.CreateScaledBitmap(pdfpixelSize, BitmapInterpolationMode.LowQuality);
						break;

					case ".zip":
						if (!getIcon) break;
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/folder_zip_FILL1_wght400_GRAD0_opsz24.png")));
						double zipscale = (double)Width / bitmap.PixelSize.Width;
						PixelSize zippixelSize = new((int)(bitmap.PixelSize.Width * zipscale), (int)(bitmap.PixelSize.Height * zipscale));
						bitmap = bitmap.CreateScaledBitmap(zippixelSize, BitmapInterpolationMode.LowQuality);
						break;

					case ".mp4":
					case ".mov":
					case ".ts":
					case ".mkv":
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/movie_FILL1_wght400_GRAD0_opsz24.png")));
						double movscale = (double)Width / bitmap.PixelSize.Width;
						PixelSize movpixelSize = new((int)(bitmap.PixelSize.Width * movscale), (int)(bitmap.PixelSize.Height * movscale));
						bitmap = bitmap.CreateScaledBitmap(movpixelSize, BitmapInterpolationMode.LowQuality);
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
						double codescale = (double)Width / bitmap.PixelSize.Width;
						PixelSize codepixelSize = new((int)(bitmap.PixelSize.Width * codescale), (int)(bitmap.PixelSize.Height * codescale));
						bitmap = bitmap.CreateScaledBitmap(codepixelSize, BitmapInterpolationMode.LowQuality);
						break;

					case ".html":
						if (!getIcon) break;
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/html_FILL1_wght400_GRAD0_opsz24.png")));
						double htmlscale = (double)Width / bitmap.PixelSize.Width;
						PixelSize htmlpixelSize = new((int)(bitmap.PixelSize.Width * htmlscale), (int)(bitmap.PixelSize.Height * htmlscale));
						bitmap = bitmap.CreateScaledBitmap(htmlpixelSize, BitmapInterpolationMode.LowQuality);
						break;

					default:
						if (!getIcon) break;
						bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/draft_FILL1_wght400_GRAD0_opsz24.png")));
						double defscale = (double)Width / bitmap.PixelSize.Width;
						PixelSize defpixelSize = new((int)(bitmap.PixelSize.Width * defscale), (int)(bitmap.PixelSize.Height * defscale));
						bitmap = bitmap.CreateScaledBitmap(defpixelSize, BitmapInterpolationMode.LowQuality);
						break;  // 何もしない
				}
			}
			else if (Directory.Exists(item.FilePath))
			{

			}

			else if (!String.IsNullOrEmpty(item.Url))
			{
				bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://VisualAxe/Assets/link_FILL1_wght400_GRAD0_opsz24.png")));
				double picscale = (double)Width / bitmap.PixelSize.Width;
				PixelSize picpixelSize = new((int)(bitmap.PixelSize.Width * picscale), (int)(bitmap.PixelSize.Height * picscale));
				bitmap = bitmap.CreateScaledBitmap(picpixelSize, BitmapInterpolationMode.LowQuality);
			}

			return bitmap;
		}

		public async Task<int> AddToDB()
		{
			this.AddedDate = DateTime.Now;

			//ファイルがなくてもフォルダはとりあえず作成しておく
			await Task.Run(() =>
			{
				if (!Directory.Exists("." + Path.DirectorySeparatorChar + ItemsStorageName))    //格納用フォルダがなければ作成
				{
					Directory.CreateDirectory("." + Path.DirectorySeparatorChar + ItemsStorageName);
				}
			});
			string rand_folder = Guid.NewGuid().ToString().Substring(0, 16);    //GUIDを使ってランダムな16文字を用意
			await Task.Run(() =>
			{
				Directory.CreateDirectory("." + Path.DirectorySeparatorChar + ItemsStorageName + Path.DirectorySeparatorChar + rand_folder);
			});

			if (this.FilePath is not null && this.FilePath != "" && File.Exists(this.FilePath)) //もしファイルパスが定義されており、かつファイルが存在する場合はコピー
			{
				string copied_path = "." + Path.DirectorySeparatorChar + ItemsStorageName + Path.DirectorySeparatorChar + rand_folder + Path.DirectorySeparatorChar + Path.GetFileName(this.FilePath);
				await Task.Run(() =>
				{
					File.Copy(this.FilePath, copied_path);
				});
				this.FilePath = copied_path;
			}
			else if (this.Url is not null)  //もしURLが定義されていた場合
			{
				//画像ならダウンロードしフォルダに配置する
				if(this.Url.Contains("png", StringComparison.OrdinalIgnoreCase) || this.Url.Contains("jpg", StringComparison.OrdinalIgnoreCase) || this.Url.Contains("jpeg", StringComparison.OrdinalIgnoreCase))
				{
					string download_path = "." + Path.DirectorySeparatorChar + ItemsStorageName + Path.DirectorySeparatorChar + rand_folder + Path.DirectorySeparatorChar + "image.png";
					await Task.Run(async () =>
					{
						var client = new HttpClient();
						var responce = await client.GetAsync(this.Url);
						if(responce.IsSuccessStatusCode)
						{
							using var stream = await responce.Content.ReadAsStreamAsync();
							using var outStream = File.Create(download_path);
							stream.CopyTo(outStream);
							this.FilePath = download_path;
						}
					});
				}
				
			}

			var items = db_context.GetCollection<Item>("items");
			int returnId = items.Insert(this);
			return returnId;
		}

		public async Task MakeIndex()
		{
			//await Task.Delay(5000);
			Bitmap? mybmp = await Item.GetPreviewAsync(this, 200, false);
			if(mybmp is not null)	//もし画像があれば
			{	
				List<Color> colors = await Analysis.GetMajorColorAsync(mybmp, 50);
				this.Colors = [.. colors];
				mybmp.Dispose();
			}
			
			this.Index = "AnalysisOK";
			await this.UpdateDB();
		}

		public async Task UpdateDB()
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
