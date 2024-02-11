using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
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

				desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}