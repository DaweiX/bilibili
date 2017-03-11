using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using bilibili.Http;
using bilibili.Models;
using bilibili.Helpers;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media.Animation;
using Windows.Data.Json;
using bilibili.Methods;

//  “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
   /// <summary>
   /// 可用于自身或导航至 Frame 内部的空白页。
   /// </summary>
    public sealed partial class Partition : Page
    {
        static bool isTopicLoaded = false;
        static bool isFriendsLoaded = false;
        string cursor = "-1";
        public Partition()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            comment.Navi += Comment_Navi;
            comment.Info += Comment_Info;
            // comment.live += Comment_live;
            ContentServ.report += Report;
        }

        private async void Report(string status)
        {
            await popup.Show(status);
        }

        private void Comment_live(string tid)
        {
            Frame.Navigate(typeof(Live), tid);
        }

        private void Comment_Info(string aid)
        {
            Frame.Navigate(typeof(Detail_P), aid, new DrillInNavigationTransitionInfo());
        }

        private void Comment_Navi(string content)
        {
            byte num = 0;
            switch (content)
            {
                case "广告": num = 0; break;
                case "番剧": num = 1; break;
                case "动画": num = 2; break;
                case "舞蹈": num = 3; break;
                case "娱乐": num = 4; break;
                case "时尚": num = 5; break;
                case "游戏": num = 6; break;
                case "鬼畜": num = 7; break;
                case "生活": num = 8; break;
                case "电影": num = 9; break;
                case "音乐": num = 10; break;
                case "电视剧": num = 11; break;
                case "科技": num = 12; break;
            }
            Frame.Navigate(typeof(PartViews.Part), num, new DrillInNavigationTransitionInfo());
        }

        // private void Refesh_Click(object sender, RoutedEventArgs e)
        // {
        //     loadItems();
        // }

        private async void mainpivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WebStatusHelper.IsOnline())
            {
                try
                {
                    switch (mainpivot.SelectedIndex)
                    {
                        case 0:
                            {
                                if (!isTopicLoaded)
                                {
                                    // string url = "http://api.bilibili.com/x/web-show/res/loc?jsonp=jsonp&pf=0&id=23";
                                    // List<Models.Topic> MyList = await ContentServ.GetTopicListAsync(url);
                                    // foreach (var item in MyList)
                                    // {
                                    //     show_1.Source.Add(new Controls.RecommandShow.MySource { bmp = new BitmapImage { UriSource = new Uri(item.Pic) }, url = item.Url });
                                    // }
                                    // show_1.show();
                                    await comment.init();
                                    header_Home.init(await ContentServ.GetHomeBanners());
                                    header_Home.navi += Header_navi;
                                    isTopicLoaded = true;
                                }
                            }
                            break;
                        case 1:
                            {

                                if (!await addcomment(cursor))
                                {
                                    await addcomment(cursor);
                                }
                                header_bangumi.init(await ContentServ.GetBangumiBanners());
                                if (list_lastupdate.Items.Count == 0)
                                {
                                    list_lastupdate.ItemsSource = await ContentServ.GetLastUpdateAsync();
                                }
                                header_bangumi.navi += Header_navi;
                            }
                            break;
                        case 2:
                            {
                                if (!isFriendsLoaded)
                                {
                                    await loadpulls(1);
                                    isFriendsLoaded = true;
                                }
                            }
                            break;
                        case 3:
                            {
                                // 发现
                                if (list_hottags.Items.Count == 0)
                                {
                                    list_hottags.ItemsSource = await ContentServ.GetHotSearchAsync();
                                }
                            }
                            break;
                    }
                }
                catch(Exception err)
                {
                    string a = err.Message;
                }
            }        
        }

        private void Header_navi(string arg)
        {
            // link=http://bangumi.bilibili.com/anime/5516
            // @"(?<=av)\d+
            if (Regex.IsMatch(arg, @"(?<=anime/)\d*"))
            {
                Frame.Navigate(typeof(Detail), Regex.Match(arg, @"(?<=anime/)\d*").Value, new DrillInNavigationTransitionInfo());
            }
            else
            {
                Frame.Navigate(typeof(MyWeb), arg, new DrillInNavigationTransitionInfo());
            }
        }
        bool isPullLoadingDone = false;
        async Task loadpulls(int page)
        {
            isloadingpull = true;
            List<Pulls> list = await ContentServ.GetPullsAsync(page);
            foreach (var item in list)
            {
                list_pull.Items.Add(item);
            }
            if (list.Count < 20)
            {
                list_pull.ContainerContentChanging -= list_pull_ContainerContentChanging;
                isPullLoadingDone = true;
            }
            isloadingpull = false;
        }

        // 番剧推荐
        async Task<bool> addcomment(string p)
        {
            isLoading = true;
            string url = "http://bangumi.bilibili.com/api/bangumi_recommend?appkey=" + ApiHelper.appkey + "&build=427000&cursor=" + p + "&pagesize=10&platform=android&ts=" + ApiHelper.GetLinuxTS().ToString();
            url += ApiHelper.GetSign(url);
            List<HotBangumi> MyList = await ContentServ.GetHotBangumiAsync(url);
            if (MyList.Count < 1) return false;
            foreach (var item in MyList)
            {
                list_commandbangumi.Items.Add(item);
            }
            isLoading = false;
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!WebStatusHelper.IsOnline())
            {
                Report("没有网络连接");
                return;
            }
            Button btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "1":
                    {
                        if (!ApiHelper.IsLogin())
                        {
                            Report("请先登录");
                            return;
                        }
                        Frame.Navigate(typeof(MyConcerns), UserHelper.Mid, new SlideNavigationTransitionInfo());
                    }
                    break;
                case "2": Frame.Navigate(typeof(Timeline), null, new SlideNavigationTransitionInfo()); break;
                case "3": Frame.Navigate(typeof(Bangumi), null, new SlideNavigationTransitionInfo()); break;
            }
        }
        bool isLoading = false;
        private void scollviewer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            scollviewer.ViewChanged += async (s, a) =>
            {
                if (scollviewer.VerticalOffset == scollviewer.ScrollableHeight && !isLoading)
                {
                    int count0 = list_commandbangumi.Items.Count;
                    // 滑动到底部了    
                    cursor = (list_commandbangumi.Items[list_commandbangumi.Items.Count - 1] as HotBangumi).Cursor;
                    await addcomment(cursor);
                    if (list_commandbangumi.Items.Count == count0)
                    {
                        return;
                    }
                }
            };
        }

        private void list_friends_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Friends), (e.ClickedItem as Friend).Fid, new DrillInNavigationTransitionInfo());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = WidthFit.GetWidth(ActualWidth);
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (WebStatusHelper.IsOnline())
                Frame.Navigate(typeof(Search), SearchBox.Text, new DrillInNavigationTransitionInfo());
        }

        private void list_tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Search), (e.ClickedItem as KeyWord).Keyword, new DrillInNavigationTransitionInfo());
        }

        private async void Random_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!WebStatusHelper.IsOnline())
            {
                Report("没有网络连接");
                return;
            }
            int aid = 0;
            Details details = new Details();
            do
            {
                aid = new Random().Next(10000, 5000000);
                string a = "http://app.bilibili.com/x/view?_device=android&_ulv=10000&plat=0&build=424000&aid=";
                details = await ContentServ.GetDetailsAsync(a + aid, true);
            } while (details == null || details.Aid == null);
            Frame.Navigate(typeof(Detail_P), aid);
        }

        private void Topic_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Button btn = sender as Button;
            Frame.Navigate(typeof(WebShell));
        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
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

        private void rank_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RankPage), null, new DrillInNavigationTransitionInfo());
        }

        private void list_lastupdate_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail), (e.ClickedItem as LastUpdate).Sid, new SlideNavigationTransitionInfo());
        }

        private void list_commandbangumi_ItemClick(object sender, ItemClickEventArgs e)
        {
            HotBangumi hot = e.ClickedItem as HotBangumi;
            string para = string.Empty;
            string url = hot.Link;
            if (Regex.IsMatch(url, @"anime/"))
            {
                para = Regex.Match(url, @"(?<=anime/)\d*").Value;
                this.Frame.Navigate(typeof(Detail), para);
                return;
            }
            if (Regex.IsMatch(url, @"bangumi/i/"))
            {
                para = Regex.Match(url, @"(?<=bangumi/i/)\d*").Value;
                this.Frame.Navigate(typeof(Detail), para);
                return;
            }
            if ((Regex.IsMatch(url, @"/video/av")))
            {
                para = Regex.Match(url, @"(?<=/video/av)\d*").Value;
                Frame.Navigate(typeof(Detail_P), para);
                return;
            }
            else
            {
                Frame.Navigate(typeof(MyWeb), url, new DrillInNavigationTransitionInfo());
            }
        }

        private void list_pull_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail_P), (e.ClickedItem as Pulls).Aid);
        }
        bool isloadingpull = false;
        private void list_pull_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var scroll = Load.FindChildOfType<ScrollViewer>(list_pull);
            scroll.ViewChanged += async (s, a) =>
            {
                if (scroll.VerticalOffset == scroll.ScrollableHeight && !isloadingpull && !isPullLoadingDone)  
                {
                    // 滑动到底部了    
                    int page = list_pull.Items.Count / 20 + 1;
                    await loadpulls(page);
                }
            };
        }
    }

    public sealed class PullTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Pull_up { get; set; }
        public DataTemplate Pull_bangumi { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Pulls pulls = item as Pulls;
            if (pulls != null)
            {
                switch(pulls.Type)
                {
                    case "1": return Pull_up;
                    case "3": return Pull_bangumi;
                }
            }
            return null;
        }
    }
}
