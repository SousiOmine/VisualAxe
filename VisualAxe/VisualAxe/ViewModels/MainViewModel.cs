using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisualAxe.Models;

namespace VisualAxe.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		public ObservableCollection<ItemViewModel> Items { get; } = new();
		public ObservableCollection<ItemViewModel> SelectedItems { get; } = new();	//選択しているアイテム

		public MainViewModel()
		{
			AddItem = ReactiveCommand.Create(() =>
			{
				Item item = new();
				item.AddToDB();
				LoadFromDB();
			});
			DeleteItem = ReactiveCommand.Create(() =>
			{
				foreach (var item in SelectedItems)
				{
					item.Delete();
					
				}
				LoadFromDB();
			});
			LoadFromDB();
		}

		public ICommand AddItem { get; }
		public ICommand DeleteItem { get; }

		private async void LoadFromDB()
		{
			var itemfromdb = await Item.GetAllItems();
			Items.Clear();
			foreach (var item in itemfromdb)
			{
				Items.Add(new ItemViewModel(item));
			}
		}
	}
}
