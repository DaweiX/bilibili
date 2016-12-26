using System;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using bilibili.Http;
using bilibili.Methods;
using bilibili.Models;
using System.Collections.Generic;
using bilibili.Helpers;
using Windows.UI;
using Windows.UI.Xaml.Media;
using bilibili.UI;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Timeline : Page
    {
        public Timeline()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            show();
        }
        async void show()
        {
           JsonObject json = await BaseService.GetJson("http://bangumi.bilibili.com/jsonp/timeline_v2?_device=wp&_ulv=10000&build=424000&platform=android&appkey=422fd9d7289a1dd9");
            if (json.ContainsKey("list"))
            {
                var list = json["list"];
                if (list != null)
                {
                    var json2 = list.GetArray();
                    for (int i = 0; i < json2.Count; i++)
                    {
                        Times time = new Times();
                        var json3 = JsonObject.Parse(json2[i].ToString());
                        if (json3.ContainsKey("title"))
                            time.Title = json3["title"].GetString();
                        if (json3.ContainsKey("bgmcount"))
                            time.Count = StringDeal.delQuotationmarks(json3["bgmcount"].ToString());
                        if (json3.ContainsKey("lastupdate_at"))
                            time.LastUpdate = json3["lastupdate_at"].GetString();
                        if (json3.ContainsKey("weekday"))
                            time.Weekday = json3["weekday"].ToString();
                        if (json3.ContainsKey("is_finish"))
                            time.IsFinish = json3["is_finish"].ToString();
                        if (json3.ContainsKey("square_cover"))
                            time.Cover = json3["square_cover"].GetString();
                        if (json3.ContainsKey("new"))
                            time.IsNew = json3["new"].ToString() == "true" ? true : false;
                        time.ID = json3["season_id"].ToString();
                        listview.Items.Add(time);
                    }
                    var groups = from item in listview.Items group item by (item as Times).Weekday;
                    this.cvsData.Source = groups;
                    listview.SelectedIndex = -1;
                }
            }
        }

        private void listview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = listview.SelectedItem as Times;
            if (item != null)
                Frame.Navigate(typeof(Detail), item.ID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth < 640)
            {
                width.Width = ActualWidth - 32;
            }
            else
            {
                int i = Convert.ToInt32(this.ActualWidth / 400);
                width.Width = (this.ActualWidth / i) - 8 * i - 16;
            }
        }
    }

    public class BoolToForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            SolidColorBrush brush = new SolidColorBrush();
            if (b)
            {
                brush.Color = ColorRelated.GetColor();
                return brush;
            }
            else
            {
                return Application.Current.Resources["bili_Fontcolor_Main"];
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
