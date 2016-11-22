using bilibili.Helpers;
using bilibili.Http;
using bilibili.Methods;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Topic : Page
    {
        public Topic()
        {
            this.InitializeComponent();
            load();
        }

        private async void load()
        {
            List<Models.Topic> topic = await ContentServ.GetTopicListAsync(1);
            foreach (var item in topic)
            {
                list_topic.Items.Add(item);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth < 640)
            {
                width.Width = double.NaN;
            }
            else
            {
                int i = Convert.ToInt32(this.ActualWidth / 400);
                width.Width = (this.ActualWidth / i) - 8 * i - 8;
            }
        }

        private void list_topic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyWeb), (list_topic.SelectedItem as Models.Topic).Url);
        }

        bool isLoading = false;
        private void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            GridView gridview = sender as GridView;
            var scroll = Load.FindChildOfType<ScrollViewer>(gridview);
            var text = Load.FindChildOfType<TextBlock>(gridview);
            scroll.ViewChanged += async (s, a) =>
            {
                if ((scroll.VerticalOffset >= scroll.ScrollableHeight - 50 || scroll.ScrollableHeight == 0) && !isLoading)
                {
                    text.Visibility = Visibility.Visible;
                    int count0 = gridview.Items.Count;
                    int page = gridview.Items.Count / 20 + 1;
                    isLoading = true;
                    List<Models.Topic> temps = await ContentServ.GetTopicListAsync(page);
                    if (temps.Count == 0)
                    {
                        text.Text = "装填完毕！";
                        return;
                    }
                    text.Visibility = Visibility.Collapsed;
                    foreach (var item in temps)
                    {
                        gridview.Items.Add(item);
                    }
                    isLoading = false;
                }
            };
        }
    }
}
