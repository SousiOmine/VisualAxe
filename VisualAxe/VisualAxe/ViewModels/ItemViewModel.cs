using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualAxe.Models;

namespace VisualAxe.ViewModels
{
	public class ItemViewModel : ViewModelBase
	{
		private Item _item;
		private Bitmap? _previewBitmap;
		public string Title => _item.Title;

		public ItemViewModel(Item item)
		{
			_item = item;
		}

		public Bitmap? PreviewBitmap
		{
			get => _previewBitmap;
			set => this.RaiseAndSetIfChanged(ref _previewBitmap, value);
		}

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

		public async Task DeleteAsync()	//ViewModelから保持しているItemを削除できる
		{
			await _item.DeleteFromDB();
			if(PreviewBitmap != null) PreviewBitmap.Dispose();
		}
	}
}
