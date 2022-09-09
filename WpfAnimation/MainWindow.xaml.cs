using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KeyFrameAnimation;
using System.Windows.Forms;

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
    }
}
