using System;
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
using Newtonsoft.Json;
using bilibili.Http.ContentService;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserInfo : Page
    {
        List<Folder> myFolder = new List<Folder>();
        string tl, ts = string.Empty;
        static bool isLoaded = false;
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
                            coins.Text += json["coins"].ToString();
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
                            string url2 = "http://space.bilibili.com/ajax/settings/getSettings?mid=" + UserHelper.mid;
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
                            myFolder = await ContentServ.GetFavFolders();
                            folderlist.ItemsSource = myFolder;
                            conlist.ItemsSource = await UserRelated.GetConcernBangumiAsync("", 1, true);
                            isLoaded = true;
                        }
                    }
                }            
            }
            catch
            {
                
            }
        }

        private void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ApiHelper.logout();
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyConcerns), UserHelper.mid, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
        }

        private void conlist_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            GridView ctrl = sender as GridView;
            var item = ctrl.SelectedItem;
            if (item != null)
            {
                Frame.Navigate(typeof(Detail), (item as ConcernItem).Season_id, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }

        private async void coin_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                Dialogs.CoinHistory ch = new Dialogs.CoinHistory();
                await ch.ShowAsync();
            }
        }

        private void StackPanel_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
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

        private async void Face_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                ContentDialog dialog = new ContentDialog
                {
                    Content = "确定登出吗？",
                    IsPrimaryButtonEnabled = true,
                    IsSecondaryButtonEnabled = true,
                    PrimaryButtonText = "确定",
                    SecondaryButtonText = "手滑了",
                };
                dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;
                await dialog.ShowAsync();
            }
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

        private void fav_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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
