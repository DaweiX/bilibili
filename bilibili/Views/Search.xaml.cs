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
using bilibili.Helpers;
// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Search : Page
    {
        string keyword = "";
        public Search()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            keyword = e.Parameter.ToString();
            keyword = Uri.EscapeUriString(keyword);
        }

        private void listTapped(object sender, TappedRoutedEventArgs e)
        {
            ListView listview = sender as ListView;
            switch (pivot.SelectedIndex)
            {
                case 0:
                    {
                        var item = listview.SelectedItem as SearchResult;
                        string aid = item.Aid;
                        Frame.Navigate(typeof(Detail_P), aid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    }break;
                case 1:
                    {
                        var item = listview.SelectedItem as SearchResult_Bangumi;
                        string sid = item.ID;
                        Frame.Navigate(typeof(Detail), sid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    }
                    break;
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem st = pivot.SelectedItem as PivotItem;
            string tag = st.Tag.ToString();
            switch(tag)
            {
                case "video":
                    {
                        if (list_videos.Items.Count == 0)
                        {
                            List<SearchResult> list_rs = new List<SearchResult>();
                            list_rs = await ContentServ.GetSearchResultAsync(keyword, 1);
                            if (list_rs == null)
                            {
                                messagepop.Show("没有搜索结果，请更换关键字重新搜索~");
                            }
                            else if (list_rs != null && list_rs.Count > 0)
                            {
                                foreach (var item in list_rs)
                                {
                                    list_videos.Items.Add(item);
                                }
                            }
                        }
                    }break;
                case "fanju":
                    {
                        if (list_fanju.Items.Count == 0)
                        {
                            List<SearchResult_Bangumi> list_rs = new List<SearchResult_Bangumi>();
                            list_rs = await ContentServ.GetBangumisAsync(keyword, 1);
                            if (list_rs == null)
                            {
                                messagepop.Show("没有搜索结果，请更换关键字重新搜索~");
                            }
                            else if (list_rs != null && list_rs.Count > 0)
                            {
                                foreach (var item in list_rs)
                                {
                                    list_fanju.Items.Add(item);
                                }
                            }
                        }
                    }
                    break;
                case "up":
                    {
                        //if (list_up.Items.Count == 0)
                        //{
                        //    List<Friend> list_rs = new List<Friend>();
                        //    list_rs = await ContentServ.GetUpsAsync(keyword, 1);
                        //    if (list_rs == null)
                        //    {
                        //        messagepop.Show("没有搜索结果，请更换关键字重新搜索~");
                        //    }
                        //    else if (list_rs != null && list_rs.Count > 0)
                        //    {
                        //        foreach (var item in list_rs)
                        //        {
                        //            list_fanju.Items.Add(item);
                        //        }
                        //    }
                        //}
                    }
                    break;
            }
        }
        bool isLoading = false;
        private void list_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            ListView listview = sender as ListView;
            var scroll = Load.FindChildOfType<ScrollViewer>(listview);
            var text = Load.FindChildOfType<TextBlock>(listview);
            scroll.ViewChanged += async (s, a) =>
            {
                if ((scroll.VerticalOffset >= scroll.ScrollableHeight - 50 || scroll.ScrollableHeight == 0) && !isLoading)
                {
                    text.Visibility = Visibility.Visible;
                    int count0 = listview.Items.Count;
                    int page = listview.Items.Count / 20 + 1;
                    isLoading = true;
                    switch (listview.Tag.ToString())
                    {
                        case "videos":
                            {
                                var temps = await ContentServ.GetSearchResultAsync(keyword, page);
                                if (temps.Count == 0)
                                {
                                    text.Text = "装填完毕！";
                                    return;
                                }
                                text.Visibility = Visibility.Collapsed;
                                foreach (var item in temps)
                                {
                                    listview.Items.Add(item);
                                }
                                isLoading = false;
                            }
                            break;
                        case "bangumi":
                            {
                                var temps = await ContentServ.GetBangumisAsync(keyword, page);
                                if (temps.Count == 0)
                                {
                                    text.Text = "装填完毕！";
                                    return;
                                }
                                text.Visibility = Visibility.Collapsed;
                                foreach (var item in temps)
                                {
                                    listview.Items.Add(item);
                                }
                                isLoading = false;
                                break;
                            }
                        case "up":
                            {

                                break;
                            }
                    }
                }
            };
        }
    }
}
