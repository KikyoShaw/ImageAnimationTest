using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using KeyFrameAnimation;

namespace WpfAnimation
{
    static class Helper
    {
        public static List<KeyFrame> GetKeyFrames(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var duration = 50; // 每一帧间隔
            var k = new List<KeyFrame>();
            var list = GetBitmapListFromPath(path);
            foreach (var image in list)
            {
                k.Add(new KeyFrame
                {
                    AFrame = image,
                    Duration = duration
                });
            }
            return k;
        }

        public static List<KeyFrame> GetWebpOrGif(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var duration = 100; // 每一帧间隔
            var k = new List<KeyFrame>();
            var list = ImageLoader.Loader.GetKeyFramesByLocalPath(path);
            foreach (var frame in list)
            {
                k.Add(new KeyFrame
                {
                    AFrame = frame.AFrame,
                    Duration = duration
                });
            }
            return k;
        }

        private static List<BitmapImage> GetBitmapListFromPath(string sPath)
        {
            try
            {
                if (!Directory.Exists(sPath))
                    return null;

                DirectoryInfo cacheDir = new DirectoryInfo(sPath);
                FileInfo[] cacheFiles = cacheDir.GetFiles();
                if (cacheFiles.Length == 0)
                {
                    Directory.Delete(sPath, true);
                    return null;
                }

                List<BitmapImage> vResultList = new List<BitmapImage>();
                foreach (var f in cacheFiles)
                {
                    if (vResultList.Count >= 35)
                        break;

                    string fileType = f.Extension.ToLower();
                    if (fileType != ".png" && fileType != ".bmp" && fileType != ".jpg" &&
                        fileType != ".jpeg" && fileType != ".jpe")
                    {
                        continue;
                    }

                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(f.FullName);
                    bmp.EndInit();
                    bmp.Freeze();
                    vResultList.Add(bmp);
                }

                return vResultList;
            }
            catch (Exception ex)
            {
                //LogHelper.LogError($"DownResources error={ex.Message}");
            }

            return null;
        }

        //获取字符串md5值
        public static string GetStringMd5(string oriString)
        {
            try
            {
                if (oriString.Length == 0)
                {
                    return "";
                }
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] ret = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(oriString));

                StringBuilder sBuilder = new StringBuilder();
                foreach (var t in ret)
                {
                    sBuilder.Append(t.ToString("x2"));
                }
                return sBuilder.ToString();
            }
            catch { }
            return oriString;
        }

        //解压文件
        public static bool UnCompressFile(string sExtractPath, string sZipFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(sExtractPath) || string.IsNullOrEmpty(sZipFilePath))
                    return false;

                if (!File.Exists(sZipFilePath))
                    return false;

                //var sDir = Path.GetDirectoryName(sExtractPath);
                //if (string.IsNullOrEmpty(sDir))
                //    return false;

                if (Directory.Exists(sExtractPath))
                    Directory.Delete(sExtractPath, true);

                if (AppDomain.CurrentDomain.BaseDirectory != null)
                    SevenZip.SevenZipBase.SetLibraryPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.dll"));

                SevenZip.SevenZipExtractor extractor = new SevenZip.SevenZipExtractor(sZipFilePath);
                extractor.ExtractArchive(sExtractPath);
                extractor.Dispose();
            }
            catch {}
            return true;
        }

    }
}
