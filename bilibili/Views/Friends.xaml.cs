using bilibili.Http;
using bilibili.Models;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        User user = new User();
        public Friends()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            mid = e.Parameter.ToString();
            user = await ContentServ.GetUserinfoAsync(mid);
            Face.ImageSource = new BitmapImage { UriSource = new Uri(user.Face) };
            level.Source = new BitmapImage { UriSource = new Uri("ms-appx:///Assets//Others//lv" + user.Current_level + ".png", UriKind.Absolute) };
            exp_current.Text = user.Current_exp;
            toutu.Source = new BitmapImage(new Uri("http://i0.hdslb.com/" + user.Toutu));
            birth.Text += user.BirthDay;
            regdate.Text += user.RegTime;
            sex.Text += user.Sex;
            addr.Text += user.Place.Length > 0 ? user.Place : "未知";
            if (user.Current_level == "6")
                bar.Value = 100;
            else
            {
                bar.Value = int.Parse(user.Current_exp);
                bar.Maximum = int.Parse(user.Next_exp);
                exp_total.Text = user.Next_exp;
            }         
            userName.Text = user.Name;
            user.Sign = user.Sign;
            list_concern.ItemsSource = await ContentServ.GetFriendsAsync(mid);//以后得改
            list_videos.ItemsSource = await ContentServ.GetMyVideoAsync(mid, 1);
            conlist.ItemsSource = await ContentServ.GetFriendsCons(mid, 1);
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

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            TextBlock txt = this.FindName(string.Format("h{0}", pivot.SelectedIndex)) as TextBlock;
            for (int i = 0; i < pivot.Items.Count; i++)
            {
                TextBlock temp = this.FindName(string.Format("h{0}", i)) as TextBlock;
                temp.Foreground = new SolidColorBrush(Colors.LightGray);
            }
            txt.Foreground = new SolidColorBrush(Colors.White);
        }

        private void conlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (conlist.SelectedItem != null)
            {
                Frame.Navigate(typeof(Detail), (conlist.SelectedItem as Concern).ID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyConcerns), mid, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = Methods.WidthFit.GetWidth(ActualWidth, 400, 200);
        }

        private void toutu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = ActualWidth > 600 ? new Uri("http://i0.hdslb.com/" + user.Toutu, UriKind.Absolute) : new Uri("http://i0.hdslb.com/" + user.Toutu_s, UriKind.Absolute);
            toutu.Source = bmp;
        }
    }
}
