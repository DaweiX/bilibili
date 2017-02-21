using bilibili.Helpers;
using bilibili.Http;
using bilibili.Models;
using bilibili.Methods;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RankPage : Page
    {
        public RankPage()
        {
            this.InitializeComponent();
            ContentServ.report += Report;
            for (int i = 0; i < headlist.Count; i++)
            {
                mainpivot.Items.Add(GetPivotItem(headlist[i]));
            }
        }

        private async void Report(string status)
        {
            await popup.Show(status);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = Methods.WidthFit.GetWidth(ActualWidth, 600, 400);
        }

        private void listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Rank rank = e.ClickedItem as Rank;
            if (rank != null)
            {
                Frame.Navigate(typeof(Detail_P), rank.Aid, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
            }
        }

        private async void mainpivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tid= (mainpivot.SelectedItem as PivotItem).Tag.ToString();
            GridView gridview = FindName("list" + tid) as GridView;
            if (gridview != null && gridview.Items.Count == 0)
            {
                gridview.ItemsSource = tid == "0"
                    ? await ContentServ.GetOriRankItemsAsync()
                    : await ContentServ.GetRankItemsAsync(tid);
            }
        }

        List<KeyValuePair<string, string>> headlist => new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("原创","0"),
            new KeyValuePair<string, string>("番剧","13"),
            new KeyValuePair<string, string>("动画","1"),
            new KeyValuePair<string, string>("音乐","3"),
            new KeyValuePair<string, string>("舞蹈","129"),
            new KeyValuePair<string, string>("科技","36"),
            new KeyValuePair<string, string>("游戏","4"),
            new KeyValuePair<string, string>("生活","160"),
            new KeyValuePair<string, string>("鬼畜","119"),
            new KeyValuePair<string, string>("时尚","155"),
            new KeyValuePair<string, string>("娱乐","5"),
            new KeyValuePair<string, string>("电影","23"),
            new KeyValuePair<string, string>("电视剧","11"),
        };

        private PivotItem GetPivotItem(KeyValuePair<string, string> args)
        {
            PivotItem pivotItem = new PivotItem
            {
                Margin = new Thickness(0),
                Header = args.Key,
                Tag = args.Value
            };
            GridView gridview = new GridView
            {
                Name = "list" + args.Value,
                HorizontalAlignment = HorizontalAlignment.Center,
                SelectionMode = ListViewSelectionMode.None,
                IsItemClickEnabled = true,
                ItemTemplate = Resources["mydatatemplate"] as DataTemplate
            };
            gridview.ItemClick += listview_ItemClick;
            pivotItem.Content = gridview;
            return pivotItem;
        }
    }
}
