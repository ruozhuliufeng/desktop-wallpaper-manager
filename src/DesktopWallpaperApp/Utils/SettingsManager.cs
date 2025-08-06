using System;
using System.IO;
using System.Text.Json;
using DesktopWallpaperApp.Models;

namespace DesktopWallpaperApp.Utils
{
    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DesktopWallpaperApp",
            "settings.json"
        );

        static SettingsManager()
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
            }

            return new AppSettings();
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
            }
        }
    }

    public class AppSettings
    {
        public double WindowWidth { get; set; } = 800;
        public double WindowHeight { get; set; } = 600;
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
        public double GlobalOpacity { get; set; } = 1.0;
        public bool StartMinimized { get; set; } = false;
        public bool AutoStartWithWindows { get; set; } = false;
        public string LastWallpaper { get; set; } = string.Empty;
        public string LastLayoutPath { get; set; } = string.Empty;
        public bool EnableLowMemoryMode { get; set; } = true;
    }
}