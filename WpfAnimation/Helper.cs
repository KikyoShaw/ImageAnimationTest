using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using KeyFrameAnimation;

namespace WpfAnimation
{
    internal class SchedulerBase : TaskScheduler
    {
        private readonly BlockingCollection<Task> m_queue = new BlockingCollection<Task>();

        public SchedulerBase(int iThreads, ThreadPriority priority)
        {
            for (int i = 0; i < iThreads; ++i)
            {
                Thread th = new Thread(DoRun);
                th.IsBackground = true;
                th.Priority = priority;
                th.Start();
            }
        }

        private void DoRun()
        {
            try
            {
                Task t;
                while (m_queue.TryTake(out t, Timeout.Infinite))
                {
                    try
                    {
                        if (t != null)
                        {
                            TryExecuteTask(t);//在当前线程执行Task
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return m_queue;
        }

        protected override void QueueTask(Task task)
        {
            m_queue?.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }
    }

    static class Helper
    {
        public static List<KeyFrame> GetKeyFrames(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var duration = 100; // 每一帧间隔
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

        // TODO 暂时先用单线程处理下载，多线程如果重复下载同一个资源的时候同步文件占用状态有点麻烦
        private static readonly Lazy<SchedulerBase> LazyDownTask = new Lazy<SchedulerBase>(() => new SchedulerBase(1, ThreadPriority.Normal));

        public static async Task<byte[]> GetSourceData(string url, string localPath)
        {
            return await Task<byte[]>.Factory.StartNew(() =>
                {
                    // 单线程下载，如果需要多线程需要缓存一下url做判断防止重复下载导致资源竞争
                    try
                    {
                        if (File.Exists(localPath))
                        {
                            var readFs = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                            var bytes = new byte[readFs.Length];
                            readFs.Read(bytes, 0, bytes.Length);
                            readFs.Dispose();
                            return bytes;
                        }

                        // 不存在的话就下载
                        var req = (HttpWebRequest)WebRequest.Create(url);
                        req.Timeout = 5000;
                        req.Proxy = null!;
                        using var webResponse = (HttpWebResponse)req.GetResponse();
                        using var ms = new MemoryStream();
                        var netStream = webResponse.GetResponseStream();

                        if (netStream == null) return null;

                        netStream.CopyTo(ms);
                        netStream.Close();

                        var writeFs = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.Write);
                        var byteArray = ms.ToArray();
                        writeFs.Write(byteArray, 0, byteArray.Length);
                        writeFs.Dispose();

                        return byteArray;
                    }
                    catch /*(Exception e)*/
                    {
                        //LogHelper.LogError($"webp error msg = {e.Message}");
                    }
                    return null;
                }, CancellationToken.None, TaskCreationOptions.None,
                LazyDownTask.Value);
        }
    }
}
