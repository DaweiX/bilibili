using System;
using bilibili.Http;
using bilibili.Models;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FavCollection : Page
    {
        string name = string.Empty;
        Folder folder = new Folder();
        string fid = string.Empty;
        public FavCollection()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (!string.IsNullOrEmpty(e.Parameter.ToString()))
                {
                    name = e.Parameter.ToString();
                }
            }
            load();
        }

        async void load()
        {
            cb_folder.Items.Clear();
            List<Folder> myFolder = await ContentServ.GetFavFolders();
            foreach (var item in myFolder)
            {
                cb_folder.Items.Add(item);
            }
            try
            {
                if (string.IsNullOrEmpty(name))
                    cb_folder.SelectedIndex = 0;
                else
                    cb_folder.SelectedIndex = myFolder.FindIndex(o => o.Name == name);
            }
            catch { cb_folder.SelectedIndex = -1; }
        }
        //0:公开
        private async void cb_folder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            folder = cb_folder.SelectedItem as Folder;
            if (folder != null)
            {
                name = folder.Name;
                togpublic.Toggled -= togpublic_Toggled;
                togpublic.IsOn = folder.State == "0"|| folder.State == "2" ? true : false;
                togpublic.Toggled += togpublic_Toggled;
                fid = folder.Fid;
                txt_count.Text = folder.Count.ToString() + " / " + folder.MCount.ToString() + "\t创建时间:" + folder.Ctime;
                //待加：增量加载
                favlist.ItemsSource = await ContentServ.GetFavAsync(fid, 1, 20);
                return;
            }
        }

        private void favlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail_P), (e.ClickedItem as Content).Num, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void favlist_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            width.Width = Methods.WidthFit.GetWidth(ActualWidth, 600, 300);
        }

        private async void rename_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(fid) && !string.IsNullOrWhiteSpace(txt_name.Text)) 
                {
                    string url = "http://space.bilibili.com/ajax/fav/renameBox";
                    string Args = "fav_box=" + fid + "&new_name=" + txt_name.Text;
                    JsonObject json = JsonObject.Parse(await BaseService.SendPostAsync(url, Args, "http://space.bilibili.com"));
                    if (bool.Parse(json["status"].ToString()))
                    {
                        txt_name.Text = string.Empty;
                        flyout.Hide();
                        load();
                    }
                }      
                else
                {
                    pop.Show("内容无效");
                    flyout.Hide();
                }         
            }
            catch
            {
                //重命名失败
            }           
        }

        private async void delete_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new Dialogs.DelFavFolder { Content = string.Format("确定要删除收藏夹“{0}”", folder.Name) };
            await dialog.ShowAsync();
            if ((bool)dialog.Tag)
            {
                string url = "http://space.bilibili.com/ajax/fav/delBox";
                string Args = "fav_box=" + fid;
                JsonObject json = JsonObject.Parse(await BaseService.SendPostAsync(url, Args, "http://space.bilibili.com"));
                if (bool.Parse(json["status"].ToString()))
                {
                    name = string.Empty;
                    load();
                }
            }            
        }

        private void refresh_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            load();
        }

        private void flyout_Opened(object sender, object e)
        {
            txt_name.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }

        //Toggged事件会在不受用户控制的情况下调用,Tapped又用不了，233
        private async void togpublic_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(fid))
            {
                string url = "http://space.bilibili.com/ajax/fav/setBoxPublic";
                string Args = "fav_box=" + fid + "&public=" + (togpublic.IsOn ? "0" : "1");
                JsonObject json = JsonObject.Parse(await BaseService.SendPostAsync(url, Args, "http://space.bilibili.com"));
                if (bool.Parse(json["status"].ToString()))
                {
                    load();
                }
            }
        }

        private void flyout2_Opened(object sender, object e)
        {
            txt_new.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }

        private async void new_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txt_new.Text))
                {
                    string url = "http://space.bilibili.com/ajax/fav/CreateBox";
                    string Args = "name=" + txt_new.Text;
                    JsonObject json = JsonObject.Parse(await BaseService.SendPostAsync(url, Args, "http://space.bilibili.com"));
                    if (bool.Parse(json["status"].ToString()))
                    {
                        txt_new.Text = string.Empty;
                        flyout2.Hide();
                        load();
                    }
                }
                else
                {
                    pop.Show("收藏夹得有个名字哟！");
                    flyout2.Hide();
                }
            }
            catch
            {
                //新建失败
            }
        }
    }
}
