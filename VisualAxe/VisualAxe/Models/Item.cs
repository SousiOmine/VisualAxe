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
					if (item.Title.Contains(s)) results.Add(item);
					continue;
				}
				if (!String.IsNullOrWhiteSpace(item.Memo))
				{
					if (item.Memo.Contains(s)) results.Add(item);
					continue;
				}
				if (!String.IsNullOrWhiteSpace(item.Url))
				{
					if (item.Url.Contains(s)) results.Add(item);
					continue;
				}
				if (!String.IsNullOrWhiteSpace(item.Index))
				{
					if (item.Index.Contains(s)) results.Add(item);
					continue;
				}
			}
			results.Sort((x, y) => y.AddedDate.CompareTo(x.AddedDate)); //追加日時順にソート

			return results;
		}

		public async Task AddToDB()
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
			items.Insert(this);
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
