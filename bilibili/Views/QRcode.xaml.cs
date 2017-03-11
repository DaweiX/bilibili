using System.Collections.Generic;
using System.Net;
using bilibili.Helpers;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.Storage.Provider;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls.Primitives;

//  “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
   /// <summary>
   /// 可用于自身或导航至 Frame 内部的空白页。
   /// </summary>
    public sealed partial class QRcode : Page
    {
        string url = string.Empty;
        public QRcode()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            uname.Text = UserHelper.Uname;
            string color = string.Empty;
            switch (SettingHelper.GetValue("_Theme").ToString())
            {
                case "Pink":
                    color = "E273A9";
                    break;
                case "Red":
                    color = "F44336";
                    break;
                case "Yellow":
                    color = "FFB007";
                    break;
                case "Green":
                    color = "7BB33A";
                    break;
                case "Blue":
                    color = "18BDFB";
                    break;
                case "Purple":
                    color = "B92CBF";
                    break;
                case "Orange":
                    color = "FF6633";
                    break;
                default:
                    color = "000000";break;
            }
            url = "http://qr.liantu.com/api.php?w=500&text=" + WebUtility.UrlEncode("http://space.bilibili.com/" + UserHelper.Mid) + "&inpt=" + color + "&logo=" + UserHelper.Face;
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri(url);
            img.Source = bmp;
        }

        private async void Save_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeChoices.Add("图片", new List<string>() { ".jpg" });
            picker.SuggestedFileName = UserHelper.Uname + "的bilibili二维码";
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                IBuffer buffer = await DownloadHelper.GetBuffer(url);
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, buffer);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            }
        }

        private void img_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(img);
        }
    }
}
