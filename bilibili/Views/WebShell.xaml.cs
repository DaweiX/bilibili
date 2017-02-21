using bilibili.FrameManager;
using bilibili.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebShell : Page
    {

        public WebShell()
        {
            this.InitializeComponent();
        }

        private void Lframe_Loaded(object sender, RoutedEventArgs e)
        {
            FrameHelpers.FrameManager.LeftFrame = (Frame)sender;
            FrameHelpers.FrameManager.LeftFrame.Navigate(typeof(Topic));
        }

        private void Rframe_Loaded(object sender, RoutedEventArgs e)
        {
            FrameHelpers.FrameManager.RightFrame = (Frame)sender;
            FrameHelpers.FrameManager.RightFrame.Navigate(typeof(PlaceHolderPage));
        }

        private void VisualStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (SettingHelper.DeviceType == DeviceType.PC)
            {
                //之前有Right而之后没有，补充动画
                if (e.OldState != Left && e.NewState == Left)
                {
                    var animation = new PopInThemeAnimation
                    {
                        FromHorizontalOffset = 0,
                        FromVerticalOffset = 100,
                        SpeedRatio = 0.5
                    };
                    Storyboard.SetTarget(animation, Lframe);
                    Storyboard storyboard = new Storyboard();
                    storyboard.Children.Add(animation);
                    storyboard.Begin();
                }
            }
        }
    }
}
