using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VisualAxe.ViewModels;

namespace VisualAxe.Views;

public partial class ItemView : UserControl
{
    private ItemViewModel? VM
    {
        get { return this.DataContext as ItemViewModel; }
    }

    public ItemView()
    {
        InitializeComponent();
    }
}