using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using bilibili.Helpers;
using Windows.Data.Json;
using bilibili.Methods;
using Windows.UI.Xaml.Media.Imaging;
using bilibili.Http;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using Windows.Storage;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace bilibili
{
   /// <summary>
   /// 可用于自身或导航至 Frame 内部的空白页。
   /// </summary>
    public sealed partial class MainPage : Page
    {
        // public delegate void ShowStatus(string message);
        // public event ShowStatus Showstatus;
        bool currentTheme;
        bool isExit = false;
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            // 后退键
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            // MainList.SelectedIndex = 0;
            if (SettingHelper.ContainsKey("_topbar"))
            {
                TopShoworHide();
            }
            if (SettingHelper.ContainsKey("_nighttheme"))
            {
                if (SettingHelper.GetValue("_nighttheme").ToString() == "dark")
                {
                    RequestedTheme = ElementTheme.Dark;
                    currentTheme = false;
                    // txt.Text = "日间模式";
                }
                else if (SettingHelper.GetValue("_nighttheme").ToString() == "light")
                {
                    RequestedTheme = ElementTheme.Light;
                    currentTheme = true;
                    // txt.Text = "夜间模式";
                }
            }
            else
            {
                RequestedTheme = ElementTheme.Light;
                currentTheme = true;
                // txt.Text = "夜间模式";
            }
            ChangeTheme();
            if (SettingHelper.DeviceType == DeviceType.PC)
            {
                CoreWindow.GetForCurrentThread().KeyDown += MainPage_KeyDown;
            }
        }

        private async void MainPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = true;
            if (args.VirtualKey != Windows.System.VirtualKey.Back && args.VirtualKey != Windows.System.VirtualKey.Escape) return;
            if (FrameManager.FrameHelpers.action())
                return;
            if (mainframe == null)
                return;
            if (mainframe.CanGoBack)
            {
                mainframe.GoBack();
            }
            else
            {
                if (isExit)
                {
                    Application.Current.Exit();
                }
                else
                {
                    isExit = true;
                    await popup.Show("再次点击后退键退出");
                    await Task.Delay(2000);
                    isExit = false;
                }
            }
        }

        private async void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (FrameManager.FrameHelpers.action())
                return;
            if (mainframe == null)
                return;
            if (e.Handled == false)
            {
                if (mainframe.CanGoBack)
                {
                    e.Handled = true;
                    mainframe.GoBack();
                }
                else
                {
                    if (isExit)
                    {
                        Application.Current.Exit();
                    }
                    else
                    {
                        e.Handled = true;
                        isExit = true;
                        await popup.Show("再次点击后退键退出");
                        await Task.Delay(2000);
                        isExit = false;
                    }
                }
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainframe.Navigate(typeof(Views.Partition));
            // 处理跳转参数
            if (e.Parameter is StorageFile)
            {
                mainframe.Navigate(typeof(Views.Video), e.Parameter);
            }
            else
            {
                if (e.Parameter != null)
                {
                    string arg = e.Parameter.ToString();
                    if (WebStatusHelper.IsOnline())
                    {
                        if (!string.IsNullOrEmpty(arg))
                        {
                            if (arg[0] == 't')
                            {
                                mainframe.Navigate(typeof(Views.Detail_P), arg.Substring(1));
                            }
                            else if (arg[0] == 's')
                            {
                                mainframe.Navigate(typeof(Views.Detail), arg.Substring(1));
                            }
                            else if (arg[0] == 'm')
                            {
                                mainframe.Navigate(typeof(Views.Message), arg.Substring(2));
                            }
                        }                      
                    }
                    else
                    {
                        await popup.Show("没有网络连接");
                    }
                }               
            }
            if (ApiHelper.IsLogin())
            {
                await ShowStatus();
            }
        }

        private async Task ShowStatus()
        {
            await popup.Show("登录成功");
            try
            {
                ApiHelper.accesskey = SettingHelper.GetValue("_accesskey").ToString();
                string url = "http://api.bilibili.com/myinfo?appkey=" + ApiHelper.appkey + "&access_key=" + SettingHelper.GetValue("_accesskey").ToString();
                url += ApiHelper.GetSign(url);
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("mid"))
                    UserHelper.Mid = json["mid"].ToString();
                if (json.ContainsKey("face"))
                {
                    string f = json["face"].GetString();
                    UserHelper.Face = f;
                    face.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(f)) };
                }
                if (json.ContainsKey("uname"))
                {
                    UserHelper.Uname = uname.Text = json["uname"].GetString();
                }
                if (json.ContainsKey("level_info"))
                {
                    JsonObject json2 = JsonObject.Parse(json["level_info"].ToString());
                    if (json2.ContainsKey("current_level"))
                    {
                        switch (json2["current_level"].ToString())
                        {
                            case "0": rank.Text = "普通用户"; break;
                            case "1": rank.Text = "注册会员"; break;
                            case "2": rank.Text = "正式会员"; break;
                            case "3": rank.Text = "字幕君"; break;
                            case "4": rank.Text = "VIP用户"; break;
                            case "5": rank.Text = "职人"; break;
                            case "6": rank.Text = "站长大人"; break;
                        }
                    }
                }
            }
            catch { }
        }

        bool ChangeTheme(bool a = true)
        {
            try
            {
                if (SettingHelper.ContainsKey("_Theme"))
                {
                    var TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                    Color color = ColorRelated.GetColor();
                    Application.Current.Resources["bili_Theme"] = new SolidColorBrush(color);
                    ChangeDarkMode(currentTheme);
                    ChangeDarkMode(currentTheme);
                    TitleBar.BackgroundColor = color;
                    TitleBar.ButtonBackgroundColor = color;
                    TitleBar.ForegroundColor = Colors.White;
                    TitleBar.ButtonHoverForegroundColor = Colors.Black;
                    TitleBar.ButtonHoverBackgroundColor = Colors.White;
					if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
					{
						StatusBar sb = StatusBar.GetForCurrentView();
						sb.BackgroundColor = color;
						sb.BackgroundOpacity = 1;
					}
				}
                return true;
            }
			catch { return false; }
        }

        async void TopShoworHide()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar sb = StatusBar.GetForCurrentView();
                if ((bool)SettingHelper.GetValue("_topbar") == false)
                {
                    await sb.ShowAsync();
                    Color color = ColorRelated.GetColor();
                    sb.BackgroundColor = color;
                    sb.BackgroundOpacity = 1;
                }
                else if ((bool)SettingHelper.GetValue("_topbar") == true)
                {
                    await sb.HideAsync();
                }
            }
        }
        // async void topShowOrHide()
        // {
        //     if (_appSettings.Values["_topbar"].ToString() == "False")
        //     {
        //         await sb.ShowAsync();
        //         sb.BackgroundColor = Color.FromArgb(1, 226, 115, 170);
        //     }
        //     else if (_appSettings.Values["_topbar"].ToString() == "True")
        //     {
        //         await sb.HideAsync();
        //     }
        // }
        // StatusBar sb = StatusBar.GetForCurrentView();

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (WebStatusHelper.IsOnline())
                mainframe.Navigate(typeof(Views.Search), SearchBox.Text);
        }

        private void mainframe_Navigated(object sender, NavigationEventArgs e)
        {
            // bilibili.Views.PartViews.Bangumi, bilibili, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
            // Sets.SelectedIndex = -1;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = mainframe.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            if (mainframe.CurrentSourcePageType == typeof(Views.Video))
            {
                grid_top.Visibility = Visibility.Collapsed;
                ham.DisplayMode = SplitViewDisplayMode.Overlay;
            }
            else
            {
                grid_top.Visibility = Visibility.Visible;
                if (this.ActualWidth >= 800)
                    ham.DisplayMode = SplitViewDisplayMode.CompactInline;
            }
            switch (mainframe.CurrentSourcePageType.AssemblyQualifiedName.Split(',')[0].Split('.')[2])
            {
                case "Setting":
                    txt_head.Text = "设置";
                    (mainframe.Content as Views.Setting).ChangeDark += ChangeDarkMode;
                    (mainframe.Content as Views.Setting).ChangeTheme += ChangeTheme;
                    break;
                case "Detail":
                    txt_head.Text = "番剧详情";
                    // if (!ApiHelper.IsLogin())
                    // {
                    //     (mainframe.Content as Views.Detail).trylogin += Detail_trylogin;
                    // }
                    break;
                case "Detail_P":
                    (mainframe.Content as Views.Detail_P).pageNavi += DetailPNavi;
                    break;
                case "login":
                    txt_head.Text = "用户登录";
                    (mainframe.Content as Views.login).reportLogin += reportlogin;
                    break;
                case "Message":
                    txt_head.Text = "我的信息";
                    break;
                case "Partition":
                    txt_head.Text = "首页";
                    break;
                case "MyWeb":
                    txt_head.Text = "浏览网页";
                    break;
                case "Search":
                    txt_head.Text = "搜索结果";
                    break;
                case "Topic":
                    txt_head.Text = "话题浏览";
                    break;
                case "Download":
                    txt_head.Text = "离线管理";
                    break;
                case "FavCollection":
                    txt_head.Text = "我的收藏";
                    break;
                case "Bangumi":
                    txt_head.Text = "番剧分类";
                    break;
                case "Timeline":
                    txt_head.Text = "放送表";
                    break;
                case "MyConcerns":
                    txt_head.Text = "订阅番剧";
                    break;
                case "History":
                    txt_head.Text = "播放历史";
                    break;
                case "UserInfo":
                    txt_head.Text = "个人中心";
                    break;
                case "RankPage":
                    txt_head.Text = "排行榜";
                    break;
            }
        }

        private async void reportlogin()
        {
            await ShowStatus();
        }

        private void DetailPNavi(string text)
        {
            txt_head.Text = text;
        }

        private void face_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {           
            if (ham.DisplayMode == SplitViewDisplayMode.Overlay)
                ham.IsPaneOpen = false;
            if (ApiHelper.IsLogin())
            {
                mainframe.Navigate(typeof(Views.UserInfo), null, new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
            }
            else
            {
                mainframe.Navigate(typeof(Views.login), null, new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
            }
        }

        private bool ChangeDarkMode(bool value)
        {
            if (value)
            {
                RequestedTheme = ElementTheme.Dark;
                currentTheme = false;
                SettingHelper.SetValue("_nighttheme", "dark");
                // txt.Text = "日间模式";
                return true;
            }
            else 
            {
                RequestedTheme = ElementTheme.Light;
                currentTheme = true;
                SettingHelper.SetValue("_nighttheme", "light");
                // txt.Text = "夜间模式";
                return false;
            }
        }

        private void Border_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            ham.IsPaneOpen = !ham.IsPaneOpen;
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            if (mainframe.CanGoBack)
            {
                mainframe.GoBack();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // if (DisplayInformation.AutoRotationPreferences == DisplayOrientations.Landscape || DisplayInformation.AutoRotationPreferences == DisplayOrientations.LandscapeFlipped) 
            // {
            //     back.Visibility = Visibility.Visible;
            // }
        }
        
        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.ItemsSource = null;
                return;
            }
            if (!WebStatusHelper.IsOnline())
            {
                return;
            }
            string url = "http://api.bilibili.com/suggest?_device=wp&appkey=" + ApiHelper.appkey + "&bangumi_acc_num=0&bangumi_num=0&build=429001&func=suggest&main_ver=v3&mobi_app=win&special_acc_num=0&special_num=0&suggest_type=accurate&term=" + SearchBox.Text + "&topic_acc_num=0&topic_num=0&upuser_acc_num=0&upuser_num=0&_hwid=0100d4c50200c2a6&platform=uwp_mobile";
            url += ApiHelper.GetSign(url);
            JsonObject json = await BaseService.GetJson(url);
            try
            {
                if (json.ContainsKey("tag"))
                {
                    List<string> list = new List<string>();
                    json = JsonObject.Parse(json["tag"].ToString());
                    int i = 0;
                    do
                    {
                        i++;
                    } while (json.ContainsKey(i.ToString()));
                    for (int j = 0; j < i; j++)
                    {
                        JsonObject json2 = JsonObject.Parse(json[j.ToString()].ToString());
                        if (json2.ContainsKey("value"))
                        {
                            list.Add(json2["value"].GetString());
                        }
                    }
                    SearchBox.ItemsSource = list;
                }
            }
            catch { }
        }

        public List<PaneItem> PaneListItems => new List<PaneItem>
        {
            new PaneItem { Title = "精彩推荐", Glyph = "ms-appx:///Assets//Icons//tv.png" , Index = 0 },
            new PaneItem { Title = "播放历史", Glyph = "ms-appx:///Assets//Icons//clock.png" , Index = 1, IsLoginNeeded = true  },
            new PaneItem { Title = "我的消息", Glyph = "ms-appx:///Assets//Icons//message.png" , Index = 2, IsLoginNeeded = true  },
            new PaneItem { Title = "我的收藏", Glyph = "ms-appx:///Assets//Icons//star.png" , Index = 3, IsLoginNeeded = true },
            new PaneItem { Title = "离线管理", Glyph = "ms-appx:///Assets//Icons//download.png" , Index = 4 }
        };
        // xE20a xE2ad xE119 xE208 xE118
        private async void MainList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ham.DisplayMode == SplitViewDisplayMode.Overlay) ham.IsPaneOpen = false;
            var item = e.ClickedItem as PaneItem;
            Type frame = null;
            bool islog = false;
            if (item != null)
            {
                islog = ApiHelper.IsLogin();
                switch (item.Index)
                {
                    case 0: frame = typeof(Views.Partition); break;
                    case 1: frame = typeof(Views.History); break;
                    case 2: frame = typeof(Views.Message); break;
                    case 3: frame = typeof(Views.FavCollection); break;
                    case 4: frame = typeof(Views.Download); break;
                    // case "night":
                    //     {
                    //         // fonticon.Glyph = "&#xE708/6;";
                    //         bool isDark = ChangeDarkMode(currentTheme);
                    //     }
                    //     break;
                }
                // Sets.SelectedIndex = -1;
            }
            if (item.IsLoginNeeded && islog == false)
            {
                await popup.Show("请先登录");
                return;
            }
            if (frame != null)
            {
                mainframe.Navigate(frame);
            }
        }

        private void set_Click(object sender, RoutedEventArgs e)
        {
            if (ham.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                ham.IsPaneOpen = false;
            }
            mainframe.Navigate(typeof(Views.Setting));
        }

        private void night_Click(object sender, RoutedEventArgs e)
        {
            bool isDark = ChangeDarkMode(currentTheme);
        }
    }
}
