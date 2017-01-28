using bilibili.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Data;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

/*-----------------------------------
     键值说明：Notes/SettingKeys
------------------------------------*/
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
        bool isWordEditting = false;
        public Setting()
        {
            InitializeComponent();
            init();
        }

        void init()
        {
            var type = SettingHelper.Devicetype;
            if (type == DeviceType.PC)
            {
                m_top.Visibility = Visibility.Collapsed;
            }
            else if (type == DeviceType.Mobile)
            {
                pc_fullscreen.Visibility = Visibility.Collapsed;
                pc_cursor.Visibility = Visibility.Collapsed;
            }            
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
            else
            {
                quality.SelectedIndex = 1;
            }
            if (SettingHelper.ContainsKey("_pull"))
            {
                background.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_pull"));
            }
            else
            {
                background.IsOn = true;
            }          
            if (SettingHelper.ContainsKey("_toast"))
            {
                toast.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_toast"));
            }
            else
            {
                toast.IsOn = true;
            }
            if (toast.IsOn)
            {
                if (SettingHelper.ContainsKey("_toast_m"))
                {
                    t_message.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_toast_m"));
                }
                else
                {
                    t_message.IsOn = true;
                }
                if (SettingHelper.ContainsKey("_toast_b"))
                {
                    t_bangumi.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_toast_b"));
                }
                else
                {
                    t_bangumi.IsOn = true;
                }
            }
            if (SettingHelper.ContainsKey("_autokill"))
            {
                autokill.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_autokill"));
            }
            else
            {
                autokill.IsOn = true;
            }
            if (SettingHelper.ContainsKey("_isdanmaku"))
            {
                danmakuinit.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_isdanmaku"));
            }
            if (SettingHelper.ContainsKey("_Theme"))
            {
                switch (SettingHelper.GetValue("_Theme").ToString())
                {
                    case "Pink":
                        cb_Theme.SelectedIndex = 0;
                        break;
                    case "Red":
                        cb_Theme.SelectedIndex = 1;
                        break;
                    case "Yellow":
                        cb_Theme.SelectedIndex = 2;
                        break;
                    case "Green":
                        cb_Theme.SelectedIndex = 3;
                        break;
                    case "Blue":
                        cb_Theme.SelectedIndex = 4;
                        break;
                    case "Purple":
                        cb_Theme.SelectedIndex = 5;
                        break;
                    case "Orange":
                        cb_Theme.SelectedIndex = 6;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                cb_Theme.SelectedIndex = 0;
            }
            if (SettingHelper.ContainsKey("_words"))
            {
                string[] words = SettingHelper.GetValue("_words").ToString().Split(' ');
                foreach (string word in words) 
                {
                    if (word.Length > 0) list_kill.Items.Add(word);
                }
            }
            if (SettingHelper.ContainsKey("_path"))
            {
                if (!string.IsNullOrEmpty(SettingHelper.GetValue("_path").ToString()))
                {
                    path.Toggled -= path_Toggled;
                    path.IsOn = true;
                    txt_path.Text = SettingHelper.GetValue("_path").ToString();
                    path.Toggled += path_Toggled;
                }
                else
                {
                    path.IsOn = false;
                    txt_path.Text = @"视频库\哔哩哔哩";
                }
            }
            else
            {
                path.IsOn = false;
                txt_path.Text = @"视频库\哔哩哔哩";
            }
            if (SettingHelper.ContainsKey("_space"))
            {
                sli_space.Value = int.Parse(SettingHelper.GetValue("_space").ToString());
            }
            if (SettingHelper.ContainsKey("_speed"))
            {
                sli_speed.Value = int.Parse(SettingHelper.GetValue("_speed").ToString());
            }
            else
            {
                sli_space.Value = 8;
            }
            if (SettingHelper.ContainsKey("_fontsize"))
            {
                sli_fontsize.Value = int.Parse(SettingHelper.GetValue("_fontsize").ToString());
            }
            else
            {
                sli_fontsize.Value = 1;
            }
            cb_font.Items.Add("默认");
            cb_font.Items.Add("宋体");
            cb_font.Items.Add("等线");
            cb_font.Items.Add("楷体");
            if (SettingHelper.GetDeviceType() == DeviceType.PC)
            {
                cb_font.Items.Add("幼圆");
            }
            if (SettingHelper.ContainsKey("_danmufont"))
            {
                cb_font.SelectedIndex = int.Parse(SettingHelper.GetValue("_danmufont").ToString());
            }
            else
            {
                cb_font.SelectedIndex = 0;
            }
            if(SettingHelper.ContainsKey("_videoformat"))
            {
                cb_format.SelectedIndex = int.Parse(SettingHelper.GetValue("_videoformat").ToString());
            }
            else
            {
                cb_format.SelectedIndex = 0;
            }
            if (SettingHelper.ContainsKey("_backtaskcost"))
            {
                backtaskcost.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_backtaskcost"));
            }
            //根据平台分化的设置项
            if (type == DeviceType.PC)
            {
                if (SettingHelper.ContainsKey("_cursor"))
                {
                    pc_cursor.IsOn = (bool)(SettingHelper.GetValue("_cursor"));
                }
                else
                {
                    pc_cursor.IsOn = true;
                }
                if (SettingHelper.ContainsKey("_autofull"))
                {
                    pc_fullscreen.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_autofull"));
                }
                else
                {
                    pc_fullscreen.IsOn = true;
                }
            }
            else if (type == DeviceType.Mobile)
            {
                if (SettingHelper.ContainsKey("_topbar"))
                {
                    m_top.IsOn = Convert.ToBoolean(SettingHelper.GetValue("_topbar"));
                }
                else
                {
                    m_top.IsOn = true;
                }
            }
        }

        private void night_Toggled(object sender, RoutedEventArgs e)
        {
            ChangeDark?.Invoke(night.IsOn);
        }

        private void sli_space_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("_space", sli_space.Value);
        }        

        private void sli_speed_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("_speed", sli_speed.Value);
        }

        private async void top_Toggled(object sender, RoutedEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar sb = StatusBar.GetForCurrentView();
                SettingHelper.SetValue("_topbar", m_top.IsOn);
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
                    case 6:
                        SettingHelper.SetValue("_Theme", "Orange");
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
            var b = await ApplicationData.Current.TemporaryFolder.GetBasicPropertiesAsync();
            size = (int)(b.Size / 1024);
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
            SettingHelper.SetValue("_tile", background.IsOn);
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string font = string.Empty;
            font = cb_font.SelectedItem.ToString();
            if (danmaku != null)
            {
                SettingHelper.SetValue("_danmufont", cb_font.SelectedIndex);
            }
        }

        private void quality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.SetValue("_quality", (quality.SelectedItem as ComboBoxItem).Tag.ToString());
        }

        private void sli_fontsize_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("_fontsize", (int)sli_fontsize.Value);
        }

        private void toast_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_toast", toast.IsOn);
        }

        private void addword_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.ContainsKey("_words"))
            {
                SettingHelper.SetValue("_words", string.Empty);
            }
            string oldstring = SettingHelper.GetValue("_words").ToString();
            string word = txt_word.Text;
            if (!string.IsNullOrEmpty(word) && !oldstring.Contains(word)) 
            {
                oldstring += word + " ";
                list_kill.Items.Add(word);
                SettingHelper.SetValue("_words", oldstring);
                txt_word.Text = string.Empty;
            }
        }

        private void delword_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;
            if (!isWordEditting)
            {
                btn.Content = "退出删除";
                list_kill.Visibility = Visibility.Visible;
                isWordEditting = true;
            }
            else
            {
                btn.Content = "管理";
                list_kill.Visibility = Visibility.Collapsed;
                isWordEditting = false;
            }
        }

        private void list_kill_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!isWordEditting) return;
            if (!SettingHelper.ContainsKey("_words"))
            {
                SettingHelper.SetValue("_words", string.Empty);
            }
            string oldstring = SettingHelper.GetValue("_words").ToString();
            string newstring = oldstring.Replace(e.ClickedItem.ToString() + ",", string.Empty);
            SettingHelper.SetValue("_words", newstring);
            list_kill.Items.Remove(e.ClickedItem);
        }

        private void fullscreen_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_autofull", pc_fullscreen.IsOn);
        }

        private void autokill_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_autokill", autokill.IsOn);
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:DaweiX@outlook.com"));
        }

        private async void path_Toggled(object sender, RoutedEventArgs e)
        {
            if (path.IsOn == false)
            {
                SettingHelper.SetValue("_path", string.Empty);
                txt_path.Text = @"视频库\哔哩哔哩";
                return;
            }
            StorageFolder folder;
            FolderPicker picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".flv");
            folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                string path = folder.Path;
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                SettingHelper.SetValue("_path", path);
                txt_path.Text = path;
            }
            else
            {
                path.IsOn = false;
            }
        }

        private void t_bangumi_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_toast_b", t_bangumi.IsOn);
        }

        private void t_message_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_toast_m", t_message.IsOn);
        }

        private async void regex_Click(object sender, RoutedEventArgs e)
        {
            await new Dialogs.RegexTip().ShowAsync();
        }

        private void ComboBoxItem_Tapped_1(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            SettingHelper.SetValue("_videoformat", item.Tag.ToString());
        }

        private void cursor_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_cursor", pc_cursor.IsOn);
        }

        private void danmakuinit_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_isdanmaku", danmakuinit.IsOn);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = sender as ListView;
            switch (list.SelectedIndex)
            {
                case 0: Frame.Navigate(typeof(Test.Test)); break;
                case 1: Frame.Navigate(typeof(Test.ListAnimation)); break;
            }
        }

        private void backtaskcost_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_backtaskcost", backtaskcost.IsOn);
        }
    }
    public class BoolToVisibility : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}