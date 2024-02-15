using Avalonia.Media;
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

		private IBrush? _color1;
		private IBrush? _color2;
		private IBrush? _color3;
		private IBrush? _color4;
		private IBrush? _color5;

		

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
				UpdateItem();
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

		public IBrush? Color1
		{
			get => _color1;
			set => this.RaiseAndSetIfChanged(ref _color1, value);
		}
		public IBrush? Color2
		{
			get => _color2;
			set => this.RaiseAndSetIfChanged(ref _color2, value);
		}
		public IBrush? Color3
		{
			get => _color3;
			set => this.RaiseAndSetIfChanged(ref _color3, value);
		}
		public IBrush? Color4
		{
			get => _color4;
			set => this.RaiseAndSetIfChanged(ref _color4, value);
		}
		public IBrush? Color5
		{
			get => _color5;
			set => this.RaiseAndSetIfChanged(ref _color5, value);
		}

		public SideViewModel(Item item)
		{
			_item = item;

			Title = item.Title;
			Memo = item.Memo;
			Url = item.Url;

			if (item.Colors is not null)
			{
				if (item.Colors.Count > 0) Color1 = Brush.Parse(item.Colors[0].ToString());
				if (item.Colors.Count > 1) Color2 = Brush.Parse(item.Colors[1].ToString());
				if (item.Colors.Count > 2) Color3 = Brush.Parse(item.Colors[2].ToString());
				if (item.Colors.Count > 3) Color4 = Brush.Parse(item.Colors[3].ToString());
				if (item.Colors.Count > 4) Color5 = Brush.Parse(item.Colors[4].ToString());
			}

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
			PreviewBitmap = await Item.GetPreviewAsync(_item, 600, true);
		}

		private async void UpdateItem()
		{
			Item? item = new Item();
			item = await Item.GetItem(_item.Id);
			if (item is null) return;
			item.Memo = this.Memo;
			await item.UpdateDB();
		}
	}
}
