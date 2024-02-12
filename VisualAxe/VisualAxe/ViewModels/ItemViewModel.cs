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

		public async Task LoadPreviewAsync()
		{
			if (File.Exists(_item.FilePath))
			{
				switch(Path.GetExtension(_item.FilePath))
				{
					case ".png":
						Bitmap bmp;
						try
						{
							bmp = await Task.Run(() => new Bitmap(_item.FilePath));
						}
						catch (Exception)
						{
							break;
						}

						PreviewBitmap = bmp;
						break;

					default:
						break;
				}
			}
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
