using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisualAxe.Models;

namespace VisualAxe.ViewModels
{
	public class SearchPlateViewModel : ViewModelBase
	{
		private SearchInfo _info;
		private string? _word;
		private IBrush? _color;
		private bool _isPinned;

		private MainViewModel _mainViewModel;

		public string? Word
		{
			get => _word;
			set => this.RaiseAndSetIfChanged(ref _word, value);
		}

		public IBrush? Color
		{
			get => _color;
			set => this.RaiseAndSetIfChanged(ref _color, value);
		}

		public bool IsPinned
		{
			get => _isPinned;
			set => this.RaiseAndSetIfChanged(ref _isPinned, value);
		}

		public SearchPlateViewModel(SearchInfo info, bool isPinned, MainViewModel mainViewModel) 
		{
			_info = info;
			if (info.word is not null) Word = info.word;
			if (info.color is not null) Color = Brush.Parse(info.color.ToString());

			IsPinned = isPinned;
			_mainViewModel = mainViewModel;

			Pinned = ReactiveCommand.Create(() =>
			{
				_info.AddToDB();
				_mainViewModel.LoadPinPlates();
			});
			Unpinned = ReactiveCommand.Create(() =>
			{
				_info.DeleteFromDB();
				_mainViewModel.LoadPinPlates();
			});
		}

		public ICommand Pinned { get; }
		public ICommand Unpinned { get; }

		public SearchInfo GetSearchInfo()
		{
			return _info;
		}
	}
}
