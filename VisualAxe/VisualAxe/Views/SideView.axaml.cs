using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using VisualAxe.Models;

namespace VisualAxe.Views;

public partial class SideView : UserControl
{
    /*public static readonly StyledProperty<IEnumerable<Item>> ItemsSourceProperty = 
        AvaloniaProperty.Register<SideView, IEnumerable<Item>>(nameof(ItemsSource));

    public IEnumerable<Item> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}*/

	public SideView()
    {
        InitializeComponent();
    }
}