using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopWallpaperApp.Models
{
    public class WidgetModel : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private double _opacity = 1.0;
        private bool _isVisible = true;
        private string _id = Guid.NewGuid().ToString();

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, Math.Max(0.1, Math.Min(1.0, value)));
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class ClockModel : WidgetModel
    {
        private string _timeFormat = "HH:mm:ss";
        private string _dateFormat = "yyyy-MM-dd";
        private bool _showDate = true;

        public string TimeFormat
        {
            get => _timeFormat;
            set => SetProperty(ref _timeFormat, value);
        }

        public string DateFormat
        {
            get => _dateFormat;
            set => SetProperty(ref _dateFormat, value);
        }

        public bool ShowDate
        {
            get => _showDate;
            set => SetProperty(ref _showDate, value);
        }
    }
}