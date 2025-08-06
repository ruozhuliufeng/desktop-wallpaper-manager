using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace DesktopWallpaperApp.Utils
{
    public static class PerformanceOptimizer
    {
        private static readonly DispatcherTimer _memoryCleanupTimer;
        
        static PerformanceOptimizer()
        {
            _memoryCleanupTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5) // 每5分钟清理一次
            };
            _memoryCleanupTimer.Tick += MemoryCleanupTimer_Tick;
        }

        public static void StartOptimization()
        {
            // 设置进程优先级为低，避免影响其他应用
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            
            // 启动内存清理定时器
            _memoryCleanupTimer.Start();
            
            // 初始清理
            ForceGarbageCollection();
        }

        public static void StopOptimization()
        {
            _memoryCleanupTimer.Stop();
        }

        private static void MemoryCleanupTimer_Tick(object? sender, EventArgs e)
        {
            ForceGarbageCollection();
        }

        public static void ForceGarbageCollection()
        {
            try
            {
                // 强制垃圾收集
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // 尝试释放未使用的内存给操作系统
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"内存清理失败: {ex.Message}");
            }
        }

        public static long GetMemoryUsage()
        {
            return GC.GetTotalMemory(false);
        }

        public static string GetFormattedMemoryUsage()
        {
            long bytes = GetMemoryUsage();
            return FormatBytes(bytes);
        }

        private static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = { "B", "KB", "MB", "GB" };
            
            long max = (long)Math.Pow(scale, orders.Length - 1);
            
            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);
                max /= scale;
            }
            return "0 B";
        }

        // 限制进程工作集大小
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        // 检查是否需要内存清理
        public static bool ShouldCleanMemory()
        {
            long currentMemory = GetMemoryUsage();
            const long threshold = 50 * 1024 * 1024; // 50MB 阈值
            return currentMemory > threshold;
        }

        // 低内存模式配置
        public static void EnableLowMemoryMode(bool enable)
        {
            if (enable)
            {
                // 更频繁的清理
                _memoryCleanupTimer.Interval = TimeSpan.FromMinutes(2);
                
                // 设置更低的进程优先级
                try
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
                }
                catch
                {
                    // 如果无法设置，静默处理
                }
            }
            else
            {
                _memoryCleanupTimer.Interval = TimeSpan.FromMinutes(5);
                
                try
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                }
                catch
                {
                    // 如果无法设置，静默处理
                }
            }
        }
    }
}