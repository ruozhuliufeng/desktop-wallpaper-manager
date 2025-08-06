using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace DesktopWallpaperApp.Services
{
    public class WallpaperService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        public static bool SetWallpaper(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                    return false;

                // 设置壁纸
                int result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, 
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                if (result == 0)
                    return false;

                // 更新注册表设置
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                {
                    if (key != null)
                    {
                        key.SetValue("WallpaperStyle", "10"); // 填充
                        key.SetValue("TileWallpaper", "0");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置壁纸失败: {ex.Message}");
                return false;
            }
        }

        public static string GetCurrentWallpaper()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false))
                {
                    if (key != null)
                    {
                        object wallpaper = key.GetValue("Wallpaper");
                        return wallpaper?.ToString() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取当前壁纸失败: {ex.Message}");
            }
            return string.Empty;
        }

        public static bool IsImageFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".jpg" || extension == ".jpeg" || 
                   extension == ".png" || extension == ".bmp" || 
                   extension == ".gif" || extension == ".tiff";
        }
    }
}