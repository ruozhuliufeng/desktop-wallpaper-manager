using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DesktopWallpaperApp
{
    public partial class OverlayWindow : Window
    {
        private readonly List<UserControl> _widgets = new();

        public OverlayWindow()
        {
            InitializeComponent();
            SetupWindow();
        }

        private void SetupWindow()
        {
            // 设置为全屏
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            
            // 设置为透明背景
            Background = System.Windows.Media.Brushes.Transparent;
            AllowsTransparency = true;
            
            // 忽略鼠标事件，让组件可以交互
            IsHitTestVisible = true;
            
            // 设置为桌面覆盖层
            ShowInTaskbar = false;
        }

        public void AddWidget(UserControl widget)
        {
            WidgetCanvas.Children.Add(widget);
            _widgets.Add(widget);
        }

        public void RemoveWidget(UserControl widget)
        {
            WidgetCanvas.Children.Remove(widget);
            _widgets.Remove(widget);
        }

        public void ClearWidgets()
        {
            WidgetCanvas.Children.Clear();
            _widgets.Clear();
        }

        public List<UserControl> GetWidgets()
        {
            return _widgets;
        }
    }
}