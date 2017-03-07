using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using bilibili.Http;
using bilibili.Methods;
using bilibili.Models;
using System.Threading.Tasks;
using System.Collections;
// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Search : Page
    {
        string keyword = string.Empty;
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

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem st = pivot.SelectedItem as PivotItem;
            string tag = st.Tag.ToString();
            switch(tag)
            {
                case "video":
                    {
                        await LoadItem(list_videos, SearchType.Videos);
                    }break;
                case "fanju":
                    {
                        await LoadItem(list_fanju, SearchType.Animes);
                    }
                    break;
                case "up":
                    {
                        await LoadItem(list_up, SearchType.Ups);
                    }
                    break;
            }
        }

        enum SearchType
        {
            Videos,
            Animes,
            Ups
        }

        async Task LoadItem(GridView list, SearchType type)
        {
            try
            {
                if (list.Items.Count == 0 && !isLoading)
                {
                    ArrayList items = new ArrayList();
                    switch (type)
                    {
                        case SearchType.Videos:
                            var t1 = await ContentServ.GetSearchResultAsync(keyword, 1);
                            foreach (var item in t1)
                            {
                                items.Add(item);
                            }
                            break;
                        case SearchType.Animes:
                            var t2 = await ContentServ.GetBangumisAsync(keyword, 1);
                            foreach (var item in t2)
                            {
                                items.Add(item);
                            }
                            break;
                        case SearchType.Ups:
                            var t3 = await ContentServ.GetUpsAsync(keyword, 1);
                            foreach (var item in t3)
                            {
                                items.Add(item);
                            }
                            break;
                    }
                    if (items == null)
                    {
                        return;
                    }
                    else if (list != null && items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            list.Items.Add(item);
                        }
                    }
                    if (items.Count < 20)
                    {
                        var text = Load.FindChildOfType<TextBlock>(list);
                        text.Text = "装填完毕！";
                    }
                }
                return;
            }
            catch { }
        }

        bool isLoading = false;
        private void list_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            GridView listview = sender as GridView;
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
                                if (temps.Count < 20) 
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
                                if (temps.Count < 20)
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
                                List<UpForSearch> uplist = await ContentServ.GetUpsAsync(keyword, page);
                                if (uplist.Count < 20)
                                {
                                    text.Text = "装填完毕！";
                                    return;
                                }
                                text.Visibility = Visibility.Collapsed;
                                foreach (var item in uplist)
                                {
                                    listview.Items.Add(item);
                                }
                                isLoading = false;
                                break;
                            }
                    }
                }
            };
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double i = ActualWidth;
            //if (i > 1200)
            //{
            //    i /= 2;
            //}
            //else if (i > 750)
            //{
            //    i = 3;
            //}
            //else if (i > 500)
            //{
            //    i = 2;
            //}
            //else
            //{
            //    i = 1;
            //}
            //i /= 300;
            //i = i == 0 ? 1 : i;
            //width.Width = ActualWidth / (int)i - 20;
            width.Width = WidthFit.GetWidth(i);
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            //当然，不一定非要用pivot判定，也可以使用is进行类型的判定
            switch (pivot.SelectedIndex)
            {
                case 0:
                    {
                        var item = e.ClickedItem as SearchResult;
                        string aid = item.Aid;
                        Frame.Navigate(typeof(Detail_P), aid);
                    }
                    break;
                case 1:
                    {
                        var item = e.ClickedItem as SearchResult_Bangumi;
                        string sid = item.ID;
                        Frame.Navigate(typeof(Detail), sid);
                    }
                    break;
                case 2:
                    {
                        var item = e.ClickedItem as UpForSearch;
                        string mid = item.Param;
                        Frame.Navigate(typeof(Friends), mid);
                    }
                    break;
            }
        }
    }
}
