using bilibili.Http;
using bilibili.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System;
using bilibili.Methods;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using bilibili.Helpers;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyConcerns : Page
    {
        bool isGrid;
        bool isConcernLoad = false;
        int page = 1;
        public MyConcerns()
        {
            this.InitializeComponent();         
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!isConcernLoad)
            {
                if (e.Parameter == null)
                {
                    load(string.Empty);
                }
                else
                {
                    load(e.Parameter.ToString());
                }
            }            
        }

        async void load(string mid)
        {
            if (SettingHelper.GetDeviceType() == DeviceType.PC)
            {
                isGrid = true;
                conlist.ItemTemplate = this.Resources["TemplateGrid"] as DataTemplate;
            }
            else
            {
                isGrid = false;
                conlist.ItemTemplate = this.Resources["TemplateList"] as DataTemplate;
            }
            if (mid.Length == 0)
            {
                foreach (var item in await ContentServ.GetConAsync(1))
                {
                    conlist.Items.Add(item);
                }
            }
            else
            {
                foreach (var item in await ContentServ.GetFriendsCons(mid, 1)) 
                {
                    conlist.Items.Add(item);
                }
            }
            if (conlist.Items.Count < 30)
            {
                var text1 = Load.FindChildOfType<TextBlock>(conlist);
                text1.Text = "加载完毕！";

            }
            con.Text += conlist.Items.Count == 0 ? "（暂无订阅）" : "";
            isConcernLoad = true;
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
                    List<Concern> temps = await ContentServ.GetConAsync(page);
                    text.Visibility = Visibility.Collapsed;
                    foreach (var item in temps)
                    {
                        view.Items.Add(item);
                    }
                    if (temps.Count < 30)
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
            Frame.Navigate(typeof(Detail), (e.ClickedItem as Concern).ID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void conlist_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FitWidth();
        }

        private void FitWidth()
        {
            if (isGrid)
            {
                width.Width = WidthFit.GetWidth(ActualWidth, 160, 120, 10);
            }
            else
            {
                width.Width = WidthFit.GetWidth(ActualWidth, 500, 400);
            }
        }
    }
}
