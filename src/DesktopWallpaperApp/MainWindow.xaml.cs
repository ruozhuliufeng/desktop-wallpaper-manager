using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using DesktopWallpaperApp.Controls;
using DesktopWallpaperApp.Models;
using DesktopWallpaperApp.Services;
using DesktopWallpaperApp.Utils;

namespace DesktopWallpaperApp
{
    public partial class MainWindow : Window
    {
        private readonly List<UserControl> _widgets = new();
        private readonly DispatcherTimer _statusTimer;
        private readonly DispatcherTimer _memoryMonitorTimer;
        private OverlayWindow? _overlayWindow;
        private AppSettings _settings;

        public MainWindow(AppSettings settings)
        {
            InitializeComponent();
            
            _settings = settings;
            ApplySettings();

            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _statusTimer.Tick += (s, e) =>
            {
                StatusLabel.Content = "准备就绪";
                _statusTimer.Stop();
            };

            // 内存监控定时器
            _memoryMonitorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _memoryMonitorTimer.Tick += MemoryMonitorTimer_Tick;
            _memoryMonitorTimer.Start();

            LoadCurrentWallpaper();
            Loaded += MainWindow_Loaded;
        }

        private void ApplySettings()
        {
            Width = _settings.WindowWidth;
            Height = _settings.WindowHeight;
            Left = _settings.WindowLeft;
            Top = _settings.WindowTop;
            GlobalOpacitySlider.Value = _settings.GlobalOpacity;

            if (!string.IsNullOrEmpty(_settings.LastLayoutPath) && File.Exists(_settings.LastLayoutPath))
            {
                Loaded += (s, e) => LoadLayout(_settings.LastLayoutPath);
            }
        }

        private void MemoryMonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (PerformanceOptimizer.ShouldCleanMemory())
            {
                PerformanceOptimizer.ForceGarbageCollection();
            }

            // 在状态栏显示内存使用情况（调试时）
            #if DEBUG
            string memoryUsage = PerformanceOptimizer.GetFormattedMemoryUsage();
            StatusLabel.Content = $"内存使用: {memoryUsage}";
            #endif
        }

        public void SaveSettings()
        {
            _settings.WindowWidth = ActualWidth;
            _settings.WindowHeight = ActualHeight;
            _settings.WindowLeft = Left;
            _settings.WindowTop = Top;
            _settings.GlobalOpacity = GlobalOpacitySlider.Value;
            
            SettingsManager.SaveSettings(_settings);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateStatus("应用程序已启动");
        }

        private void UpdateStatus(string message)
        {
            StatusLabel.Content = message;
            _statusTimer.Stop();
            _statusTimer.Start();
        }

        private void LoadCurrentWallpaper()
        {
            try
            {
                string currentWallpaper = WallpaperService.GetCurrentWallpaper();
                if (!string.IsNullOrEmpty(currentWallpaper))
                {
                    CurrentWallpaperTextBlock.Text = Path.GetFileName(currentWallpaper);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"加载当前壁纸信息失败: {ex.Message}");
            }
        }

