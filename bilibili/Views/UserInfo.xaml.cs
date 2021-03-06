﻿using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using bilibili.Helpers;
using bilibili.Http;
using bilibili.Methods;
using bilibili.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using bilibili.Http.ContentService;
using System.ComponentModel;

//  “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
   /// <summary>
   /// 可用于自身或导航至 Frame 内部的空白页。
   /// </summary>
    public sealed partial class UserInfo : Page
    {
        List<Folder> myFolder = new List<Folder>();
        string tl, ts = string.Empty;
        int page_friend = 1;
        static bool isLoaded;
        public UserInfo()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await load();
        }
        async Task load()
        {
            try
            {
                if (!isLoaded)
                {
                    if (SettingHelper.GetValue("_accesskey").ToString().Length > 2)
                    {
                        string url = "http://account.bilibili.com/api/myinfo?access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&platform=wp&type=json";
                        url += ApiHelper.GetSign(url);
                        JsonObject json = await BaseService.GetJson(url);
                        if (json.ContainsKey("face"))
                            Face.Source = new BitmapImage { UriSource = new Uri(StringDeal.delQuotationmarks((json["face"].ToString()))) };
                        if (json.ContainsKey("coins"))
                            coins.Text = "硬币：" + json["coins"].ToString();
                        if (json.ContainsKey("sign"))
                            sign.Text = StringDeal.delQuotationmarks(json["sign"].ToString());
                        if (json.ContainsKey("uname"))
                            userName.Text = StringDeal.delQuotationmarks(json["uname"].ToString());
                        if (json.ContainsKey("level_info"))
                        {
                            JsonObject json2 = JsonObject.Parse(json["level_info"].ToString());
                            if (json2.ContainsKey("next_exp"))
                            {
                                exp_total.Text = json2["next_exp"].ToString();
                                bar.Maximum = Convert.ToInt32(json2["next_exp"].ToString());
                            }
                            if (json2.ContainsKey("current_exp"))
                            {
                                exp_current.Text = json2["current_exp"].ToString();
                                bar.Value = Convert.ToInt32(json2["current_exp"].ToString());
                            }
                            if (json2.ContainsKey("current_level"))
                            {
                                level.Source = new BitmapImage { UriSource = new Uri("ms-appx:///Assets//Others//lv" + json2["current_level"].ToString() + ".png", UriKind.Absolute) };
                            }
                            string url2 = "http://space.bilibili.com/ajax/settings/getSettings?mid=" + UserHelper.Mid;
                            JsonObject json_toutu = await BaseService.GetJson(url2);
                            if (json_toutu.ContainsKey("data"))
                            {
                                json_toutu = json_toutu["data"].GetObject();
                                if (json_toutu.ContainsKey("toutu"))
                                {
                                    json_toutu = json_toutu["toutu"].GetObject();
                                    if (json_toutu.ContainsKey("l_img"))
                                        tl = "http://i0.hdslb.com/" + json_toutu["l_img"].GetString();
                                    if (json_toutu.ContainsKey("s_img"))
                                        ts = "http://i0.hdslb.com/" + json_toutu["s_img"].GetString();

                                }
                            }
                            UpDateHeader();
                            int pagesize = 20;
                            if (SettingHelper.DeviceType == DeviceType.Mobile)
                            {
                                pagesize = 3;
                                width.Width = ActualWidth / 3 - 8;
                            }
                            myFolder = await ContentServ.GetFavFolders();
                            folderlist.ItemsSource = myFolder;
                            Site_Concern concern = await UserRelated.GetConcernBangumiAsync("", 1, true, pagesize);
                            if (concern != null)
                            {
                                concern_count.Text = concern.Count;
                                conlist.ItemsSource = concern.Result;
                            }
                            isLoaded = true;
                        }
                    }
                }            
            }
            catch(Exception)
            {
                
            }
        }

        public class MyClass : INotifyPropertyChanged
        {
            private double myWidth;
            public double MyWidth
            {
                get { return myWidth; }
                set
                {
                    myWidth = value;
                    OnPropertyChanged("MyWidth");
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyConcerns), UserHelper.Mid, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
        }

        private async void coin_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                Dialogs.CoinHistory ch = new Dialogs.CoinHistory();
                await ch.ShowAsync();
            }
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpDateHeader();
        }

        private void UpDateHeader()
        {
            if (tl != null && ts != null)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.UriSource = ActualWidth > 600 ? new Uri(tl, UriKind.Absolute) : new Uri(ts, UriKind.Absolute);
                img.Source = bmp;
            }
            Face.Width = Face.Height = ActualWidth > 600 ? 120 : 80;
        }

        private void folderlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(FavCollection), (e.ClickedItem as Folder).Name, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            double offset = scrollviewer.VerticalOffset;
            if (offset < img.ActualHeight)
            {
                img.Opacity = 1 - (offset / img.ActualHeight);
            }
        }

        private void conlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail), (e.ClickedItem as ConcernItem).Season_id);
        }

        private async void record_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                MenuFlyoutItem item = sender as MenuFlyoutItem;
                if (item.Tag.ToString() == "0")
                {
                    await new Dialogs.AccountRecord().ShowAsync();
                }
                else if (item.Tag.ToString() == "1")
                {
                    await new Dialogs.ExpRecord().ShowAsync();
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Tag.ToString() == "y")
            {
                ApiHelper.logout();
                Frame.Navigate(typeof(Partition));
            }
            flyout_logout.Hide();
        }

        private void list_friends_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Friends),(e.ClickedItem as Friend).Fid);
        }

        bool isFriendLoaded;
        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            switch(pivot.SelectedIndex)
            {
                case 2:
                    {
                        if (!isFriendLoaded)
                        {
                            var list = await ContentServ.GetFriendsAsync(UserHelper.Mid, page_friend);
                            if (list.isEmpty)
                            {
                                // 提示：没有关注的人
                            }
                            else
                            {
                                for (int i = 0; i < list.List.Count; i++)
                                {
                                    list_friends.Items.Add(list.List[i]);
                                }
                            }
                            isFriendLoaded = true;
                        }
                    };break;
            }
        }

        private void list_friends_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var scroll = Load.FindChildOfType<ScrollViewer>(list_friends);
            scroll.ViewChanged += async (s, a) =>
            {
                if (scroll.VerticalOffset == scroll.ScrollableHeight)
                {
                    page_friend++;
                    var result = await ContentServ.GetFriendsAsync(UserHelper.Mid, page_friend);
                    if (list_friends.Items.Count < result.Result)
                    {
                        for (int i = 0; i < result.List.Count; i++)
                        {
                            list_friends.Items.Add(result.List[i]);
                        }
                    }
                }
            };
        }

        private void qr_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(QRcode));
        }

        private async void ReFresh_Click(object sender, RoutedEventArgs e)
        {
            if (SettingHelper.GetValue("_accesskey").ToString().Length > 2)
            {
                string url = "http://account.bilibili.com/api/myinfo?access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&platform=wp&type=json";
                url += ApiHelper.GetSign(url);
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("coins"))
                    coins.Text = "硬币：" + json["coins"].ToString();
                if (json.ContainsKey("level_info"))
                {
                    JsonObject json2 = JsonObject.Parse(json["level_info"].ToString());
                    if (json2.ContainsKey("next_exp"))
                    {
                        exp_total.Text = json2["next_exp"].ToString();
                        bar.Maximum = Convert.ToInt32(json2["next_exp"].ToString());
                    }
                    if (json2.ContainsKey("current_exp"))
                    {
                        exp_current.Text = json2["current_exp"].ToString();
                        bar.Value = Convert.ToInt32(json2["current_exp"].ToString());
                    }
                    if (json2.ContainsKey("current_level"))
                    {
                        level.Source = new BitmapImage { UriSource = new Uri("ms-appx:///Assets//Others//lv" + json2["current_level"].ToString() + ".png", UriKind.Absolute) };
                    }
                }
            }
        }

        private void fav_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FavCollection), null, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
        }
    }
    public sealed class MyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ThreePic { get; set; }
        public DataTemplate TwoPic { get; set; }
        public DataTemplate OnePic { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Folder folder = item as Folder;
            if (folder != null)
            {
                switch (folder.VideoPics.Count)
                {
                    case 1: return OnePic;
                    case 2: return TwoPic;
                    case 3: return ThreePic;
                }
            }
            return null;
        }
    }
}
