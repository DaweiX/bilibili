using bilibili.Http;
using bilibili.Methods;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using bilibili.FrameManager;

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
            ContentServ.report += Report;
        }

        private async void Report(string status)
        {
            await popup.Show(status);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            load();
        }

        private async void load()
        {
            List<Models.Topic> topic = await ContentServ.GetTopicListAsync(1);
            if (topic != null)
            {
                foreach (var item in topic)
                {
                    list_topic.Items.Add(item);
                }
            }

            list_event.Visibility = Visibility.Visible;
            List<Models.Event> events = await ContentServ.GetEventListAsync(1);
            if (events != null)
            {
                foreach (var item in events)
                {
                    list_event.Items.Add(item);
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = WidthFit.GetWidth(ActualWidth);
        }

        bool isLoading = false;
        private void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            GridView gridview = sender as GridView;
            var scroll = Load.FindChildOfType<ScrollViewer>(gridview);
            scroll.ViewChanged += async (s, a) =>
            {
                if ((scroll.VerticalOffset >= scroll.ScrollableHeight - 50 || scroll.ScrollableHeight == 0) && !isLoading)
                {
                    int count0 = gridview.Items.Count;
                    int page = gridview.Items.Count / 20 + 1;
                    isLoading = true;
                    if (gridview.Tag.ToString() == "0")
                    {
                        var temps = await ContentServ.GetTopicListAsync(page);
                        if (temps.Count == 0)
                        {
                            return;
                        }
                        foreach (var item in temps)
                        {
                            gridview.Items.Add(item);
                        }
                        isLoading = false;
                    }
                    else
                    {
                        var temps = await ContentServ.GetEventListAsync(page);
                        if (temps.Count == 0)
                        {
                            return;
                        }
                        foreach (var item in temps)
                        {
                            gridview.Items.Add(item);
                        }
                        isLoading = false;
                    }                  
                }
            };
        }

        private void list_topic_ItemClick(object sender, ItemClickEventArgs e)
        {
            GridView list = sender as GridView;
            if (list.Tag.ToString() == "0") 
            {
                FrameHelpers.FrameManager.RightFrameNavi(frame => 
                {
                    frame.Navigate(typeof(MyWeb), (e.ClickedItem as Models.Topic).Url);
                });
            }
            else if(list.Tag.ToString() == "1")
            {
                FrameHelpers.FrameManager.RightFrameNavi(frame =>
                {
                    frame.Navigate(typeof(MyWeb), (e.ClickedItem as Models.Event).Link);
                });
            }
        }
    }
}
