using System.Windows;
using DesktopWallpaperApp.Utils;

namespace DesktopWallpaperApp
{
    public partial class App : Application
    {
        private AppSettings _settings;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 确保只运行一个实例
            var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            var runningProcesses = System.Diagnostics.Process.GetProcessesByName(processName);
            if (runningProcesses.Length > 1)
            {
                MessageBox.Show("应用程序已在运行中。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            // 加载设置
            _settings = SettingsManager.LoadSettings();

            // 启动性能优化
            PerformanceOptimizer.StartOptimization();
            PerformanceOptimizer.EnableLowMemoryMode(_settings.EnableLowMemoryMode);

            // 创建主窗口
            var mainWindow = new MainWindow(_settings);
            MainWindow = mainWindow;
            
            if (_settings.StartMinimized)
            {
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.ShowInTaskbar = false;
            }
            
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 保存设置
            if (MainWindow is MainWindow mainWindow)
            {
                mainWindow.SaveSettings();
            }

            // 停止性能优化
            PerformanceOptimizer.StopOptimization();

            base.OnExit(e);
        }
    }
}