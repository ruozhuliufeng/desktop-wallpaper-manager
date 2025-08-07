using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DesktopWallpaperApp.Models;

namespace DesktopWallpaperApp.Controls
{
    public partial class ClockWidget : UserControl, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;
        private bool _isDragging;
        private Point _dragStartPoint;
        private ClockModel _model;

        public ClockModel Model
        {
            get => _model;
            set
            {
                _model = value;
                DataContext = this;
                OnPropertyChanged();
            }
        }

        public string CurrentTime => DateTime.Now.ToString(_model?.TimeFormat ?? "HH:mm:ss");
        public string CurrentDate => DateTime.Now.ToString(_model?.DateFormat ?? "yyyy-MM-dd");
        public bool ShowDate => _model?.ShowDate ?? true;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? PositionChanged;

        public ClockWidget()
        {
            InitializeComponent();
            
            _model = new ClockModel();
            DataContext = this;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            Cursor = Cursors.Hand;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentTime));
            OnPropertyChanged(nameof(CurrentDate));
        }

        private void ClockWidget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowSettings();
                return;
            }

            _isDragging = true;
            _dragStartPoint = e.GetPosition(null);
            CaptureMouse();
            e.Handled = true;
        }

        private void ClockWidget_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && IsMouseCaptured)
            {
                Point currentPosition = e.GetPosition(null);
                double deltaX = currentPosition.X - _dragStartPoint.X;
                double deltaY = currentPosition.Y - _dragStartPoint.Y;

                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaX);
                    Canvas.SetTop(this, Canvas.GetTop(this) + deltaY);

                    // 更新模型位置
                    _model.X = Canvas.GetLeft(this);
                    _model.Y = Canvas.GetTop(this);
                }

                _dragStartPoint = currentPosition;
                e.Handled = true;
            }
        }

        private void ClockWidget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                e.Handled = true;
                
                // 通知位置变化
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ShowSettings()
        {
            var settingsWindow = new Views.WidgetSettingsWindow(this, Model)
            {
                Owner = Application.Current.MainWindow
            };
            
            if (settingsWindow.ShowDialog() == true)
            {
                // 设置已保存，触发属性更改通知
                OnPropertyChanged(nameof(Model));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            
            // 确保组件不会移出屏幕边界
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                double maxX = parentWindow.ActualWidth - ActualWidth;
                double maxY = parentWindow.ActualHeight - ActualHeight;

                double currentX = Canvas.GetLeft(this);
                double currentY = Canvas.GetTop(this);

                if (currentX > maxX) Canvas.SetLeft(this, maxX);
                if (currentY > maxY) Canvas.SetTop(this, maxY);
                if (currentX < 0) Canvas.SetLeft(this, 0);
                if (currentY < 0) Canvas.SetTop(this, 0);
            }
        }

        public void UpdateOpacity(double opacity)
        {
            Opacity = Math.Max(0.1, Math.Min(1.0, opacity));
            _model.Opacity = Opacity;
        }
    }
}