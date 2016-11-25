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
using Windows.Graphics.Display;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace bilibili
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timer = new DispatcherTimer();
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            //标题栏
            var TitleBar = ApplicationView.GetForCurrentView().TitleBar;
            TitleBar.BackgroundColor = Color.FromArgb(1, 226, 115, 170);
            TitleBar.ForegroundColor = Colors.White;
            TitleBar.ButtonBackgroundColor = Color.FromArgb(1, 226, 115, 170);
            TitleBar.ButtonHoverForegroundColor = Colors.Black;
            TitleBar.ButtonHoverBackgroundColor = Colors.White;
            //后退键
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            MainList.SelectedIndex = 0;
            mainframe.Navigate(typeof(Views.Partition), null, new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
            if (SettingHelper.ContainsKey("_topbar"))
            {
                TopShoworHide();
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (WebStatusHelper.IsOnline())
            {
                await autologin();
            }
            else
            {
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += Timer_Tick;
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (await autologin() == true)
            {
                timer.Stop();
            }
        }

        bool ChangeTheme(bool a=true)
        {
            string ThemeName = string.Empty;
            if (SettingHelper.ContainsKey("_Theme"))
            {
                ThemeName = SettingHelper.GetValue("_Theme") as string;
            }
            else
            {
                ThemeName = "Pink";
                SettingHelper.SetValue("_Theme", "Pink");
            }
            //Application.Current.Resources.ThemeDictionaries.Clear();
            //KeyValuePair<object, object> openWith = new KeyValuePair<object, object>("bili_Theme", new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)));
            //Application.Current.Resources.ThemeDictionaries.Add(new ResourceDictionary { });
            //找不到来自“ms-appx:///Theme//RedTheme.xaml”的资源
            ResourceDictionary newDictionary = new ResourceDictionary();
            newDictionary.Source = new Uri("ms-appx:///Theme/RedTheme.xaml", UriKind.RelativeOrAbsolute);
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newDictionary);
            return true;
        }

        async void TopShoworHide()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar sb = StatusBar.GetForCurrentView();
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
        /// <summary>
        /// 自动登录
        /// </summary>
        async Task<bool> autologin()
        {
            try
            {
                if (!SettingHelper.ContainsKey("_accesskey"))
                    SettingHelper.SetValue("_accesskey", "");
                if (SettingHelper.ContainsKey("_autologin") && ApiHelper.IsLogin() == false) 
                {
                    if ((bool)SettingHelper.GetValue("_autologin") == true)
                    {
                        await ApiHelper.login("","");
                    }
                }
                if (SettingHelper.GetValue("_accesskey").ToString().Length > 2)
                {
                    ApiHelper.accesskey = SettingHelper.GetValue("_accesskey").ToString();
                    string url = "http://api.bilibili.com/myinfo?appkey=422fd9d7289a1dd9&access_key=" + SettingHelper.GetValue("_accesskey").ToString();
                    url += ApiHelper.GetSign(url);
                    JsonObject json = await BaseService.GetJson(url);
                    if (json.ContainsKey("mid"))
                        UserHelper.mid = json["mid"].ToString();
                    if (json.ContainsKey("face"))
                        face.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(StringDeal.delQuotationmarks((json["face"].ToString())))) };
                    if (json.ContainsKey("uname"))
                        uname.Text = StringDeal.delQuotationmarks(json["uname"].ToString());
                    if (json.ContainsKey("level_info"))
                    {
                        JsonObject json2 = JsonObject.Parse(json["level_info"].ToString());
                        if (json2.ContainsKey("current_level"))
                        {
                            switch(json2["current_level"].ToString())
                            {
                                case "0": rank.Text = "普通用户"; break;
                                case "1": rank.Text = "注册会员";break;
                                case "2": rank.Text = "正式会员"; break;
                                case "3": rank.Text = "字幕君"; break;
                                case "4": rank.Text = "VIP用户"; break;
                                case "5": rank.Text = "职人"; break;
                                case "6": rank.Text = "站长大人"; break;
                            }
                        }
                    }
                }
                return true;
            }

            catch 
            {
                return false;
            }
        }

        //async void topShowOrHide()
        //{
        //    if (_appSettings.Values["_topbar"].ToString() == "False")
        //    {
        //        await sb.ShowAsync();
        //        sb.BackgroundColor = Color.FromArgb(1, 226, 115, 170);
        //    }
        //    else if (_appSettings.Values["_topbar"].ToString() == "True")
        //    {
        //        await sb.HideAsync();
        //    }
        //}
        //StatusBar sb = StatusBar.GetForCurrentView();
        //后退键
        private async void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            bool isExit = false;
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
                        messagepop.Show("再次电击后退键退出", 2000);
                        await Task.Delay(2000);
                        isExit = false;
                    }
                }
            }
        }

        private void Sets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ham.DisplayMode == SplitViewDisplayMode.Overlay)
                ham.IsPaneOpen = false;
            MainList.SelectedIndex = -1;
            mainframe.Navigate(typeof(Views.Setting));
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (WebStatusHelper.IsOnline())
                mainframe.Navigate(typeof(Views.Search), SearchBox.Text, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void mainframe_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            //bilibili.Views.PartViews.Bangumi, bilibili, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
            Sets.SelectedIndex = -1;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = mainframe.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            stk.Background = (mainframe.CurrentSourcePageType == typeof(Views.Partition))|| mainframe.CurrentSourcePageType == typeof(Views.Friends) || mainframe.CurrentSourcePageType == typeof(Views.Setting) || mainframe.CurrentSourcePageType.AssemblyQualifiedName.Split(',')[0].Split('.')[2] == "PartViews" || mainframe.CurrentSourcePageType == typeof(Views.Message) ? new SolidColorBrush(Color.FromArgb(255, 226, 115, 169)) : (this.RequestedTheme == ElementTheme.Dark ? new SolidColorBrush(Color.FromArgb(255, 43, 43, 43)) : new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)));
            uname.Foreground = (mainframe.CurrentSourcePageType == typeof(Views.Partition))|| mainframe.CurrentSourcePageType == typeof(Views.Friends) || mainframe.CurrentSourcePageType == typeof(Views.Setting) || mainframe.CurrentSourcePageType.AssemblyQualifiedName.Split(',')[0].Split('.')[2] == "PartViews" || mainframe.CurrentSourcePageType == typeof(Views.Message) ? new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) : new SolidColorBrush(Color.FromArgb(255, 226, 115, 169));
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
                    txt_head.Text = "";
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
            }
        }

        private async void reportlogin()
        {
            await autologin();
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

        private void MainList_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ham.DisplayMode == SplitViewDisplayMode.Overlay)
                ham.IsPaneOpen = false;          
            var lbi = MainList.SelectedItem as ListBoxItem;
            if (lbi != null)
            {
                switch (lbi.Tag.ToString())
                {
                    case "index":
                        {
                            mainframe.Navigate(typeof(Views.Partition));
                        }
                        break;              
                    case "class":
                        {
                            mainframe.Navigate(typeof(Views.Bangumi));
                        }
                        break;
                    case "message":
                        {
                            if (ApiHelper.IsLogin())
                            {
                                mainframe.Navigate(typeof(Views.Message));
                            }
                            else
                            {
                                messagepop.Show("请先登录");
                            }
                        }
                        break;
                    case "history":
                        {
                            if (ApiHelper.IsLogin())
                            {
                                mainframe.Navigate(typeof(Views.History));
                            }
                            else
                            {
                                messagepop.Show("请先登录");
                            }
                        }
                        break;
                    case "me":
                        {
                            if (ApiHelper.IsLogin())
                            {
                                mainframe.Navigate(typeof(Views.FavCollection));
                            }
                            else
                            {
                                messagepop.Show("请先登录");
                            }
                        }
                        break;
                    case "download":
                        {
                            mainframe.Navigate(typeof(Views.Download));
                        }
                        break;
                    case "night":
                        {
                            //fonticon.Glyph = "&#xE708/6;";
                            bool currentTheme = this.RequestedTheme == ElementTheme.Light ? true : false;
                            bool isDark = ChangeDarkMode(currentTheme);
                            txt.Text = isDark ? "日间模式" : "夜间模式";
                        }
                        break;
                }
                Sets.SelectedIndex = -1;
            }
        }


        private bool ChangeDarkMode(bool value = true)
        {
            if (!value)
            {
                this.RequestedTheme = ElementTheme.Light;
                SettingHelper.SetValue("_nighttheme", false);
                stk.Background = (mainframe.CurrentSourcePageType == typeof(Views.Partition)) || mainframe.CurrentSourcePageType == typeof(Views.Friends) || mainframe.CurrentSourcePageType.AssemblyQualifiedName.Split(',')[0].Split('.')[2] == "PartViews" || mainframe.CurrentSourcePageType == typeof(Views.Message) ? new SolidColorBrush(Color.FromArgb(255, 226, 115, 169)) : (this.RequestedTheme == ElementTheme.Dark ? new SolidColorBrush(Color.FromArgb(255, 43, 43, 43)) : new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)));
                return false;
            }
            else
            {
                this.RequestedTheme = ElementTheme.Dark;
                SettingHelper.SetValue("_nighttheme", true);
                stk.Background = (mainframe.CurrentSourcePageType == typeof(Views.Partition)) || mainframe.CurrentSourcePageType == typeof(Views.Friends) || mainframe.CurrentSourcePageType.AssemblyQualifiedName.Split(',')[0].Split('.')[2] == "PartViews" || mainframe.CurrentSourcePageType == typeof(Views.Message) ? new SolidColorBrush(Color.FromArgb(255, 226, 115, 169)) : (this.RequestedTheme == ElementTheme.Dark ? new SolidColorBrush(Color.FromArgb(255, 43, 43, 43)) : new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)));
                return true;
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
            //if (DisplayInformation.AutoRotationPreferences == DisplayOrientations.Landscape || DisplayInformation.AutoRotationPreferences == DisplayOrientations.LandscapeFlipped) 
            //{
            //    back.Visibility = Visibility.Visible;
            //}
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            
        }
    }
}
