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
            
            // 订阅位置变化事件
            clock.PositionChanged += (s, e) => SyncWidgetToOverlay(clock);
            
            WidgetCanvas.Children.Add(clock);
            _widgets.Add(clock);
            UpdateWidgetList();
            UpdateStatus("时钟组件已添加");
        }

        private void AddPomodoroWidget()
        {
            var pomodoro = new PomodoroWidget();
            Canvas.SetLeft(pomodoro, 200);
            Canvas.SetTop(pomodoro, 150 + (_widgets.Count * 20));
            
            // 订阅位置变化事件
            pomodoro.PositionChanged += (s, e) => SyncWidgetToOverlay(pomodoro);
            
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
                    ClockWidget clock => clock.Model?.Name ?? "时钟",
                    PomodoroWidget pomodoro => pomodoro.Model?.Name ?? "番茄时钟",
                    _ => "未知组件"
                };
                WidgetListBox.Items.Add($"{widgetType} #{i + 1}");
            }
        }

        public void RefreshWidgetList()
        {
            UpdateWidgetList();
        }

        private void GlobalOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WidgetListBox.SelectedItem is UserControl selectedWidget)
            {
                selectedWidget.Opacity = e.NewValue;
                UpdateStatus($"组件透明度已设置为 {e.NewValue:F2}");
                
                // 同步到桌面覆盖层
                SyncWidgetToOverlay(selectedWidget);
            }
        }

        private void SyncWidgetToOverlay(UserControl sourceWidget)
        {
            if (_overlayWindow == null || !_overlayWindow.IsVisible) return;

            // 找到对应的覆盖层组件并同步属性
            var overlayWidgets = _overlayWindow.GetWidgets();
            int sourceIndex = _widgets.IndexOf(sourceWidget);
            
            if (sourceIndex >= 0 && sourceIndex < overlayWidgets.Count)
            {
                var targetWidget = overlayWidgets[sourceIndex];
                SyncWidgetProperties(sourceWidget, targetWidget);
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
            
            // 复制所有组件到覆盖窗口并建立同步关系
            foreach (var widget in _widgets)
            {
                UserControl clonedWidget;
                double x = Canvas.GetLeft(widget);
                double y = Canvas.GetTop(widget);

                if (widget is ClockWidget clockWidget)
                {
                    var overlayClockWidget = new ClockWidget { Model = clockWidget.Model };
                    clonedWidget = overlayClockWidget;
                    
                    // 建立属性同步
                    SetupWidgetSync(clockWidget, overlayClockWidget);
                }
                else if (widget is PomodoroWidget pomodoroWidget)
                {
                    var overlayPomodoroWidget = new PomodoroWidget { Model = pomodoroWidget.Model };
                    clonedWidget = overlayPomodoroWidget;
                    
                    // 建立属性同步
                    SetupWidgetSync(pomodoroWidget, overlayPomodoroWidget);
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

        private void SetupWidgetSync(UserControl sourceWidget, UserControl targetWidget)
        {
            // 监听源组件的属性变化
            if (sourceWidget is ClockWidget sourceClockWidget && targetWidget is ClockWidget targetClockWidget)
            {
                sourceClockWidget.Model.PropertyChanged += (s, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SyncWidgetProperties(sourceClockWidget, targetClockWidget);
                    });
                };
            }
            else if (sourceWidget is PomodoroWidget sourcePomodoroWidget && targetWidget is PomodoroWidget targetPomodoroWidget)
            {
                sourcePomodoroWidget.Model.PropertyChanged += (s, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SyncWidgetProperties(sourcePomodoroWidget, targetPomodoroWidget);
                    });
                };
            }
        }

        private void SyncWidgetProperties(UserControl sourceWidget, UserControl targetWidget)
        {
            if (_overlayWindow == null || !_overlayWindow.IsVisible) return;

            // 同步位置
            double sourceX = Canvas.GetLeft(sourceWidget);
            double sourceY = Canvas.GetTop(sourceWidget);
            Canvas.SetLeft(targetWidget, sourceX);
            Canvas.SetTop(targetWidget, sourceY);

            // 同步透明度
            targetWidget.Opacity = sourceWidget.Opacity;

            // 同步模型数据（已经通过共享Model实现）
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
                            
                            // 订阅位置变化事件
                            if (widget is ClockWidget clockWidget)
                            {
                                clockWidget.PositionChanged += (s, e) => SyncWidgetToOverlay(clockWidget);
                            }
                            else if (widget is PomodoroWidget pomodoroWidget)
                            {
                                pomodoroWidget.PositionChanged += (s, e) => SyncWidgetToOverlay(pomodoroWidget);
                            }
                            
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
            if (WidgetListBox.SelectedIndex >= 0 && WidgetListBox.SelectedIndex < _widgets.Count)
            {
                var selectedWidget = _widgets[WidgetListBox.SelectedIndex];
                GlobalOpacitySlider.Value = selectedWidget.Opacity;
                
                // 高亮显示选中的组件
                foreach (var widget in _widgets)
                {
                    widget.BorderBrush = widget == selectedWidget ? 
                        System.Windows.Media.Brushes.Red : 
                        System.Windows.Media.Brushes.Transparent;
                    widget.BorderThickness = widget == selectedWidget ? 
                        new Thickness(2) : 
                        new Thickness(0);
                }
            }
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