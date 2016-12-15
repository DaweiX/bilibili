using bilibili.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Notifications;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Setting : Page
    {
        public delegate bool SettingHandler(bool value);
        public event SettingHandler ChangeDark;
        public event SettingHandler ChangeTheme;
        public Setting()
        {
            InitializeComponent();
            init();
        }

        void init()
        {
            if (SettingHelper.ContainsKey("_nighttheme"))
            {
                night.IsOn = SettingHelper.GetValue("_nighttheme").ToString() == "light" ? false : true;
            }
            if (SettingHelper.ContainsKey("_downloadcost"))
                downstyle.IsOn = SettingHelper.GetValue("_downloadcost").ToString() == "wifionly" ? false : true;
            if (SettingHelper.ContainsKey("_downdanmu"))
            {
                danmaku.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_downdanmu"));
            }
            if (SettingHelper.ContainsKey("_isdirect"))
            {
                direct.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_isdirect"));
            }
            if (SettingHelper.ContainsKey("_quality"))
            {
                quality.SelectedIndex = Convert.ToInt32(SettingHelper.GetValue("_quality").ToString()) - 1;
            }
            if (SettingHelper.ContainsKey("_pull"))
            {
                background.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_pull"));
            }
        }

        private void night_Toggled(object sender, RoutedEventArgs e)
        {
            ChangeDark?.Invoke(night.IsOn);
        }

        private void uri_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void sli_space_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("_space", sli_space.Value);
        }        

        private void sli_speed_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("_speed", sli_speed.Value);
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private async void top_Toggled(object sender, RoutedEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar sb = StatusBar.GetForCurrentView();
                if (!string.IsNullOrEmpty(top.IsOn.ToString()))
                {           
                    SettingHelper.SetValue("_topbar", top.IsOn);
                    if ((bool)SettingHelper.GetValue("_topbar") == false) 
                    {
                        await sb.ShowAsync();
                        sb.BackgroundColor = Color.FromArgb(1, 226, 115, 170);
                        sb.BackgroundOpacity = 1;
                    }
                    else if ((bool)SettingHelper.GetValue("_topbar") == true)
                    {
                        await sb.HideAsync();
                    }
                }
            }
        }

        private void cb_Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Theme.SelectedItem != null)
            {
                switch (cb_Theme.SelectedIndex)
                {
                    case 0:
                        SettingHelper.SetValue("_Theme", "Pink");
                        break;
                    case 1:
                        SettingHelper.SetValue("_Theme", "Red");
                        break;
                    case 2:
                        SettingHelper.SetValue("_Theme", "Yellow");
                        break;
                    case 3:
                        SettingHelper.SetValue("_Theme", "Green");
                        break;
                    case 4:
                        SettingHelper.SetValue("_Theme", "Blue");
                        break;
                    case 5:
                        SettingHelper.SetValue("_Theme", "Purple");
                        break;
                    default:
                        break;
                }
                ChangeTheme?.Invoke(true);
            }
        }

        async void topShowOrHide()
        {
            StatusBar sb = StatusBar.GetForCurrentView();
            if (SettingHelper.GetValue("_topbar").ToString() == "False")
            {
                await sb.ShowAsync();
                sb.BackgroundColor = Color.FromArgb(1, 226, 115, 170);
                sb.BackgroundOpacity = 1;
            }
            else if (SettingHelper.GetValue("_topbar").ToString() == "True")
            {
                await sb.HideAsync();
            }
        }


        private async void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            int size = 0;
            var a = await ApplicationData.Current.LocalCacheFolder.GetBasicPropertiesAsync();
            var b = await ApplicationData.Current.TemporaryFolder.GetBasicPropertiesAsync();
            size = (int)(a.Size / 1024)+ (int)(b.Size / 1024);
            cache.Text = "清理缓存" + "[" + size.ToString() + "MB" + "]";
        }

        private void downstyle_Toggled(object sender, RoutedEventArgs e)
        {
            if (downstyle.IsOn == false) 
                SettingHelper.SetValue("_downloadcost", "wifionly");
            else if (downstyle.IsOn == true)
                SettingHelper.SetValue("_downloadcost", "wifidata");
        }

        private void danmaku_Toggled(object sender, RoutedEventArgs e)
        {
            if (danmaku.IsOn == true)
                SettingHelper.SetValue("_downdanmu", true);
            else if (danmaku.IsOn == false)
                SettingHelper.SetValue("_downdanmu", false);
        }

        private void background_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_pull", background.IsOn);
            if (background.IsOn == false)
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
            }
        }

        private void direct_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_isdirect", direct.IsOn);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            TextBlock txt = this.FindName(string.Format("h{0}", pivot.SelectedIndex)) as TextBlock;
            for (int i = 0; i < pivot.Items.Count; i++)
            {
                TextBlock temp = this.FindName(string.Format("h{0}", i)) as TextBlock;
                temp.Foreground = new SolidColorBrush(Colors.LightGray);
            }
            txt.Foreground = new SolidColorBrush(Colors.White);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void quality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.SetValue("_quality", (quality.SelectedItem as ComboBoxItem).Tag.ToString());
        }

        private void sli_fontsize_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("_fontsize", (int)sli_fontsize.Value);
        }
    }
}