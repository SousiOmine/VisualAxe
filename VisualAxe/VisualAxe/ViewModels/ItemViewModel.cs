using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisualAxe.Models;

namespace VisualAxe.ViewModels
{
	public class ItemViewModel : ViewModelBase
	{
		private Item _item;
		private MainViewModel _mainViewModel;
		private Bitmap? _previewBitmap;
		public string? Title => _item.Title;
		public bool HaveFile => !String.IsNullOrWhiteSpace(_item.FilePath);
		public bool HaveUrl => !String.IsNullOrWhiteSpace(_item.Url);
		public Bitmap? PreviewBitmap
		{
			get => _previewBitmap;
			set => this.RaiseAndSetIfChanged(ref _previewBitmap, value);
		}

		public ItemViewModel(Item item, MainViewModel mainViewModel)
		{
			_item = item;
			_mainViewModel = mainViewModel;


			OpenFile = ReactiveCommand.Create(() =>
			{
				_mainViewModel.OpenFile.Execute(null);
			});
			OpenUrl = ReactiveCommand.Create(() =>
			{
				_mainViewModel.OpenUrl.Execute(null);
			});
			DeleteItem = ReactiveCommand.Create(async () =>
			{
				_mainViewModel.DeleteItem.Execute(null);
			});
		}

		public ICommand OpenUrl { get; }
		public ICommand OpenFile { get; }
		public ICommand DeleteItem { get; }

		

		public Item GetItem()
		{
			return _item;
		}

		public async Task LoadPreviewAsync()
		{
			PreviewBitmap = await Item.GetPreviewAsync(_item, 200, true);
		}

		public void OpenByProcess()
		{
			if(File.Exists(_item.FilePath))
			{
				var startInfo = new System.Diagnostics.ProcessStartInfo()
				{
					FileName = _item.FilePath,
					UseShellExecute = true,
					CreateNoWindow = true
				};
				System.Diagnostics.Process.Start(startInfo);

			}
		}

		public void OpenByBrowser()
		{
			if (_item.Url is not null)
			{
				ProcessStartInfo psi = new ProcessStartInfo()
				{
					FileName = _item.Url,
					UseShellExecute = true
				};
				Process.Start(psi);
			}
		}

		public async Task DeleteAsync()	//ViewModelから保持しているItemを削除できる
		{
			await _item.DeleteFromDBAsync();
			if(PreviewBitmap != null) PreviewBitmap.Dispose();
		}
	}
}
