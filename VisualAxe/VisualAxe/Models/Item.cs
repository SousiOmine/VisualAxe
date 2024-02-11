using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualAxe.Models
{
	public class Item
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string FileName { get; set; }
		public string Memo { get; set; }
		public string Url { get; set; }
		public string Index { get; set; }

		private static LiteDatabase db_context = new("./data.db");

		public static async Task<List<Item>> GetAllItems()
		{
			var items = new List<Item>();
			items = db_context.GetCollection<Item>("items").FindAll().ToList();
			return items;
		}

		public async void AddToDB()
		{
			var items = db_context.GetCollection<Item>("items");
			items.Insert(this);
		}

		public async void UpdateDB()
		{
			var items = db_context.GetCollection<Item>("items");
			items.Update(this);
		}

		public async void DeleteFromDB()
		{
			var items = db_context.GetCollection<Item>("items");
			items.Delete(this.Id);
		}
	}
}
