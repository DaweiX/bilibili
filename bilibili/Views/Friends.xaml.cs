using bilibili.Http.ContentService;
using bilibili.Methods;
using bilibili.Models;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Friends : Page
    {
        string mid = string.Empty;
        Site_UserInfo user = new Site_UserInfo();
        Site_UserSettings sets = new Site_UserSettings();
        bool isloaded_video = false;
        bool isloaded_friend = false;
        public Friends()
        {
            this.InitializeComponent();
            toutu.SizeChanged += toutu_SizeChanged;
            pivot.SelectionChanged += pivot_SelectionChanged;
            scroll_friend.ViewChanged += ScrollViewer_ViewChanged;
            scroll_myvideo.ViewChanged += scroll_myvideo_ViewChanged;
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
            Frame.Navigate(typeof(Detail_P), (e.ClickedItem as MyVideo).Aid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
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

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
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
            if (isLoading) return;
            if (scroll_friend.VerticalOffset > scroll_friend.ScrollableHeight - 50) 
            {
                isLoading = true;
                int page = list_concern.Items.Count / 30 + 1;
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
        bool isloading_myvideo = false;
        private async void scroll_myvideo_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (isloading_myvideo) return;
            if (scroll_myvideo.VerticalOffset > scroll_myvideo.ScrollableHeight - 50)
            {
                isloading_myvideo = true;
                int page = list_videos.Items.Count / 20 + 1;
                List<MyVideo> list = await UserRelated.GetMyVideoAsync(mid, page);
                foreach (var item in list)
                {
                    list_videos.Items.Add(item);
                }
                if (list.Count < 20)
                {
                    scroll_myvideo.ViewChanged -= scroll_myvideo_ViewChanged;
                }
                isloading_myvideo = false;
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isloaded_friend && isloaded_video)
            {
                pivot.SelectionChanged -= pivot_SelectionChanged;
                return;
            }
            if (pivot.SelectedIndex == 1 && !isloaded_video) 
            {
                List<MyVideo> myvideos = await UserRelated.GetMyVideoAsync(mid, 1);
                for (int i = 0; i < myvideos.Count; i++)
                {
                    list_videos.Items.Add(myvideos[i]);
                }
                if (myvideos.Count < 20)
                {
                    scroll_myvideo.ViewChanged -= scroll_myvideo_ViewChanged;
                }
                isloaded_video = true;
            }
            if (pivot.SelectedIndex == 2 && !isloaded_friend) 
            {
                List<Friend> list = await UserRelated.GetFriendsAsync(mid, 1);
                for (int i = 0; i < list.Count; i++)
                {
                    list_concern.Items.Add(list[i]);
                }
                if (list.Count < 30)
                {
                    scroll_friend.ViewChanged -= ScrollViewer_ViewChanged;
                }
                isloaded_friend = true;
            }
        }
    }
}
