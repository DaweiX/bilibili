using bilibili.Http;
using bilibili.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili.Controls
{
    public sealed partial class Comment : UserControl
    {
        public delegate void MyHandler(string tid);
        public event MyHandler Navi;
        public event MyHandler Info;
        public event MyHandler live;
        public Comment()
        {
            this.InitializeComponent();
        }

        public async void init()
        {
            //直播
            await LoadLive();
            //番剧
            await LoadItems(gridview1, 13, 1);
            //动画
            await LoadItems(gridview2, 1, 1);
            //生活
            await LoadItems(gridview3, 160, 1);
            //电影
            await LoadItems(gridview4, 23, 1);
            //娱乐
            await LoadItems(gridview5, 71, 1);
            //鬼畜
            await LoadItems(gridview6, 119, 1);
            //科技
            await LoadItems(gridview7, 36, 1);
            //游戏
            await LoadItems(gridview8, 4, 1);
            //音乐
            await LoadItems(gridview9, 3, 1);
            //舞蹈
            await LoadItems(gridview10, 20, 1);
            //时尚
            await LoadItems(gridview11, 155, 1);
            //广告
            await LoadItems(gridview12, 166, 1);
            //电视剧
            await LoadItems(gridview13, 11, 1);
        }

        private async Task LoadLive()
        {
            gridview0.ItemsSource = await ContentServ.GetCommentLiveAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private async Task LoadItems(GridView gridview, int tid, int page)
        {
            //int count=
            List<Content> a = await ContentServ.GetContentAsync(tid, page, 6);
            if (a != null)
            {
                gridview.ItemsSource = a;
            }
        }

        private void gridview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GridView gridview = sender as GridView;
            if (gridview.SelectedItem != null)
            {
                Info((gridview.SelectedItem as Content).Num);
            }
        }

        private async void Fresh_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;
            GridView gridview = FindName("gridview" + btn.Tag.ToString()) as GridView;
            await LoadItems(gridview, int.Parse(gridview.Tag.ToString()),new Random().Next(1,50));
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;
            Navi(btn.Content.ToString());
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double i = this.ActualWidth;
            if (i > 800)
            {
                i = 6;
            }
            else if (i > 600)
            {
                i = 4;
            }
            else
            {
                i = 3;
            }
            width.Width = this.ActualWidth / i;
        }

        private void gridview0_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var a = gridview0.SelectedItem as Live;
            if (a != null)
            {
                live(a.PlayUrl);
            }
        }
    }
}
