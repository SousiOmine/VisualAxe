using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
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
		public ObservableCollection<SideViewModel> SideViews { get; } = new();
		public ObservableCollection<ItemViewModel> SelectedItems { get; set; } = new();  //選択しているアイテム
		private ObservableCollection<ItemViewModel> _resultItems { get; } = new();
		private ItemViewModel? _selectedItem;
		private string? _searchText;
		private bool _isBusy;
		private int _loadLimit = 200;
		private CancellationTokenSource? _cancellationTokenSource;

		public string? SearchText
		{
			get => _searchText;
			set {
				this.RaiseAndSetIfChanged(ref _searchText, value);
				//DoSearchItems(_searchText);
			} 
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
			SelectedItems.CollectionChanged += SideViewReflection;
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

			this.WhenAnyValue(x => x.SearchText)
				.Throttle(TimeSpan.FromMilliseconds(300))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(DoSearchItems!);

			DoSearchItems("");

			
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

		private async void GetResultFromDB(string? s)
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
				var resultfromdb = await Item.SearchString(s);
				_resultItems.Clear();
				foreach (var item in resultfromdb)
				{
					_resultItems.Add(new ItemViewModel(item));
				}
			}

			IsBusy = false;
		}

		private void DoSearchItems(string? s)
		{
			GetResultFromDB(s);
			PartialLoad(0, _loadLimit, true);
		}

		public async void DropsFiles(IDataObject data)
		{
			if (data is null) return;
			if (data.GetText() != null) //もしデータがテキストだったら
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

			//var files = new Collection<Item>();
			foreach (var file in data.GetFiles())
			{
				var item = new Item()
				{
					Title = file.Name,
					FilePath = file.Path.ToString().Replace(@"file:///", "")
				};
				await item.AddToDB();
				await item.Analysis();
			}

			DoSearchItems(SearchText);
			PartialLoad(0, _loadLimit, true);
		}

		public async void AddItemFromDialog(IReadOnlyList<IStorageFile>? files)
		{
			if (files == null) return;
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

		private void SideViewReflection(object? sender, NotifyCollectionChangedEventArgs e)	//SideViewsの中身をSelectedItemsと同期する
		{
			SideViews.Clear();
			foreach (var item in SelectedItems)
			{
				SideViews.Add(new SideViewModel(item.GetItem()));
			}
		}
	}
}
