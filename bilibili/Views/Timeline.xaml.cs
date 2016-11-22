using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using bilibili.Http;
using bilibili.Methods;
using bilibili.Models;

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
                            time.Title = StringDeal.delQuotationmarks(json3["title"].ToString());
                        if (json3.ContainsKey("bgmcount"))
                            time.Count = StringDeal.delQuotationmarks(json3["bgmcount"].ToString());
                        if (json3.ContainsKey("lastupdate_at"))
                            time.LastUpdate = StringDeal.delQuotationmarks(json3["lastupdate_at"].ToString());
                        if (json3.ContainsKey("weekday"))
                            time.Weekday = Convert.ToInt32(json3["weekday"].ToString());
                        if (json3.ContainsKey("is_finish"))
                            time.IsFinish = Convert.ToInt32(json3["is_finish"].ToString());
                        if (json3.ContainsKey("square_cover"))
                            time.Cover = StringDeal.delQuotationmarks(json3["square_cover"].ToString());
                        if (json3.ContainsKey("new"))
                            time.IsNew = json3["new"].ToString() == "true" ? true : false;
                        time.ID = json3["season_id"].ToString();
                        listview.Items.Add(time);
                    }
                    var groups = from item in listview.Items group item by ((Times)item).Weekday;
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
    public class NumToWeekdays : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (int)value;
            switch (b)
            {
                case -1:return "其他";
                case 1: return "星期一";
                case 2: return "星期二";
                case 3: return "星期三"; 
                case 4: return "星期四"; 
                case 5: return "星期五";
                case 6: return "星期六";
                case 0: return "星期日";
                default:return "未知";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class BoolToForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            return b ? "#e273a9" : "auto";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class BoolToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)value == 0 ? "未完结" : "已完结";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
