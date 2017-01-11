using bilibili.Http.ContentService;
using bilibili.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using bilibili.Methods;
using System.Collections.Generic;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Friends : Page
    {
        string mid = string.Empty;
        int page = 1;
        Site_UserInfo user = new Site_UserInfo();
        Site_UserSettings sets = new Site_UserSettings();
        public Friends()
        {
            this.InitializeComponent();
            toutu.SizeChanged += toutu_SizeChanged;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            mid = e.Parameter.ToString();
            user = await UserRelated.GetBasicInfoAsync(mid);
            sets = await UserRelated.GetUserSettingAsync(mid);
            Models.UserInfo info = user.Data;
            if (user.Status == true)
            {
                Face.ImageSource = new BitmapImage { UriSource = new Uri(info.Face) };
                level.Source = new BitmapImage { UriSource = new Uri("ms-appx:///Assets//Others//lv" + info.Level_Info.Current_level + ".png", UriKind.Absolute) };
                exp_current.Text = info.Level_Info.Current_exp;
                toutu.Source = new BitmapImage(new Uri("http://i0.hdslb.com/" + info.Toutu));
                if (!string.IsNullOrWhiteSpace(info.BirthDay))
                {
                    birth.Text = "生日：" + info.BirthDay;
                }
                if (!string.IsNullOrWhiteSpace(info.RegTime))
                {
                    regdate.Text += "注册日期：" + StringDeal.LinuxToData(info.RegTime);
                }
                if (!string.IsNullOrWhiteSpace(info.Sex))
                {
                    sex.Text += "性别：" + info.Sex;
                }
                if (!string.IsNullOrWhiteSpace(info.Place)) 
                {
                    addr.Text = "地址：" + info.Place;
                }
                if (info.Level_Info.Current_level == "6")
                    bar.Value = 100;
                else
                {
                    bar.Value = int.Parse(info.Level_Info.Current_exp);
                    bar.Maximum = int.Parse(info.Level_Info.Next_exp);
                    exp_total.Text = info.Level_Info.Next_exp;
                }
                userName.Text = info.Name;
                sign.Text = info.Sign;
            }
            if (sets.Status != true)
            {
                toutu.SizeChanged -= toutu_SizeChanged;
            }
            list_videos.ItemsSource = await UserRelated.GetMyVideoAsync(mid, 1);
            List<ConcernItem> concerns = await UserRelated.GetConcernBangumiAsync(mid, 1, false);
            if (concerns != null)
            {
                if (concerns[0].Title == "PRIVATE")
                {
                    txt_private.Visibility = Visibility.Visible;
                    conlist.Visibility = Visibility.Collapsed;
                }
                else
                {
                    conlist.ItemsSource = concerns;
                }
            }
            List<Friend> list = await UserRelated.GetFriendsAsync(mid, page);
            foreach (var item in list)
            {
                list_concern.Items.Add(item);
            }
            if (list.Count >= 30)
            {
                scroll_friend.ViewChanged += ScrollViewer_ViewChanged;
            }
        }

       

        private void list_concern_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem as Friend != null)
            {
                Frame.Navigate(typeof(Friends), (e.ClickedItem as Friend).Fid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }

        private void list_videos_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail_P), (e.ClickedItem as Content).Num, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void conlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (conlist.SelectedItem != null)
            {
                Frame.Navigate(typeof(Detail), (conlist.SelectedItem as ConcernItem).Season_id, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyConcerns), mid, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = WidthFit.GetWidth(ActualWidth, 600, 400);
        }

        private void toutu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = ActualWidth > 600 ? new Uri("http://i0.hdslb.com/" + sets.Data.Toutu.l_img, UriKind.Absolute) : new Uri("http://i0.hdslb.com/" + sets.Data.Toutu.S_img, UriKind.Absolute);
            toutu.Source = bmp;
        }

        bool isLoading = false;
        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!isLoading)
            {
                isLoading = true;
                page++;
                List<Friend> list = await UserRelated.GetFriendsAsync(mid, page);
                foreach (var item in list)
                {
                    list_concern.Items.Add(item);
                }
                if (list.Count < 30)
                {
                    scroll_friend.ViewChanged -= ScrollViewer_ViewChanged;
                }
                isLoading = false;
            }           
        }
    }
}
