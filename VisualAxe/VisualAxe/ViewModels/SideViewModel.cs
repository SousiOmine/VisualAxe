using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualAxe.Models;

namespace VisualAxe.ViewModels
{
	public class SideViewModel : ViewModelBase
	{
		private Item _item;
		private string? _title;
		private string? _memo;

		public string? Title
		{
			get { return _title; }
			set { this.RaiseAndSetIfChanged(ref _title, value); }
		}

		public string? Memo
		{
			get { return _memo; }
			set {
				this.RaiseAndSetIfChanged(ref _memo, value);
				_item.Memo = value;
				_item.UpdateDB();
			}
		}

		public SideViewModel(Item item)
		{
			_item = item;
			Title = item.Title;
			Memo = item.Memo;
		}
	}
}
