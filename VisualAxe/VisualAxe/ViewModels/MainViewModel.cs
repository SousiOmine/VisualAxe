using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisualAxe.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VisualAxe.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		public ObservableCollection<ItemViewModel> Items { get; } = new();
		public ObservableCollection<ItemViewModel> SelectedItems { get; } = new();  //選択しているアイテム
		private ItemViewModel? _selectedItem;
		private string? _searchText;

		public string? SearchText
		{
			get => _searchText;
			set => this.RaiseAndSetIfChanged(ref _searchText, value);
		}

		public ItemViewModel? SelectedItem
		{
			get => _selectedItem;
			set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
		}

		public MainViewModel()
		{
			AddItem = ReactiveCommand.Create(() =>
			{
				Item item = new Item()
				{
					Title = "test"
				};
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

		public void DropsFiles(IDataObject data)
		{
			if(data.GetText() != null)
			{
				Item item = new Item()
				{
					Title = data.GetText()
				};
				item.AddToDB();
				LoadFromDB();
				return;
			}
			List<string> path = new();
			foreach(var item in data.GetFiles())
			{
				path.Add(item.Path.ToString());
			}
			if(path.Count > 0)
			{
				SearchText = path[0];
			}
		}

		public void AddItemFromDialog(IReadOnlyList<IStorageFile>? files)
		{
			/*List<string> filename = new();
			foreach (var file in files)
			{
				filename.Add(file.Name);
			}
			foreach (var i in filename)
			{
				Item item = new Item()
				{
					Title = i,
				};
				item.AddToDB();
			}*/
			foreach (var file in files)
			{
				var item = new Item()
				{
					Title = file.Name,
					FilePath = file.Path.ToString().Replace(@"file:///", "")
				};
				item.AddToDB();
			}
			LoadFromDB();
		}
	}
}
