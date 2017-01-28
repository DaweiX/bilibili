using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using bilibili.Models;
using Windows.UI.Notifications;
using Windows.Storage.Pickers;
using bilibili.Helpers;
using Windows.System.Display;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Download : Page, IDisposable
    {
        static bool Isdonelistloaded = false;
        static bool Isnowlistloaded = false;
        List<DownloadOperation> activeDownloads;
        CancellationTokenSource cts;
        DisplayRequest displayRq = null;
        public Download()
        {
            cts = new CancellationTokenSource();
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }     

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!Isdonelistloaded)
            {
                try
                {
                    StorageFolder folder = await DownloadHelper.GetMyFolderAsync();
                    foreach (StorageFolder folder1 in await folder.GetFoldersAsync())
                    {
                        try
                        {
                            foreach (StorageFile file in await folder1.GetFilesAsync())
                            {
                                var pro = await file.GetBasicPropertiesAsync();
                                if (file != null && file.FileType == ".mp4" && pro.Size != 0) 
                                    donelist.Items.Add(new LocalVideo { Part = file.DisplayName, Folder = folder1.DisplayName });
                            }
                        }
                        catch { }
                    }
                    Isdonelistloaded = true;
                }
                catch { }            
            }
            if (!Isnowlistloaded)
            {
                await DiscoverDownloadsAsync();
            }        
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (displayRq != null)
            {
                displayRq.RequestRelease();
            }
        }

        private async Task DiscoverDownloadsAsync()
        {
            activeDownloads = new List<DownloadOperation>();
            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                if (downloads.Count > 0)
                {
                    List<Task> tasks = new List<Task>();
                    foreach (DownloadOperation download in downloads)
                    {
                        list_now.Items.Add(new DownloadHelper.DownloadHandler
                        {
                            Name = download.ResultFile.Name,
                            DownOpration = download,
                            Process = (float)((download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * 100),
                            Size = download.Progress.BytesReceived.ToString(),
                        });
                    }
                    foreach (DownloadHelper.DownloadHandler model in list_now.Items)
                    {
                        tasks.Add(HandleDownloadAsync(model));
                    }
                    Isnowlistloaded = true;
                    await Task.WhenAll(tasks);
                }
                else
                {
                    txt.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            catch{ }
        }

        private async Task HandleDownloadAsync(DownloadHelper.DownloadHandler model)
        {
            var download = model.DownOpration;
            try
            {
                DownLoadProgress(download);
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownLoadProgress);
                await download.AttachAsync().AsTask(cts.Token, progressCallback);
            }
            catch (TaskCanceledException)
            {
                messagepop.Show("取消： " + download.Guid);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Dispose()
        {
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
            GC.SuppressFinalize(this);
        }

        private void DownLoadProgress(DownloadOperation download)
        {
            try
            {
                DownloadHelper.DownloadHandler test = null;
                foreach (DownloadHelper.DownloadHandler item in list_now.Items)
                {
                    if (item.DownOpration.Guid == download.Guid)
                    {
                        test = item;
                    }
                }
                if (list_now.Items.Contains(test))
                {
                    ((DownloadHelper.DownloadHandler)list_now.Items[list_now.Items.IndexOf(test)]).Size = download.Progress.BytesReceived.ToString();
                    ((DownloadHelper.DownloadHandler)list_now.Items[list_now.Items.IndexOf(test)]).Status = download.Progress.Status.ToString();
                    ((DownloadHelper.DownloadHandler)list_now.Items[list_now.Items.IndexOf(test)]).Process = (float)(download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * 100;
                    if (download.Progress.BytesReceived == download.Progress.TotalBytesToReceive && download.Progress.BytesReceived > 0) 
                    {
                        Sendtoast("下载完成", ((DownloadHelper.DownloadHandler)list_now.Items[list_now.Items.IndexOf(test)]).Name);
                        list_now.Items.Remove(test);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public class MyProgress: INotifyPropertyChanged
        {
            private int progress;
            public string Name { get; set; }
            public int Progress
            {
                get
                {
                    return progress;
                }
                set
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (list_now.SelectionMode == ListViewSelectionMode.None)
            {
                list_now.IsItemClickEnabled = false;
                list_now.IsMultiSelectCheckBoxEnabled = true;
                list_now.SelectionMode = ListViewSelectionMode.Multiple;
            }
            else if (list_now.SelectionMode == ListViewSelectionMode.Multiple)
            {
                list_now.IsItemClickEnabled = true;
                list_now.IsMultiSelectCheckBoxEnabled = false;
                list_now.SelectionMode = ListViewSelectionMode.None;
            }
        }


        private void Sendtoast(string title, string content)
        {
            var tmp = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var txtNodes = tmp.GetElementsByTagName("text");
            if (!(txtNodes == null || txtNodes.Length == 0))
            {
                var txtnode = txtNodes[0];
                txtnode.InnerText = title + Environment.NewLine + content;
                ToastNotification toast = new ToastNotification(tmp);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
        }

        private void pause_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (list_now.SelectedItems.Count > 0)
                {
                    foreach (DownloadHelper.DownloadHandler item in list_now.SelectedItems)
                    {
                        item.DownOpration.Pause();
                    }
                }
            }
            catch { }
        }

        private void play_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (list_now.SelectedItems.Count > 0)
                {
                    foreach (DownloadHelper.DownloadHandler item in list_now.SelectedItems)
                    {
                        item.DownOpration.Resume();
                    }
                }
            }
            catch { }
        }

        private async void del_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (list_now.SelectedItems.Count != 0)
            {
                foreach (DownloadHelper.DownloadHandler item in list_now.SelectedItems)
                {
                    item.cts.Cancel(false);
                    item.cts.Dispose();
                    try
                    {

                        StorageFolder DowFolder = await DownloadHelper.GetMyFolderAsync();
                        StorageFile file0 = item.DownOpration.ResultFile as StorageFile;
                        StorageFolder folder = await DowFolder.GetFolderAsync(file0.DisplayName);
                        await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        list_now.Items.Remove(item);
                    }
                    catch (Exception err)
                    {
                        string a = err.Message;
                    }
                }
            }
        }

        private void high_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (list_now.SelectedItems.Count > 0)
                {
                    foreach (DownloadHelper.DownloadHandler item in list_now.SelectedItems)
                    {
                        item.DownOpration.Priority = BackgroundTransferPriority.High;
                        //ListViewItem list = list_now.Items[list_now.Items.IndexOf(item)] as ListViewItem;
                        //list.BorderBrush = new SolidColorBrush(Colors.Pink);
                        //list.BorderThickness = new Windows.UI.Xaml.Thickness(4);
                    }
                }
            }
            catch { }
        }

        private async void local_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Frame.Navigate(typeof(Video), file);
            }
            else return;
        }

        private void donelist_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (donelist.SelectionMode == ListViewSelectionMode.Multiple) return;
            LocalVideo mv = e.ClickedItem as LocalVideo;
            if (mv != null)
            {
                Frame.Navigate(typeof(Video), mv);
            }
        }

        private void display_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if ((bool)display.IsChecked)
            {
                displayRq = new DisplayRequest();
                displayRq.RequestActive();
            }
            else
            {
                displayRq.RequestRelease();
            }
        }

        private void Select_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            donelist.SelectionMode = donelist.SelectionMode == ListViewSelectionMode.None ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
        }
    }
}
