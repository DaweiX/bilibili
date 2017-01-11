using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using bilibili.Http;
using bilibili.Models;
using bilibili.Helpers;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media.Animation;
using Windows.Data.Json;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

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
            if(!WebStatusHelper.IsOnline())
            {
                popup.Show("没有网络连接", 3000);
            }
            comment.Navi += Comment_Navi;
            comment.Info += Comment_Info;
            comment.live += Comment_live;
        }

        private void Comment_live(string tid)
        {
            Frame.Navigate(typeof(Live), tid);
        }

        private void Comment_Info(string aid)
        {
            Frame.Navigate(typeof(Detail_P), aid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void Comment_Navi(string content)
        {
            switch (content)
            {
                case "番剧": Frame.Navigate(typeof(PartViews.Bangumi), null, new DrillInNavigationTransitionInfo()); break;
                case "动画": Frame.Navigate(typeof(PartViews.Cartoon), null, new DrillInNavigationTransitionInfo()); break;
                case "生活": Frame.Navigate(typeof(PartViews.Life), null, new DrillInNavigationTransitionInfo()); break;
                case "电影": Frame.Navigate(typeof(PartViews.Movies), null, new DrillInNavigationTransitionInfo()); break;
                case "娱乐": Frame.Navigate(typeof(PartViews.Entertain), null, new DrillInNavigationTransitionInfo()); break;
                case "鬼畜": Frame.Navigate(typeof(PartViews.Guichu), null, new DrillInNavigationTransitionInfo()); break;
                case "电视剧": Frame.Navigate(typeof(PartViews.Soap), null, new DrillInNavigationTransitionInfo()); break;
                case "广告": Frame.Navigate(typeof(PartViews.Ad), null, new DrillInNavigationTransitionInfo()); break;
                case "舞蹈": Frame.Navigate(typeof(PartViews.Dance), null, new DrillInNavigationTransitionInfo()); break;
                case "游戏": Frame.Navigate(typeof(PartViews.Game), null, new DrillInNavigationTransitionInfo()); break;
                case "科技": Frame.Navigate(typeof(PartViews.Tech), null, new DrillInNavigationTransitionInfo()); break;
                case "时尚": Frame.Navigate(typeof(PartViews.Fashion), null, new DrillInNavigationTransitionInfo()); break;
                case "音乐": Frame.Navigate(typeof(PartViews.Music), null, new DrillInNavigationTransitionInfo()); break;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;
            string tag = btn.Tag.ToString();
            string content = btn.Content.ToString();
            switch(btn.Content.ToString())
            {
                case ">番剧": Frame.Navigate(typeof(PartViews.Bangumi), null, new DrillInNavigationTransitionInfo());break;
                case ">动画": Frame.Navigate(typeof(PartViews.Cartoon), null, new DrillInNavigationTransitionInfo()); break;
                case ">生活": Frame.Navigate(typeof(PartViews.Life), null, new DrillInNavigationTransitionInfo()); break;
                case ">电影": Frame.Navigate(typeof(PartViews.Movies), null, new DrillInNavigationTransitionInfo()); break;
                case ">娱乐": Frame.Navigate(typeof(PartViews.Entertain), null, new DrillInNavigationTransitionInfo()); break;
                case ">鬼畜": Frame.Navigate(typeof(PartViews.Guichu), null, new DrillInNavigationTransitionInfo()); break;
                case ">电视剧": Frame.Navigate(typeof(PartViews.Soap), null, new DrillInNavigationTransitionInfo()); break;
                case ">广告": Frame.Navigate(typeof(PartViews.Ad), null, new DrillInNavigationTransitionInfo()); break;
                case ">舞蹈": Frame.Navigate(typeof(PartViews.Dance), null, new DrillInNavigationTransitionInfo()); break;
                case ">游戏": Frame.Navigate(typeof(PartViews.Game), null, new DrillInNavigationTransitionInfo()); break;
                case ">科技": Frame.Navigate(typeof(PartViews.Tech), null, new DrillInNavigationTransitionInfo()); break;
                case ">时尚": Frame.Navigate(typeof(PartViews.Fashion), null, new DrillInNavigationTransitionInfo()); break;
                case ">音乐": Frame.Navigate(typeof(PartViews.Music), null, new DrillInNavigationTransitionInfo()); break;
            }
        }

        //private void Refesh_Click(object sender, RoutedEventArgs e)
        //{
        //    loadItems();
        //}

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
                                    bar.Visibility = Visibility.Visible;
                                    //string url = "http://api.bilibili.com/x/web-show/res/loc?jsonp=jsonp&pf=0&id=23";
                                    //List<Models.Topic> MyList = await ContentServ.GetTopicListAsync(url);
                                    //foreach (var item in MyList)
                                    //{
                                    //    show_1.Source.Add(new Controls.RecommandShow.MySource { bmp = new BitmapImage { UriSource = new Uri(item.Pic) }, url = item.Url });
                                    //}
                                    //show_1.show();
                                    await comment.init();
                                    if (SettingHelper.ContainsKey("_banner"))
                                    {
                                        if ((bool)SettingHelper.GetValue("_banner") == true)
                                        {
                                            header_Home.init(await ContentServ.GetHomeBanners(), 3.2);
                                            header_Home.navi += Header_navi;
                                        }
                                    }
                                    bar.Visibility = Visibility.Collapsed;
                                    isTopicLoaded = true;
                                }
                            }
                            break;
                        case 1:
                            {
                                bar.Visibility = Visibility.Visible;

                                if (!await addcomment(cursor))
                                {
                                    await addcomment(cursor);
                                }
                                header_bangumi.Visibility = Visibility.Collapsed;
                                //header_bangumi.init(await ContentServ.GetBangumiBanners(), 6.2 / 3);
                                if (list_lastupdate.Items.Count == 0)
                                {
                                    list_lastupdate.ItemsSource = await ContentServ.GetLastUpdateAsync();
                                }
                                header_bangumi.navi += Header_navi;
                                bar.Visibility = Visibility.Collapsed;
                            }
                            break;
                        case 2:
                            {
                                if (!isFriendsLoaded)
                                {
                                    bar.Visibility = Visibility.Collapsed;
                                    await loadfriends();
                                    bar.Visibility = Visibility.Visible;
                                    isFriendsLoaded = true;
                                }
                            }
                            break;
                        case 3:
                            {
                                //发现
                                if (list_hottags.Items.Count == 0)
                                {
                                    list_hottags.ItemsSource = await ContentServ.GetHotSearchAsync();
                                }
                            }
                            break;
                    }
                }
                catch
                {
                    
                }
            }        
        }

        private void Header_navi(string arg)
        {
            //link=http://bangumi.bilibili.com/anime/5516
            //@"(?<=av)\d+
            if (Regex.IsMatch(arg, @"(?<=anime/)\d*"))
            {
                Frame.Navigate(typeof(Detail), Regex.Match(arg, @"(?<=anime/)\d*").Value, new DrillInNavigationTransitionInfo());
            }
            else
            {
                Frame.Navigate(typeof(MyWeb), arg, new DrillInNavigationTransitionInfo());
            }
        }

        async Task<bool> loadfriends()
        {
            list_friends.ItemsSource = await ContentServ.GetFriendsAsync(UserHelper.mid);
            return true;
        }

        //番剧推荐
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
            Button btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "1": Frame.Navigate(typeof(MyConcerns), UserHelper.mid, new SlideNavigationTransitionInfo());break;
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
                    //滑动到底部了    
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
            Frame.Navigate(typeof(Friends), (e.ClickedItem as Friend).Fid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double i = ActualWidth;
            width.Width = Methods.WidthFit.GetWidth(i, 400, 280);
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (WebStatusHelper.IsOnline())
                Frame.Navigate(typeof(Views.Search), SearchBox.Text, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void list_tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Search), (e.ClickedItem as KeyWord).Keyword, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private async void Random_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            int aid = 0;
            Details details = new Details();
            do
            {
                aid = new Random().Next(10000, 5000000);
                string a = "http://app.bilibili.com/x/view?_device=android&_ulv=10000&plat=0&build=424000&aid=";
                details = await ContentServ.GetDetailsAsync(a + aid);
            } while (details == null);
            Frame.Navigate(typeof(Detail_P), aid);
        }

        private void Topic_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Tag.ToString() == "0")
            {
                Frame.Navigate(typeof(Topic), true, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                return;
            }
            if (btn.Tag.ToString() == "1")
            {
                Frame.Navigate(typeof(Topic), false, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                return;
            }
        }

        private void gridview_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            GridView gridview = sender as GridView;
            var item = gridview.SelectedItem as Content;
            if (item != null)
            {
                Frame.Navigate(typeof(Detail_P), item.Num, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }

        private void scollviewer2_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {

        }

        private void list_live_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

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
    }

    public class WidthToHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (double)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
