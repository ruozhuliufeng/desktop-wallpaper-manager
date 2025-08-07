using System;
using System.Windows;
using System.Windows.Controls;
using DesktopWallpaperApp.Controls;
using DesktopWallpaperApp.Models;
using static DesktopWallpaperApp.Models.PomodoroState;

namespace DesktopWallpaperApp.Views
{
    public partial class WidgetSettingsWindow : Window
    {
        private readonly UserControl _widget;
        private readonly WidgetModel _widgetModel;
        private bool _isApplied = false;

        public bool IsApplied => _isApplied;

        public WidgetSettingsWindow(UserControl widget, WidgetModel model)
        {
            InitializeComponent();
            _widget = widget;
            _widgetModel = model;
            
            LoadSettings();
            SetupWidgetSpecificSettings();
        }

        private void LoadSettings()
        {
            // 加载基本设置
            NameTextBox.Text = _widgetModel.Name;
            OpacitySlider.Value = _widgetModel.Opacity;
            XPositionTextBox.Text = _widgetModel.X.ToString("F0");
            YPositionTextBox.Text = _widgetModel.Y.ToString("F0");
            
            // 更新透明度显示
            UpdateOpacityDisplay();
        }

        private void SetupWidgetSpecificSettings()
        {
            switch (_widget)
            {
                case ClockWidget clockWidget:
                    TitleTextBlock.Text = "时钟组件设置";
                    ClockSettingsGroup.Visibility = Visibility.Visible;
                    
                    // 加载时钟特定设置
                    ShowDateCheckBox.IsChecked = clockWidget.Model.ShowDate;
                    FontSizeSlider.Value = 24; // 默认字体大小
                    UpdateFontSizeDisplay();
                    break;
                    
                case PomodoroWidget pomodoroWidget:
                    TitleTextBlock.Text = "番茄时钟组件设置";
                    PomodoroSettingsGroup.Visibility = Visibility.Visible;
                    
                    // 加载番茄时钟特定设置
                    WorkDurationTextBox.Text = pomodoroWidget.Model.WorkMinutes.ToString();
                    ShortBreakTextBox.Text = pomodoroWidget.Model.ShortBreakMinutes.ToString();
                    LongBreakTextBox.Text = pomodoroWidget.Model.LongBreakMinutes.ToString();
                    AutoStartBreakCheckBox.IsChecked = false; // 默认值，因为模型中没有此属性
                    PlaySoundCheckBox.IsChecked = pomodoroWidget.Model.SoundEnabled;
                    break;
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateOpacityDisplay();
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateFontSizeDisplay();
        }

        private void UpdateOpacityDisplay()
        {
            if (OpacityValueText != null)
            {
                OpacityValueText.Text = $"{(int)(OpacitySlider.Value * 100)}%";
            }
        }

        private void UpdateFontSizeDisplay()
        {
            if (FontSizeValueText != null)
            {
                FontSizeValueText.Text = ((int)FontSizeSlider.Value).ToString();
            }
        }

        private void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取屏幕中心位置
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            
            var centerX = (screenWidth - _widget.ActualWidth) / 2;
            var centerY = (screenHeight - _widget.ActualHeight) / 2;
            
            XPositionTextBox.Text = centerX.ToString("F0");
            YPositionTextBox.Text = centerY.ToString("F0");
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            _isApplied = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            _isApplied = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ApplySettings()
        {
            try
            {
                // 应用基本设置
                _widgetModel.Name = NameTextBox.Text;
                _widgetModel.Opacity = OpacitySlider.Value;
                
                if (double.TryParse(XPositionTextBox.Text, out double x))
                {
                    _widgetModel.X = x;
                    Canvas.SetLeft(_widget, x);
                }
                
                if (double.TryParse(YPositionTextBox.Text, out double y))
                {
                    _widgetModel.Y = y;
                    Canvas.SetTop(_widget, y);
                }
                
                _widget.Opacity = _widgetModel.Opacity;

                // 应用组件特定设置
                switch (_widget)
                {
                    case ClockWidget clockWidget:
                        clockWidget.Model.ShowDate = ShowDateCheckBox.IsChecked ?? false;
                        
                        // 应用字体大小（这里需要在ClockWidget中添加相应的属性）
                        // clockWidget.SetFontSize((int)FontSizeSlider.Value);
                        break;
                        
                    case PomodoroWidget pomodoroWidget:
                        if (int.TryParse(WorkDurationTextBox.Text, out int workDuration))
                            pomodoroWidget.Model.WorkMinutes = workDuration;
                            
                        if (int.TryParse(ShortBreakTextBox.Text, out int shortBreak))
                            pomodoroWidget.Model.ShortBreakMinutes = shortBreak;
                            
                        if (int.TryParse(LongBreakTextBox.Text, out int longBreak))
                            pomodoroWidget.Model.LongBreakMinutes = longBreak;
                            
                        // AutoStartBreak功能暂未实现
                        pomodoroWidget.Model.SoundEnabled = PlaySoundCheckBox.IsChecked ?? true;
                        
                        // 如果番茄时钟正在运行，需要重置时间
                        if (pomodoroWidget.Model.State == PomodoroState.Working)
                        {
                            pomodoroWidget.Model.RemainingTime = TimeSpan.FromMinutes(pomodoroWidget.Model.WorkMinutes);
                        }
                        break;
                }
                
                // 通知主窗口更新组件列表
                var mainWindow = Owner as MainWindow;
                mainWindow?.RefreshWidgetList();
                
                MessageBox.Show("设置已应用", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用设置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}