        private void SelectWallpaperButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择壁纸",
                Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|所有文件|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                
                if (WallpaperService.SetWallpaper(selectedFile))
                {
                    CurrentWallpaperTextBlock.Text = Path.GetFileName(selectedFile);
                    UpdateStatus("壁纸设置成功");
                }
                else
                {
                    UpdateStatus("壁纸设置失败");
                }
            }
        }

        private void AddClockButton_Click(object sender, RoutedEventArgs e)
        {
            AddClockWidget();
        }

        private void AddPomodoroButton_Click(object sender, RoutedEventArgs e)
        {
            AddPomodoroWidget();
        }

        private void AddClockWidget()
        {
            var clock = new ClockWidget();
            Canvas.SetLeft(clock, 50 + (_widgets.Count * 20));
            Canvas.SetTop(clock, 50 + (_widgets.Count * 20));
            
            WidgetCanvas.Children.Add(clock);
            _widgets.Add(clock);
            UpdateWidgetList();
            UpdateStatus("时钟组件已添加");
        }

        private void AddPomodoroWidget()
        {
            var pomodoro = new PomodoroWidget();
            Canvas.SetLeft(pomodoro, 50 + (_widgets.Count * 20));
            Canvas.SetTop(pomodoro, 150 + (_widgets.Count * 20));
            
            WidgetCanvas.Children.Add(pomodoro);
            _widgets.Add(pomodoro);
            UpdateWidgetList();
            UpdateStatus("番茄时钟组件已添加");
        }

        private void UpdateWidgetList()
        {
            WidgetListBox.Items.Clear();
            for (int i = 0; i < _widgets.Count; i++)
            {
                string widgetType = _widgets[i] switch
                {
                    ClockWidget => "时钟",
                    PomodoroWidget => "番茄时钟",
                    _ => "未知组件"
                };
                WidgetListBox.Items.Add($"{widgetType} #{i + 1}");
            }
        }

        private void GlobalOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (var widget in _widgets)
            {
                if (widget is ClockWidget clock)
                {
                    clock.UpdateOpacity(e.NewValue);
                }
                else if (widget is PomodoroWidget pomodoro)
                {
                    pomodoro.UpdateOpacity(e.NewValue);
                }
            }
        }

        private void HideShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow == null || !_overlayWindow.IsVisible)
            {
                ShowOverlayWindow();
                HideShowButton.Content = "显示主窗口";
                Hide();
            }
            else
            {
                HideOverlayWindow();
                HideShowButton.Content = "隐藏窗口";
                Show();
            }
        }

        private void ShowOverlayWindow()
        {
            _overlayWindow = new OverlayWindow();
            
            // 复制所有组件到覆盖窗口
            foreach (var widget in _widgets)
            {
                UserControl clonedWidget;
                double x = Canvas.GetLeft(widget);
                double y = Canvas.GetTop(widget);

                if (widget is ClockWidget clockWidget)
                {
                    clonedWidget = new ClockWidget { Model = clockWidget.Model };
                }
                else if (widget is PomodoroWidget pomodoroWidget)
                {
                    clonedWidget = new PomodoroWidget { Model = pomodoroWidget.Model };
                }
                else
                {
                    continue;
                }

                clonedWidget.Opacity = widget.Opacity;
                Canvas.SetLeft(clonedWidget, x);
                Canvas.SetTop(clonedWidget, y);
                _overlayWindow.AddWidget(clonedWidget);
            }

            _overlayWindow.Show();
        }

        private void HideOverlayWindow()
        {
            _overlayWindow?.Close();
            _overlayWindow = null;
        }

        private void PreviewLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow == null || !_overlayWindow.IsVisible)
            {
                ShowOverlayWindow();
                UpdateStatus("预览模式已启动");
            }
            else
            {
                HideOverlayWindow();
                UpdateStatus("预览模式已关闭");
            }
        }

        private void SaveLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "保存布局",
                Filter = "布局文件|*.json",
                DefaultExt = "json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveLayout(saveFileDialog.FileName);
            }
        }

        private void LoadLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "加载布局",
                Filter = "布局文件|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadLayout(openFileDialog.FileName);
            }
        }

        private void SaveLayout(string filePath)
        {
            try
            {
                var layout = new LayoutData
                {
                    Widgets = _widgets.Select((widget, index) => new WidgetData
                    {
                        Type = widget.GetType().Name,
                        X = Canvas.GetLeft(widget),
                        Y = Canvas.GetTop(widget),
                        Opacity = widget.Opacity,
                        Index = index
                    }).ToList()
                };

                string json = JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                UpdateStatus("布局已保存");
            }
            catch (Exception ex)
            {
                UpdateStatus($"保存布局失败: {ex.Message}");
            }
        }

        private void LoadLayout(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var layout = JsonSerializer.Deserialize<LayoutData>(json);
                
                if (layout?.Widgets != null)
                {
                    ClearAllWidgets();
                    
                    foreach (var widgetData in layout.Widgets)
                    {
                        UserControl? widget = widgetData.Type switch
                        {
                            nameof(ClockWidget) => new ClockWidget(),
                            nameof(PomodoroWidget) => new PomodoroWidget(),
                            _ => null
                        };

                        if (widget != null)
                        {
                            Canvas.SetLeft(widget, widgetData.X);
                            Canvas.SetTop(widget, widgetData.Y);
                            widget.Opacity = widgetData.Opacity;
                            
                            WidgetCanvas.Children.Add(widget);
                            _widgets.Add(widget);
                        }
                    }
                    
                    UpdateWidgetList();
                    UpdateStatus("布局已加载");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"加载布局失败: {ex.Message}");
            }
        }

        private void ClearAllWidgets()
        {
            WidgetCanvas.Children.Clear();
            _widgets.Clear();
            UpdateWidgetList();
        }

        private void AddClockMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddClockWidget();
        }

        private void AddPomodoroMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddPomodoroWidget();
        }

        private void ClearAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要清除所有组件吗？", "确认", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                ClearAllWidgets();
                UpdateStatus("所有组件已清除");
            }
        }

        private void WidgetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 这里可以添加选中组件时的高亮显示等功能
        }

        protected override void OnClosed(EventArgs e)
        {
            _memoryMonitorTimer?.Stop();
            HideOverlayWindow();
            SaveSettings();
            base.OnClosed(e);
        }
    }

    public class LayoutData
    {
        public List<WidgetData> Widgets { get; set; } = new();
    }

    public class WidgetData
    {
        public string Type { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Opacity { get; set; } = 1.0;
        public int Index { get; set; }
    }
}