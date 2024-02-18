using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using VisualAxe.Server;
using VisualAxe.ViewModels;
using VisualAxe.Views;

namespace VisualAxe
{
    public partial class App : Application
    {
        private System.Threading.Mutex mutex;
        private const string MutexName = "VisualAxeApp";

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
				//アプリケーションがすでに起動している場合、既存のプロセスを終了させる
				bool createdNew;
                mutex = new System.Threading.Mutex(true, MutexName, out createdNew);
				if (!createdNew)
				{
					Process currentProcess = Process.GetCurrentProcess();
					foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
					{
						if (process.Id != currentProcess.Id)
						{
							process.Kill();
						}
					}
				}

                //アプリケーション終了時にmutexを開放するよう設定
                desktop.Exit += (_, _) =>
				{
                    if (mutex != null)
                    {
                        mutex.ReleaseMutex();
                        mutex.Close();
					}
				};

				MainWindow _mainWindowInstance = MainWindow.GetInstance();
                _mainWindowInstance.DataContext = new MainWindowViewModel();
                desktop.MainWindow = _mainWindowInstance;


                /*desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };*/
                

                RestAPI.ServerStart();
			}

            base.OnFrameworkInitializationCompleted();
        }

		private void ShutdownMenuItem_Click(object? sender, System.EventArgs e)
		{
            RestAPI.ServerStop();
			//アプリを終了する
			Environment.Exit(0);
		}

		private void ShowMenuItem_Click(object? sender, System.EventArgs e)
		{
			MainWindow _mainWindowInstance = MainWindow.GetInstance();
            _mainWindowInstance.Hide();
			_mainWindowInstance.Show();
		}
	}
}