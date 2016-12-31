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
using Windows.ApplicationModel.Background;
using System.Diagnostics;
using bilibili.UI;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace bilibili
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //public delegate void ShowStatus(string message);
        //public event ShowStatus Showstatus;
        DispatcherTimer timer = new DispatcherTimer();
        bool currentTheme;
        bool isExit = false;
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            //后退键
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            MainList.SelectedIndex = 0;
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
                    txt.Text = "日间模式";
                }
                else if (SettingHelper.GetValue("_nighttheme").ToString() == "light")
                {
                    RequestedTheme = ElementTheme.Light;
                    currentTheme = true;
                    txt.Text = "夜间模式";
                }
            }
            else
            {
                RequestedTheme = ElementTheme.Light;
                currentTheme = true;
                txt.Text = "夜间模式";
            }
            ChangeTheme();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (WebStatusHelper.IsOnline())
            {
                //处理跳转参数
                string arg = e.Parameter.ToString();
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
                        mainframe.Navigate(typeof(Views.Message), arg.Substring(1));
                    }
                }
                else
                {
                    mainframe.Navigate(typeof(Views.Partition));
                }
                bool isLogin = await autologin();
                if (isLogin)
                {                    
                    if (SettingHelper.GetValue("_pull") != null)
                    {
                        if ((bool)SettingHelper.GetValue("_pull") == true)
                        {
                            await RegisterBackgroundTask(typeof(BackgroundTask.TileTask), "TileTask", new TimeTrigger(15, false), null);
                        }
                    }
                }
            }
            else
            {
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += Timer_Tick;
            }
        }

        private async Task RegisterBackgroundTask(Type EntryPoint, string name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.DeniedByUser)
            {
                return;
            }
            foreach (var item in BackgroundTaskRegistration.AllTasks)
            {
                if (item.Value.Name == name) 
                {
                    item.Value.Unregister(true);
                }
            }
            var builder = new BackgroundTaskBuilder { Name = name, TaskEntryPoint = EntryPoint.FullName, IsNetworkRequested = false };
            builder.SetTrigger(trigger);
            if (condition != null)
            {
                builder.AddCondition(condition);
            }
            builder.Register();
            BackgroundTaskRegistration a = builder.Register();
            Debug.WriteLine("---------Register---------");
            a.Progress += A_Progress;
            a.Completed += A_Completed;
            return;
        }

        private void A_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("---------Completed!---------");
        }

        private void A_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            Debug.WriteLine("---------Processing...---------");
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (WebStatusHelper.IsOnline())
            {
                if (await autologin() == true)
                {
                    timer.Stop();
                }
            }
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
        /// <summary>
        /// 自动登录
        /// </summary>
        async Task<bool> autologin()
        {
            try
            {
                if (!SettingHelper.ContainsKey("_accesskey"))
                {
                    SettingHelper.SetValue("_accesskey", string.Empty);
                }
                if (SettingHelper.ContainsKey("_autologin")) 
                {
                    if (bool.Parse(SettingHelper.GetValue("_autologin").ToString()) == true)
                    {
                        string p = string.Empty;
                        string u = string.Empty;
                        if (!string.IsNullOrEmpty(SettingHelper.GetValue("_epassword").ToString()))
                        {
                            p = SettingHelper.GetValue("_epassword").ToString();
                        }
                        if (!string.IsNullOrEmpty(SettingHelper.GetValue("_username").ToString()))
                        {
                            u = SettingHelper.GetValue("_username").ToString();
                        }
                        await ApiHelper.login(p, u, false);
                    }
                }
                if (ApiHelper.IsLogin()) 
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
                        messagepop.Show("再次点击后退键退出", 2000);
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
                    if (!ApiHelper.IsLogin())
                    {
                        (mainframe.Content as Views.Detail).trylogin += Detail_trylogin;
                    }
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

        private async void Detail_trylogin()
        {
            if (await autologin() == true)
            {
                timer.Stop();
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
                            bool isDark = ChangeDarkMode(currentTheme);
                        }
                        break;
                }
                Sets.SelectedIndex = -1;
            }
        }


        private bool ChangeDarkMode(bool value)
        {
            if (value)
            {
                RequestedTheme = ElementTheme.Dark;
                currentTheme = false;
                SettingHelper.SetValue("_nighttheme", "dark");
                txt.Text = "日间模式";
                return true;
            }
            else 
            {
                RequestedTheme = ElementTheme.Light;
                currentTheme = true;
                SettingHelper.SetValue("_nighttheme", "light");
                txt.Text = "夜间模式";
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
            //if (DisplayInformation.AutoRotationPreferences == DisplayOrientations.Landscape || DisplayInformation.AutoRotationPreferences == DisplayOrientations.LandscapeFlipped) 
            //{
            //    back.Visibility = Visibility.Visible;
            //}
        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.ItemsSource = null;
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
    }
}
