using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using bilibili.Http;
using bilibili.Helpers;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BangList : Page
    {
        public BangList()
        {
            this.InitializeComponent();
        }

        string id = string.Empty;
        int page = 1;

        async Task Load(int page)
        {
            string url = "http://bangumi.bilibili.com/api/get_season_by_tag_v2?_device=android&appkey=85eb6835b0a1034e&build=424000&indexType=0&page=" + page.ToString() + "&pagesize=40&platform=wp&tag_id=" + id + "&ts=1474645896189&sign=";
            url += ApiHelper.GetSign(url);
            List<Models.Bangumi> list_rs = new List<Models.Bangumi>();
            list_rs = await ContentServ.GetBansByTagAsync(url);
            if (list_rs == null) return;
            foreach (var item in list_rs)
            {
                listview.Items.Add(item);
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            id = e.Parameter.ToString();
            await Load(1);
        }

        private void listTapped(object sender, TappedRoutedEventArgs e)
        {
            var item = listview.SelectedItem as Models.Bangumi;
            if (item != null)
                Frame.Navigate(typeof(Detail),item.ID);
        }

        private void listview_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            listview.ContainerContentChanging -= listview_ContainerContentChanging;
            var scroll = Methods.Load.FindChildOfType<ScrollViewer>(listview);
            scroll.ViewChanged += async (s, a) =>
            {
                if (scroll.VerticalOffset == scroll.ScrollableHeight)// && NextLoading)
                {
                    var text = Methods.Load.FindChildOfType<TextBlock>(listview);
                    int count0 = listview.Items.Count;
                    //滑动到底部了    
                    text.Visibility = Visibility.Visible;
                    page++;
                    await Load(page);
                    if (listview.Items.Count == count0)
                    {
                        text.Text = "番剧装填完毕！";
                        return;
                    }
                    listview.ContainerContentChanging += listview_ContainerContentChanging;
                    text.Visibility = Visibility.Collapsed;
                }
            };
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double i = ActualWidth;
            if (i > 1600)
            {
                i = i / 480;
            }
            else if (i > 1200)
            {
                i = 3;
            }
            else if (i > 800)
            {
                i = 2;
            }
            else
            {
                i = 1;
            }
            width.Width = this.ActualWidth / i - 12;
        }
    }
}
