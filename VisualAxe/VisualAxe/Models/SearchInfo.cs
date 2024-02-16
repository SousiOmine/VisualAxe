using System;
using System.Collections.Generic;
using Avalonia.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace VisualAxe.Models
{
	public class SearchInfo
	{
		public int Id { get; set; }
		public string? word { get; set; }
		public Color? color { get; set; }

		private static LiteDatabase db_context = new("./pinsearch.db");

		public static List<SearchInfo> GetAllFromDB()
		{
			var items = db_context.GetCollection<SearchInfo>("searchinfo").FindAll().ToList();
			return items;
		}

		public void AddToDB()
		{
			var items = db_context.GetCollection<SearchInfo>("searchinfo");
			items.Insert(this);
		}


		public void DeleteFromDB()
		{
			var items = db_context.GetCollection<SearchInfo>("searchinfo");
			items.Delete(this.Id);
		}
	}
}
