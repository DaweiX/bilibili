using bilibili.Http;
using bilibili.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace bilibili.Dialogs
{
    public sealed partial class AddFav : ContentDialog
    {
        public AddFav()
        {
            this.InitializeComponent();
            load();
        }

        async void load()
        {
            list.ItemsSource = await ContentServ.GetFavFolders();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            List<string> fids = new List<string>();
            foreach (Folder item in list.SelectedItems)
            {
                fids.Add(item.Fid);
            }
            this.Tag = fids;
        }
    }
}
