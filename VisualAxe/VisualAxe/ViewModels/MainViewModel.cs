using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
		private CancellationTokenSource? _cancellationTokenSource;

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
			OpenItem = ReactiveCommand.Create(() =>
			{
				foreach(var item in SelectedItems)
				{
					item.OpenByProcess();
				}
			});
			DeleteItem = ReactiveCommand.Create(async () =>
			{
				foreach (var item in SelectedItems)
				{
					await item.DeleteAsync();
				}
				LoadFromDB();
			});
			MoreShowItem = ReactiveCommand.Create(() =>
			{

			});
			LoadFromDB();
		}

		public ICommand OpenItem { get; }
		public ICommand DeleteItem { get; }
		public ICommand MoreShowItem { get; }

		private async void LoadFromDB()
		{
			//もしLoadFromDBが事前に実行中ならそっちはキャンセルするためのもの
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = _cancellationTokenSource.Token;

			var itemfromdb = await Item.GetAllItems();
			Items.Clear();
			foreach (var item in itemfromdb)
			{
				Items.Add(new ItemViewModel(item));
			}
			foreach (var item in Items)
			{
				await item.LoadPreviewAsync();
				if(cancellationToken.IsCancellationRequested)
				{
					return;
				}
			}
		}

		public async void DropsFiles(IDataObject data)
		{
			if(data.GetText() != null)	//もしデータがテキストだったら
			{
				Item item = new Item()
				{
					Title = data.GetText()
				};
				await item.AddToDB();
				LoadFromDB();
				return;
			}
			foreach (var file in data.GetFiles())
			{
				var item = new Item()
				{
					Title = file.Name,
					FilePath = file.Path.ToString().Replace(@"file:///", "")
				};
				await item.AddToDB();
			}
			LoadFromDB();
		}

		public async void AddItemFromDialog(IReadOnlyList<IStorageFile>? files)
		{
			foreach (var file in files)
			{
				var item = new Item()
				{
					Title = file.Name,
					FilePath = file.Path.ToString().Replace(@"file:///", "")
				};
				await item.AddToDB();
			}
			LoadFromDB();
		}
	}
}
