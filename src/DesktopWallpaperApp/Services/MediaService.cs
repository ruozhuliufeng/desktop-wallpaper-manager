using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DesktopWallpaperApp.Services
{
    public class MediaService
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
        private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".mkv" };

        public static bool IsImageFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return ImageExtensions.Contains(extension);
        }

        public static bool IsVideoFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return VideoExtensions.Contains(extension);
        }

        public static List<string> GetMediaFiles(string directory)
        {
            var mediaFiles = new List<string>();

            if (!Directory.Exists(directory))
                return mediaFiles;

            try
            {
                var allExtensions = ImageExtensions.Concat(VideoExtensions);
                var files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => allExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));

                mediaFiles.AddRange(files);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取媒体文件失败: {ex.Message}");
            }

            return mediaFiles;
        }

        public static string ExtractVideoFrame(string videoPath, string outputPath, TimeSpan timeOffset)
        {
            // 这里可以使用FFmpeg或其他库来提取视频帧
            // 为简化起见，现在返回一个占位符
            try
            {
                // 实际实现中可以使用FFMpegCore等库
                // 现在只是创建一个占位符文件
                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"提取视频帧失败: {ex.Message}");
                return string.Empty;
            }
        }
    }
}