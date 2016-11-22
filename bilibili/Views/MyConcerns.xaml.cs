using bilibili.Http;
using bilibili.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System;
using bilibili.Methods;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyConcerns : Page
    {
        bool isConcernLoad = false;
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
            if (mid.Length == 0)
            {
                foreach (var item in await ContentServ.GetConAsync(1))
                {
                    conlist.Items.Add(item);
                    conlist2.Items.Add(item);
                }
            }
            else
            {
                foreach (var item in await ContentServ.GetFriendsCons(mid, 1)) 
                {
                    conlist.Items.Add(item);
                    conlist2.Items.Add(item);
                }
            }
            if (conlist.Items.Count < 20)
            {
                var text1 = Load.FindChildOfType<TextBlock>(conlist);
                text1.Text = "加载完毕！";

            }
            con.Text += conlist.Items.Count == 0 ? "（暂无订阅）" : "";
            isConcernLoad = true;
        }

        private void conlist_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Type a = sender.GetType();
            if (a.Name == "GridView") 
            {
                GridView ctrl = sender as GridView;
                var item = ctrl.SelectedItem as Concern;
                if (item != null)
                    Frame.Navigate(typeof(Detail), item.ID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
            else if(a.Name=="ListView")
            {
                ListView ctrl = sender as ListView;
                var item = ctrl.SelectedItem as Concern;
                if (item != null)
                    Frame.Navigate(typeof(Detail), item.ID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }        
        }

        private void Reflesh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               // conlist.ItemsSource = conlist2.ItemsSource = await ContentServ.GetConAsync();

            }
            catch
            {

            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            conlist.Visibility = conlist.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            conlist2.Visibility = conlist2.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        bool isLoading = false;
        private void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            Type type = sender.GetType();
            ListViewBase view;
            if(type.Name=="GridView")
            {
                view = sender as GridView;
            }
            else
            {
                view = sender as ListView;
            }
            var scroll = Load.FindChildOfType<ScrollViewer>(view);
            var text = Load.FindChildOfType<TextBlock>(view);
            scroll.ViewChanged += async (s, a) =>
            {
                if (scroll.VerticalOffset >= scroll.ScrollableHeight - 50 && !isLoading)
                {
                    text.Visibility = Visibility.Visible;
                    int count0 = view.Items.Count;
                    int page = view.Items.Count / 20 + 1;
                    isLoading = true;
                    List<Concern> temps = await ContentServ.GetConAsync(page);
                    text.Visibility = Visibility.Collapsed;
                    foreach (var item in temps)
                    {
                        view.Items.Add(item);
                    }
                    if (temps.Count < 30)
                    {
                        text.Text = "装填完毕！";
                        return;
                    }
                    isLoading = false;
                }
            };
        }
    }
}
