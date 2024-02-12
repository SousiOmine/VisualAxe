using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using VisualAxe.ViewModels;

namespace VisualAxe.Views;

public partial class MainView : UserControl
{
    private MainViewModel VM
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
		this.VM.DropsFiles(dropData);
	}
}