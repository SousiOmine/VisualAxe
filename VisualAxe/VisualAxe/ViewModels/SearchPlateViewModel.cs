using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualAxe.Models;

namespace VisualAxe.ViewModels
{
	public class SearchPlateViewModel : ViewModelBase
	{
		private SearchInfo _info;
		private string? _word;
		private IBrush? _color;

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

		public SearchPlateViewModel(SearchInfo info) 
		{
			_info = info;
			if (info.word is not null) Word = info.word;
			if (info.color is not null) Color = Brush.Parse(info.color.ToString());
		}

		public SearchInfo GetSearchInfo()
		{
			return _info;
		}
	}
}
