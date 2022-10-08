using System;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using KeyFrameAnimation;
using System.Windows.Forms;
using Path = System.IO.Path;

namespace WpfAnimation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //先停止播放动画
            TestAnimImage.IsShow = false;
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    System.Windows.MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
                var path = dialog.SelectedPath;
                var imageCache = TestAnimImage.ImageCache;
                if (imageCache != null)
                {
                    AnimationCache.Instance.AddKeyFrames(path, Helper.GetKeyFrames(path));
                    ImageBehavior.SetSourceKey(imageCache, path);
                    TestAnimImage.IsShow = true;
                }
            }
        }

        private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        {
            //先停止播放动画
            TestAnimImage.IsShow = false;
            OpenFileDialog dialog = new OpenFileDialog();
            //该值确定是否可以选择多个文件,当前不然多选
            dialog.Multiselect = false;
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = dialog.FileName;
                var imageCache = TestAnimImage.ImageCache;
                if (imageCache != null)
                {
                    AnimationCache.Instance.AddKeyFrames(path, Helper.GetWebpOrGif(path));
                    ImageBehavior.SetSourceKey(imageCache, path);
                    TestAnimImage.IsShow = true;
                }
            }
        }

        private static string _packageCacheRelativePath = @"colkwallpaper\Cache\ImageAnimationPlugin\Resources\";
        private void ButtonBase_OnClick3(object sender, RoutedEventArgs e)
        {
            //先停止播放动画
            TestAnimImage.IsShow = false;
            OpenFileDialog dialog = new OpenFileDialog();
            //该值确定是否可以选择多个文件,当前不然多选
            dialog.Multiselect = false;
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = dialog.FileName;
                var replace = path.Substring(path.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
                var safeFileName = replace.Substring(0, replace.LastIndexOf(".", StringComparison.Ordinal));
                //var safeFileName = Helper.GetStringMd5(path);
                //string resPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"temp\" + safeFileName);
                var md5 = Helper.GetStringMd5(path);
                var resPath = Path.Combine(Path.GetTempPath(), _packageCacheRelativePath);
                //resPath = Path.Combine(resPath, md5);

                //先判断文件是否存在
                if (!Directory.Exists(resPath) ||
                    ((Directory.GetFiles(resPath).Length <= 0) && (Directory.GetDirectories(resPath).Length <= 0)))
                {
                    var result = Helper.UnCompressFile(resPath, path);
                    if(!result)
                        return;
                }

                var imageCache = TestAnimImage.ImageCache;
                if (imageCache != null)
                {
                    var newPath = Path.Combine(resPath, safeFileName);
                    AnimationCache.Instance.AddKeyFrames(newPath, Helper.GetKeyFrames(newPath));
                    ImageBehavior.SetSourceKey(imageCache, newPath);
                    TestAnimImage.IsShow = true;
                }
            }

        }
    }
}
