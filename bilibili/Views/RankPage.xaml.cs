using bilibili.Helpers;
using bilibili.Http;
using bilibili.Models;
using bilibili.UI;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RankPage : Page
    {
        public RankPage()
        {
            this.InitializeComponent();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = Methods.WidthFit.GetWidth(ActualWidth, 600, 400);
        }

        private void listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Rank rank = e.ClickedItem as Rank;
            if (rank != null)
            {
                Frame.Navigate(typeof(Detail_P), rank.Aid, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
            }
        }

        private async void mainpivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tid= (mainpivot.SelectedItem as PivotItem).Tag.ToString();
            GridView gridview = FindName("list" + tid) as GridView;
            if (gridview != null && gridview.Items.Count == 0)
            {
                gridview.ItemsSource = await ContentServ.GetRankItemsAsync(tid);
            }
        }
    }
    public class NumToForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = int.Parse(value.ToString());
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = b < 4 ? ColorRelated.GetColor() : SettingHelper.GetValue("_nighttheme").ToString() == "light" ? Colors.Black : Colors.White;
            return brush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
