using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualAxe.Models;

namespace VisualAxe.ViewModels
{
	public class ItemViewModel : ViewModelBase
	{
		private Item _item;
		public string Title => _item.Title;

		public ItemViewModel(Item item)
		{
			_item = item;
		}

		public void Delete()	//ViewModelから保持しているItemを削除できる
		{
			_item.DeleteFromDB();
		}
	}
}
