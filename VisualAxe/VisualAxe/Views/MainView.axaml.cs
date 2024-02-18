using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VisualAxe.ViewModels;

namespace VisualAxe.Views;

public partial class MainView : UserControl
{
    private MainViewModel? VM
    {
        get { return this.DataContext as MainViewModel; }
	}

    public MainView()
    {
        InitializeComponent();
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DropEvent, DropFile);
	}

    private void DropFile(object? sender, DragEventArgs e)
    {
		var dropData = e.Data;
		this.VM?.DropsFiles(dropData);
	}

	private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Open File",
                AllowMultiple = true
        });
        this.VM?.AddItemFromDialog(files);
	}

	private void MenuCloseItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
        MainWindow.GetInstance().Close();
	}
}