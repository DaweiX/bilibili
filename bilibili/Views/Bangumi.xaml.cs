using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class Bangumi : Page
    {
        public Bangumi()
        {
            this.InitializeComponent();
            this.DataContext = this;          
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string url = "http://bangumi.bilibili.com/api/tags?appkey=85eb6835b0a1034e&build=424000&mobi_app=wp&platform=wp&page=1&pagesize=60&ts=1474645841127&sign=";
            string t = "appkey=85eb6835b0a1034e&build=424000&mobi_app=wp&platform=wp&page=1&pagesize=60&ts=1474645841127";
            string[] argss = t.Split('&');
            List<string> list = argss.ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string s in list)
            {
                stringBuilder.Append(stringBuilder.Length > 0 ? "&" : string.Empty);
                stringBuilder.Append(s);
            }
            string a = stringBuilder.ToString();
            string b = "2ad42749773c441109bdc0191257a664";
            string c = Secret.GetMD5(a + b);
            url += c;
            List<Tags> tags = await ContentServ.GetTagsAsync(url);
            if (tags != null && tags.Count > 0)
            {
                foreach (var item in tags)
                {
                    gridview.Items.Add(new Tags { Cover = item.Cover, TagID = item.TagID, TagName = item.TagName });
                }
            }
        }

        private void gridview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = gridview.SelectedItem as Tags;
            if (item != null)
                Frame.Navigate(typeof(BangList), item.TagID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth < 640)
            {
                width.Width = (this.ActualWidth / 3) - 8;
            }
            else
            {
                int i = Convert.ToInt32(this.ActualWidth / 180);
                width.Width = (this.ActualWidth / i) - 6 * i;
            }
        }
    }
}
