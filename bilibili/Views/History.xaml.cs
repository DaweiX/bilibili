using bilibili.Helpers;
using bilibili.Http;
using bilibili.Methods;
using Windows.UI.Xaml;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class History : Page
    {
        int page = 1;
        public History()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await load(1);
        }
        //http://api.bilibili.com/x/v2/history?_device=android&_hwid=ccbb856c97ccb8d2&_ulv=10000&access_key=a48208a7c5b67a4a369124cf5c1b515c&appkey=1d8b6e7d45233436&build=427000&mobi_app=android&platform=android&pn=1&ps=200

        async Task load(int p)
        {
            string url = "http://api.bilibili.com/x/v2/history?_device=wp&_ulv=10000&access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&build=427000&platform=android&pn=" + p.ToString() + "&ps=20";
            url += ApiHelper.GetSign(url);
            List<Models.History> hs = await ContentServ.GetHistoryAsync(url);
            conlist.ItemsSource = hs;
        }

        private void conlist_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Models.History item = conlist.SelectedItem as Models.History;
            if (item != null) 
                Frame.Navigate(typeof(Detail_P), item.Aid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void conlist_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var scroll = Load.FindChildOfType<ScrollViewer>(conlist);
            var text = Load.FindChildOfType<TextBlock>(conlist);
            scroll.ViewChanged += async (s, a) =>
            {
                if (scroll.VerticalOffset == scroll.ScrollableHeight)// && NextLoading)
                {
                    int count0 = conlist.Items.Count;
                    //滑动到底部了    
                    text.Visibility = Visibility.Visible;
                    page++;
                    await load(page);
                    if (conlist.Items.Count == count0)
                    {
                        text.Text = "评论装填完毕！";
                        return;
                    }
                    text.Visibility = Visibility.Collapsed;
                }
            };
        }
    }
}
