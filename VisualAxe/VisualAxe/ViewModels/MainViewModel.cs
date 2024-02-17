using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform.Storage;
using Avalonia.Media;
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
using Avalonia.Input.Platform;

namespace VisualAxe.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		public ObservableCollection<ItemViewModel> ItemsToDisplay { get; } = new();
		public ObservableCollection<SideViewModel> SideViews { get; } = new();
		public ObservableCollection<ItemViewModel> SelectedItems { get; set; } = new();  //選択しているアイテム
		public ObservableCollection<SearchPlateViewModel> HistoryPlates { get; } = new();
		public ObservableCollection<SearchPlateViewModel> PinPlates { get; } = new();

		private ObservableCollection<ItemViewModel> _resultItems { get; } = new();
		private ItemViewModel? _selectedItem;
		private SearchPlateViewModel? _selectHistoryPlate;
		private SearchPlateViewModel? _selectPinPlate;
		private int _selectHistoryPlateIndex;
		private int _selectPinPlateIndex;
		private string? _searchText;
		private Color? _searchColor;
		private bool _useSearchColor;
		private bool _isBusy;
		private int _loadLimit = 100;
		private CancellationTokenSource? _cancellationTokenSource;

		public string? SearchText
		{
			get => _searchText;
			set {
				this.RaiseAndSetIfChanged(ref _searchText, value);
				//DoSearchItems(_searchText);
			} 
		}

		public Color? SearchColor
		{
			get => _searchColor;
			set => this.RaiseAndSetIfChanged(ref _searchColor, value);
		}

		public bool UseSearchColor
		{
			get => _useSearchColor;
			set => this.RaiseAndSetIfChanged(ref _useSearchColor, value);
		}

		public ItemViewModel? SelectedItem
		{
			get => _selectedItem;
			set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
		}

		public SearchPlateViewModel? SelectHistoryPlate
		{
			get => _selectHistoryPlate;
			set
			{
				this.RaiseAndSetIfChanged(ref _selectHistoryPlate, value);
				if (value is not null)
				{
					SearchText = value.GetSearchInfo().word;
					SearchColor = value.GetSearchInfo().color;
					UseSearchColor = (value.GetSearchInfo().color is not null);
				}
			}
		}

		public SearchPlateViewModel? SelectPinPlate
		{
			get => _selectPinPlate;
			set
			{
				this.RaiseAndSetIfChanged(ref _selectPinPlate, value);
				if (value is not null)
				{
					if (SearchText != value.GetSearchInfo().word) SearchText = value.GetSearchInfo().word;
					if (SearchColor != value.GetSearchInfo().color) SearchColor = value.GetSearchInfo().color;
					if (UseSearchColor == (value.GetSearchInfo().color is null))
					{
						UseSearchColor = (value.GetSearchInfo().color is not null);
					}
						
				}
			}
		}

		public int SelectHistoryPlateIndex
		{
			get => _selectHistoryPlateIndex;
			set
			{
				this.RaiseAndSetIfChanged(ref _selectHistoryPlateIndex, value);
			}
		}

		public int SelectPinPlateIndex
		{
			get => _selectPinPlateIndex;
			set => this.RaiseAndSetIfChanged(ref _selectPinPlateIndex, value);		
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
				DoSearchItems();
				await PartialLoad(0, _loadLimit, true);
			});
			MoreShowItem = ReactiveCommand.Create(async () =>
			{
				await PartialLoad(_loadLimit, _loadLimit + 200, false);
				_loadLimit += 200;
			});

			this.WhenAnyValue(x => x.SearchText)
				.Throttle(TimeSpan.FromMilliseconds(300))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(_ => DoSearchItems());
			this.WhenAnyValue(x => x.SearchColor)
				.Throttle(TimeSpan.FromMilliseconds(500))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(_ => DoSearchItems());
			this.WhenAnyValue(x => x.UseSearchColor)
				.Throttle(TimeSpan.FromMilliseconds(100))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(_ => DoSearchItems());

			LoadPinPlates();
		}


		public ICommand OpenItem { get; }
		public ICommand DeleteItem { get; }
		public ICommand MoreShowItem { get; }

		private async Task<bool> PartialLoad(int start, int end, bool clear)    //startからendまで読み込む clearがfalseであれば既存のItemsを消さない
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
				if (cancellationToken.IsCancellationRequested)
				{
					IsBusy = false;
					return false;
				}
			}
			for (int i = start; i < end; i++)
			{
				if (i >= _resultItems.Count) break;
				await ItemsToDisplay[i].LoadPreviewAsync();
				if (cancellationToken.IsCancellationRequested)
				{
					IsBusy = false;
					return false;
				}
			}

			IsBusy = false;
			return true;
		}

		private async void GetResultFromDB(SearchInfo searchInfo)
		{
			IsBusy = true;
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = _cancellationTokenSource.Token;

			
			var resultfromdb = await Item.SearchAsync(searchInfo);

			_resultItems.Clear();
			foreach (var item in resultfromdb)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}
				_resultItems.Add(new ItemViewModel(item, this));
			}

			IsBusy = false;
		}

		private async void DoSearchItems()
		{
			string? useword = SearchText;
			Color? usecolor = null;
			if (UseSearchColor) usecolor = SearchColor;

			var searchInfo = new SearchInfo()
			{
				word = useword,
				color = usecolor
			};

			GetResultFromDB(searchInfo);
			bool SearchComplete = await PartialLoad(0, _loadLimit, true);
			if (SearchComplete) {
				//SearchInfoの内容が検索履歴の最も新しいものと異なれば検索履歴に追加する
				if(HistoryPlates.Count == 0 || HistoryPlates[0].GetSearchInfo().word != searchInfo.word || HistoryPlates[0].GetSearchInfo().color != searchInfo.color)
				{
					if (String.IsNullOrEmpty(searchInfo.word) && searchInfo.color == null)
					{
						return;
					}
					HistoryPlates.Insert(0, new SearchPlateViewModel(searchInfo, false, this));
					SelectHistoryPlateIndex = -1;
					SelectPinPlateIndex = -1;

				}		
			}
		}

		public async void DropsFiles(IDataObject data)
		{
			if (data is null) return;
			if (data.GetText() is not null) //もしデータがテキストだったら
			{
				Item item = new Item();
				item.Title = data.GetText();
				if(data.GetText().Contains("https://") || data.GetText().Contains("http://"))
				{
					item.Url = data.GetText();
				}

				await item.AddToDBAsync();
				DoSearchItems();
				await PartialLoad(0, _loadLimit, true);
				await item.MakeIndexAsync();
				return;
			}

			if (data.GetFiles() is null) return;
			var files = new Collection<Item>();
			foreach (var file in data.GetFiles())
			{
				var item = new Item()
				{
					Title = file.Name,
					FilePath = file.Path.ToString().Replace(@"file:///", "")
				};
				await item.AddToDBAsync();
				files.Add(item);
			}

			DoSearchItems();

			foreach (var item in files)
			{
				
				await item.MakeIndexAsync();
			}
		}

		public async void AddItemFromDialog(IReadOnlyList<IStorageFile>? files)
		{
			if (files == null) return;
			var collectFiles = new Collection<Item>();
			foreach (var file in files)
			{
				var item = new Item()
				{
					Title = file.Name,
					FilePath = file.Path.ToString().Replace(@"file:///", "")
				};
				await item.AddToDBAsync();
				collectFiles.Add(item);
			}
			DoSearchItems();

			foreach (var item in collectFiles)
			{
				await item.MakeIndexAsync();
			}
		}

		private void SideViewReflection(object? sender, NotifyCollectionChangedEventArgs e)	//SideViewsの中身をSelectedItemsと同期する
		{
			SideViews.Clear();
			foreach (var item in SelectedItems)
			{

				SideViews.Insert(0, new SideViewModel(item.GetItem()));
			}
			foreach (var item in SideViews)
			{
				item.LoadPreview();
			}
		}

		public void LoadPinPlates()
		{
			SelectPinPlateIndex = -1;
			PinPlates.Clear();

			var pinsfromdb = SearchInfo.GetAllFromDB();
			foreach (var item in pinsfromdb)
			{
				PinPlates.Add(new SearchPlateViewModel(item, true, this));
			}
		}
	}
}
