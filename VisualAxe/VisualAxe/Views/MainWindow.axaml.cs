using Avalonia.Controls;
using System;

namespace VisualAxe.Views
{
    public partial class MainWindow : Window
    {
        private static MainWindow? _instance;

        //MainWIndow�͂P�������݂��Ȃ��悤�V���O���g���ɂ��Ă���
        public static MainWindow GetInstance()
        {
            if (_instance is null)
            {
                _instance = new MainWindow();
			}
            return _instance;
        }

		private MainWindow()
        {
            InitializeComponent();
        }

		private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
		{
            e.Cancel = true;
			this.Hide();
		}

		private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			//������nownd���n����Ă���΃E�B���h�E��\��
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1 && args[1] == "nownd")
			{
				//�E�B���h�E��\��
				this.Hide();
			}
		}
	}
}