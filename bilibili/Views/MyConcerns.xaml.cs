using bilibili.Http;
using bilibili.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using bilibili.Methods;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using bilibili.Helpers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using bilibili.Http.ContentService;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyConcerns : Page
    {
        bool isGrid;
        bool? isMySelf = null;
        int page = 1;
        string mid = string.Empty;
        string url = string.Empty;
        public MyConcerns()
        {
            this.InitializeComponent();         
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            mid = e.Parameter.ToString();
            await load(mid); 
        }

        async Task load(string mid)
        {
            if (SettingHelper.Devicetype == DeviceType.PC)
            {
                isGrid = true;
                conlist.ItemTemplate = this.Resources["TemplateGrid"] as DataTemplate;
            }
            else
            {
                isGrid = false;
                conlist.ItemTemplate = this.Resources["TemplateList"] as DataTemplate;
            }
            isMySelf = mid == UserHelper.mid ? true : false;
            List<ConcernItem> list = await UserRelated.GetConcernBangumiAsync(mid, 1, (bool)isMySelf);
            foreach (var item in list)
            {
                conlist.Items.Add(item);
            }
            if (conlist.Items.Count < 20)
            {
                var text1 = Load.FindChildOfType<TextBlock>(conlist);
                text1.Text = "加载完毕！";

            }
            con.Text += conlist.Items.Count == 0 ? "（暂无订阅）" : "";
        }

        private void Reflesh_Click(object sender, RoutedEventArgs e)
        {
            page = 1;
            conlist.Items.Clear();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            isGrid = !isGrid;
            if (isGrid)
            {
                conlist.ItemTemplate = this.Resources["TemplateGrid"] as DataTemplate;
            }
            else
            {
                conlist.ItemTemplate = this.Resources["TemplateList"] as DataTemplate;
            }
            FitWidth();
        }

        bool isLoading = false;
        bool LoadingDone = false;
        private void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (isLoading) return;
            if (LoadingDone) return;
            GridView view = sender as GridView;
            var scroll = Load.FindChildOfType<ScrollViewer>(view);
            var text = Load.FindChildOfType<TextBlock>(view);
            scroll.ViewChanged += async (s, a) =>
            {
                if (scroll.VerticalOffset >= scroll.ScrollableHeight - 50 && !isLoading) 
                {
                    text.Visibility = Visibility.Visible;
                    int count0 = view.Items.Count;
                    page++;
                    isLoading = true;
                    List<ConcernItem> list = await UserRelated.GetConcernBangumiAsync(mid, page, (bool)isMySelf);
                    foreach (var item in list)
                    {
                        view.Items.Add(item);
                    }
                    text.Visibility = Visibility.Collapsed;
                    if (list.Count < 20)
                    {
                        LoadingDone = true;
                        text.Text = "装填完毕！";
                        return;
                    }
                    isLoading = false;
                }
            };
        }

        private void conlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail), (e.ClickedItem as ConcernItem).Season_id, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void conlist_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FitWidth();
        }

        private void FitWidth()
        {
            if (isGrid)
            {
                width.Width = WidthFit.GetWidth(ActualWidth, 180, 160);
            }
            else
            {
                width.Width = WidthFit.GetWidth(ActualWidth, 400, 200);
            }
        }
    }
}
