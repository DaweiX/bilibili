using bilibili.Http;
using bilibili.Methods;
using bilibili.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views.PartViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Bangumi : Page
    {
        public Bangumi()
        {
            this.InitializeComponent();
        }

        private void GridView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GridView gr = sender as GridView;
            Content content = gr.SelectedItem as Content;
            if (content != null)
            {
                Frame.Navigate(typeof(Detail_P), content.Num, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;                       
            switch (pivot.SelectedIndex)
            {
                case 0:
                    {
                        if (gv_hot.Items.Count == 0)
                        {
                            var temp = await ContentServ.GetContentAsync(13, 1);
                            foreach (var item in temp)
                            {
                                gv_hot.Items.Add(item);
                            }
                        }
                    }
                    break;
                case 1:
                    {
                        if (gv_finish.Items.Count == 0)
                        {
                            var temp = await ContentServ.GetContentAsync(32, 1);
                            foreach (var item in temp)
                            {
                                gv_finish.Items.Add(item);
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        if (gv_now.Items.Count == 0)
                        {
                            var temp = await ContentServ.GetContentAsync(33, 1);
                            foreach (var item in temp)
                            {
                                gv_now.Items.Add(item);
                            }
                        }
                    }
                    break;
                case 3:
                    {
                        if (gv_news.Items.Count == 0)
                        {
                            var temp = await ContentServ.GetContentAsync(51, 1);
                            foreach (var item in temp)
                            {
                                gv_news.Items.Add(item);
                            }
                        }
                    }
                    break;
                case 4:
                    {
                        if (gv_more.Items.Count == 0)
                        {
                            var temp = await ContentServ.GetContentAsync(152, 1);
                            foreach (var item in temp)
                            {
                                gv_more.Items.Add(item);
                            }
                        }
                    }
                    break;
                case 5:
                    {
                        if (gv_china.Items.Count == 0)
                        {
                            var temp = await ContentServ.GetContentAsync(153, 1);
                            foreach (var item in temp)
                            {
                                gv_china.Items.Add(item);
                            }
                        }
                    }
                    break;
            }
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
                    List<Content> temps = await ContentServ.GetContentAsync(int.Parse(gridview.Tag.ToString()), page);
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

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = width.Width = width.Width = WidthFit.GetWidth(ActualWidth, 260, 160);
        }
    }
}
