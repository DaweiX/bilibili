using bilibili.Http;
using bilibili.Models;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FavCollection : Page
    {
        public FavCollection()
        {
            this.InitializeComponent();
            load();
        }

        async void load()
        {
            List<Folder> myFolder = await ContentServ.GetFavFolders();
            foreach (var item in myFolder)
            {
                cb_folder.Items.Add(item);
            }
            cb_folder.SelectedIndex = 0;
        }

        private async void cb_folder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Folder folder = cb_folder.SelectedItem as Folder;
            txt_count.Text = folder.Count.ToString() + " / " + folder.MCount.ToString() + "\t创建时间:" + folder.Ctime;
            string fid = folder.Fid;
            //待加：增量加载
            favlist.ItemsSource = await ContentServ.GetFavAsync(fid, 1, 20);
        }

        private void favlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail_P), (e.ClickedItem as Models.Content).Num, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void favlist_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            width.Width = Methods.WidthFit.GetWidth(ActualWidth, 600, 300);
        }
    }
}
