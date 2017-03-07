using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using bilibili.Http;
using bilibili.Models;
using bilibili.Helpers;

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
            string url = "http://bangumi.bilibili.com/api/tags?appkey=" + ApiHelper.appkey + "&build=424000&mobi_app=wp&platform=wp&page=1&pagesize=60&ts=" + ApiHelper.GetLinuxTS().ToString();
            url += ApiHelper.GetSign(url);
            List<Tags> tags = await ContentServ.GetTagsAsync(url);
            if (WebStatusHelper.IsOnline())
            {
                gridview.Items.Add(new Tags
                {
                    Cover = "http://i0.hdslb.com/bfs/bangumi/2da98805cad609d9d55d469b76d556520fc943dc.jpg",
                    TagID = "109",
                    TagName = "新番推荐"
                });
            }
            if (tags == null) return;
            foreach (var item in tags)
            {
                gridview.Items.Add(new Tags { Cover = item.Cover, TagID = item.TagID, TagName = item.TagName });
            }
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

        private void gridview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(BangList), (e.ClickedItem as Tags).TagID);
        }
    }
}
