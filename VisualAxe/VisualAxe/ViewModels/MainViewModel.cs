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
		public ObservableCollection<ItemViewModel> ItemsToDisplay { get; } = new();
		private ObservableCollection<ItemViewModel> _resultItems { get; } = new();
		public ObservableCollection<ItemViewModel> SelectedItems { get; } = new();  //選択しているアイテム
		private ItemViewModel? _selectedItem;
		private string? _searchText;
		private bool _isBusy;
		private int _loadLimit = 200;
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

		public bool IsBusy
		{
			get => _isBusy;
			set => this.RaiseAndSetIfChanged(ref _isBusy, value);
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
				DoSearchItems(SearchText);
				PartialLoad(0, _loadLimit, true);
			});
			MoreShowItem = ReactiveCommand.Create(() =>
			{
				PartialLoad(_loadLimit, _loadLimit + 200, false);
				_loadLimit += 200;
			});
			DoSearchItems("");
			PartialLoad(0, _loadLimit, false);
		}

		public ICommand OpenItem { get; }
		public ICommand DeleteItem { get; }
		public ICommand MoreShowItem { get; }

		private async void PartialLoad(int start, int end, bool clear)    //startからendまで読み込む clearがfalseであれば既存のItemsを消さない
		{
			IsBusy = true;
			//もしLoadが事前に実行中ならそっちはキャンセルするためのもの
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = _cancellationTokenSource.Token;

			if (clear) ItemsToDisplay.Clear();
			for(int i = start; i < end; i++)
			{
				if(i >= _resultItems.Count) break;
				ItemsToDisplay.Add(_resultItems[i]);
			}
			for (int i = start; i < end; i++)
			{
				if (i >= _resultItems.Count) break;
				await ItemsToDisplay[i].LoadPreviewAsync();
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}
			}

			IsBusy = false;
		}

		private async void DoSearchItems(string? s)
		{
			IsBusy = true;

			if (System.String.IsNullOrWhiteSpace(s))
			{
				//もじ検索ワードが空白なら普通にぜんぶ読み込む
				var itemfromdb = await Item.GetAllItems();
				_resultItems.Clear();
				foreach (var item in itemfromdb)
				{
					_resultItems.Add(new ItemViewModel(item));
				}
			}
			else
			{
				
			}

			IsBusy = false;
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
				DoSearchItems(SearchText);
				PartialLoad(0, _loadLimit, true);
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
			DoSearchItems(SearchText);
			PartialLoad(0, _loadLimit, true);
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
			DoSearchItems(SearchText);
			PartialLoad(0, _loadLimit, true);
		}
	}
}
