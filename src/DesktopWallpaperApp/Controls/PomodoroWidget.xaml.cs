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
    public partial class PomodoroWidget : UserControl, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;
        private bool _isDragging;
        private Point _dragStartPoint;
        private PomodoroModel _model;

        public PomodoroModel Model
        {
            get => _model;
            set
            {
                _model = value;
                DataContext = this;
                OnPropertyChanged();
            }
        }

        public string StateText => _model?.StateText ?? "准备开始";
        public string RemainingTimeText => _model?.RemainingTimeText ?? "25:00";
        public string SessionText => $"第 {_model?.CurrentSession ?? 0} 个番茄";
        
        public string StartPauseButtonText
        {
            get
            {
                return _model?.State switch
                {
                    PomodoroState.Stopped => "开始",
                    PomodoroState.Paused => "继续",
                    _ => "暂停"
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? PositionChanged;

        public PomodoroWidget()
        {
            InitializeComponent();
            
            _model = new PomodoroModel();
            DataContext = this;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            Cursor = Cursors.Hand;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_model.State == PomodoroState.Stopped || _model.State == PomodoroState.Paused)
                return;

            if (_model.RemainingTime.TotalSeconds <= 1)
            {
                CompleteCurrentSession();
                return;
            }

            _model.RemainingTime = _model.RemainingTime.Subtract(TimeSpan.FromSeconds(1));
            OnPropertyChanged(nameof(RemainingTimeText));
        }

        private void CompleteCurrentSession()
        {
            if (_model.SoundEnabled)
            {
                PlayNotificationSound();
            }

            switch (_model.State)
            {
                case PomodoroState.Working:
                    _model.CurrentSession++;
                    OnPropertyChanged(nameof(SessionText));

                    if (_model.CurrentSession % _model.SessionsBeforeLongBreak == 0)
                    {
                        StartLongBreak();
                    }
                    else
                    {
                        StartShortBreak();
                    }
                    break;

                case PomodoroState.ShortBreak:
                case PomodoroState.LongBreak:
                    StartWork();
                    break;
            }
        }

        private void StartWork()
        {
            _model.State = PomodoroState.Working;
            _model.RemainingTime = TimeSpan.FromMinutes(_model.WorkMinutes);
            OnPropertyChanged(nameof(StateText));
            OnPropertyChanged(nameof(RemainingTimeText));
            OnPropertyChanged(nameof(StartPauseButtonText));
        }

        private void StartShortBreak()
        {
            _model.State = PomodoroState.ShortBreak;
            _model.RemainingTime = TimeSpan.FromMinutes(_model.ShortBreakMinutes);
            OnPropertyChanged(nameof(StateText));
            OnPropertyChanged(nameof(RemainingTimeText));
            OnPropertyChanged(nameof(StartPauseButtonText));
        }

        private void StartLongBreak()
        {
            _model.State = PomodoroState.LongBreak;
            _model.RemainingTime = TimeSpan.FromMinutes(_model.LongBreakMinutes);
            OnPropertyChanged(nameof(StateText));
            OnPropertyChanged(nameof(RemainingTimeText));
            OnPropertyChanged(nameof(StartPauseButtonText));
        }

        private void PlayNotificationSound()
        {
            try
            {
                System.Media.SystemSounds.Beep.Play();
            }
            catch
            {
                // 静默处理音效播放失败
            }
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_model.State)
            {
                case PomodoroState.Stopped:
                    StartWork();
                    _timer.Start();
                    break;

                case PomodoroState.Paused:
                    _model.State = PomodoroState.Working;
                    _timer.Start();
                    OnPropertyChanged(nameof(StateText));
                    OnPropertyChanged(nameof(StartPauseButtonText));
                    break;

                default:
                    _model.State = PomodoroState.Paused;
                    _timer.Stop();
                    OnPropertyChanged(nameof(StateText));
                    OnPropertyChanged(nameof(StartPauseButtonText));
                    break;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _model.State = PomodoroState.Stopped;
            _model.RemainingTime = TimeSpan.FromMinutes(_model.WorkMinutes);
            OnPropertyChanged(nameof(StateText));
            OnPropertyChanged(nameof(RemainingTimeText));
            OnPropertyChanged(nameof(StartPauseButtonText));
        }

        private void PomodoroWidget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void PomodoroWidget_MouseMove(object sender, MouseEventArgs e)
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

                    _model.X = Canvas.GetLeft(this);
                    _model.Y = Canvas.GetTop(this);
                }

                _dragStartPoint = currentPosition;
                e.Handled = true;
            }
        }

        private void PomodoroWidget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                OnPropertyChanged(nameof(StateText));
                OnPropertyChanged(nameof(RemainingTimeText));
                OnPropertyChanged(nameof(SessionText));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateOpacity(double opacity)
        {
            Opacity = Math.Max(0.1, Math.Min(1.0, opacity));
            _model.Opacity = Opacity;
        }
    }
}