using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisualAxe.Models;

namespace VisualAxe.ViewModels
{
	public class SideViewModel : ViewModelBase
	{
		private Item _item;
		private string? _title;
		private string? _memo;
		private string? _url;
		private Bitmap? _previewBitmap;

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

		public string? Url
		{
			get { return _url; }
			set { this.RaiseAndSetIfChanged(ref _url, value); }
		}

		public Bitmap? PreviewBitmap
		{
			get { return _previewBitmap; }
			set { this.RaiseAndSetIfChanged(ref _previewBitmap, value); }
		}

		public SideViewModel(Item item)
		{
			_item = item;

			Title = item.Title;
			Memo = item.Memo;
			Url = item.Url;

			OpenUrl = ReactiveCommand.Create(() =>
			{
				ProcessStartInfo psi = new ProcessStartInfo()
				{
					FileName = Url,
					UseShellExecute = true
				};
				Process.Start(psi);
			});
		}

		public ICommand OpenUrl { get; }

		public async void LoadPreview()
		{
			PreviewBitmap = await Item.GetPreviewAsync(_item, 600);
		}
	}
}
