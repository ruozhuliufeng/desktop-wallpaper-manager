using System;
using System.ComponentModel;

namespace DesktopWallpaperApp.Models
{
    public enum PomodoroState
    {
        Stopped,
        Working,
        ShortBreak,
        LongBreak,
        Paused
    }

    public class PomodoroModel : WidgetModel
    {
        private int _workMinutes = 25;
        private int _shortBreakMinutes = 5;
        private int _longBreakMinutes = 15;
        private int _sessionsBeforeLongBreak = 4;
        private int _currentSession = 0;
        private PomodoroState _state = PomodoroState.Stopped;
        private TimeSpan _remainingTime = TimeSpan.FromMinutes(25);
        private bool _soundEnabled = true;

        public PomodoroModel()
        {
            Name = "番茄时钟组件";
        }

        public int WorkMinutes
        {
            get => _workMinutes;
            set => SetProperty(ref _workMinutes, Math.Max(1, Math.Min(60, value)));
        }

        public int ShortBreakMinutes
        {
            get => _shortBreakMinutes;
            set => SetProperty(ref _shortBreakMinutes, Math.Max(1, Math.Min(30, value)));
        }

        public int LongBreakMinutes
        {
            get => _longBreakMinutes;
            set => SetProperty(ref _longBreakMinutes, Math.Max(1, Math.Min(60, value)));
        }

        public int SessionsBeforeLongBreak
        {
            get => _sessionsBeforeLongBreak;
            set => SetProperty(ref _sessionsBeforeLongBreak, Math.Max(1, Math.Min(10, value)));
        }

        public int CurrentSession
        {
            get => _currentSession;
            set => SetProperty(ref _currentSession, value);
        }

        public PomodoroState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set => SetProperty(ref _remainingTime, value);
        }

        public bool SoundEnabled
        {
            get => _soundEnabled;
            set => SetProperty(ref _soundEnabled, value);
        }

        public string StateText
        {
            get
            {
                return _state switch
                {
                    PomodoroState.Stopped => "准备开始",
                    PomodoroState.Working => "工作中",
                    PomodoroState.ShortBreak => "短休息",
                    PomodoroState.LongBreak => "长休息",
                    PomodoroState.Paused => "已暂停",
                    _ => "未知状态"
                };
            }
        }

        public string RemainingTimeText => $"{_remainingTime.Minutes:D2}:{_remainingTime.Seconds:D2}";
    }
}