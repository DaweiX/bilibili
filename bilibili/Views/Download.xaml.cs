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
                    StorageFolder folder = await KnownFolders.VideosLibrary.GetFolderAsync("哔哩哔哩");
                    foreach (StorageFolder folder1 in await folder.GetFoldersAsync())
                    {
                        try
                        {
                            foreach (StorageFile file in await folder1.GetFilesAsync())
                            {
                                var pro = await file.GetBasicPropertiesAsync();
                                if (file != null && file.FileType == ".mp4" && pro.Size != 0) 
                                    donelist.Items.Add(new MyVideo { Part = file.DisplayName, Folder = folder1.DisplayName });
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
                        list_now.Items.Add(new HandleModel
                        {
                            Name = download.ResultFile.Name,
                            DownOpration = download,
                            Progress = (download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * 100,
                            Size = download.Progress.BytesReceived.ToString(),
                        });
                    }
                    foreach (HandleModel model in list_now.Items)
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

        private async Task HandleDownloadAsync(HandleModel model)
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
                HandleModel test = null;
                foreach (HandleModel item in list_now.Items)
                {
                    if (item.DownOpration.Guid == download.Guid)
                    {
                        test = item;
                    }
                }
                if (list_now.Items.Contains(test))
                {
                    ((HandleModel)list_now.Items[list_now.Items.IndexOf(test)]).Size = download.Progress.BytesReceived.ToString();
                    ((HandleModel)list_now.Items[list_now.Items.IndexOf(test)]).Status = download.Progress.Status.ToString();
                    ((HandleModel)list_now.Items[list_now.Items.IndexOf(test)]).Progress = ((double)download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * 100;
                    if ((int)((HandleModel)list_now.Items[list_now.Items.IndexOf(test)]).Progress == 100)
                    {
                        Sendtoast("下载完成", ((HandleModel)list_now.Items[list_now.Items.IndexOf(test)]).Name);
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

        public class HandleModel : INotifyPropertyChanged
        {
            public CancellationTokenSource cts = new CancellationTokenSource();
            public event PropertyChangedEventHandler PropertyChanged;
            protected void thisPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            private DownloadOperation downOpration;
            public DownloadOperation DownOpration
            {
                get { return downOpration; }
                set
                {
                    downOpration = value;
                }
            }
            public string Name { get; set; }
            private double progress;
            public double Progress
            {
                get { return progress; }
                set
                {
                    progress = value;
                    thisPropertyChanged("Progress");
                }
            }

            private string size;
            public string Size
            {
                get { return size; }
                set
                {
                    size = (Convert.ToDouble(value) / 1024 / 1024).ToString("0.0") + "M/" + ((double)downOpration.Progress.TotalBytesToReceive / 1024 / 1024).ToString("0.0") + "M";
                    thisPropertyChanged("Size");
                }
            }
            public string Guid { get { return downOpration.Guid.ToString(); } }
            public string status;
            public string Status
            {
                get { thisPropertyChanged("Status"); return status; }
                set
                {
                    switch (downOpration.Progress.Status)
                    {
                        case BackgroundTransferStatus.Idle:
                            status = "空闲中";
                            break;
                        case BackgroundTransferStatus.Running:
                            status = "下载中";
                            break;
                        case BackgroundTransferStatus.PausedByApplication:
                            status = "已暂停";
                            break;
                        case BackgroundTransferStatus.PausedCostedNetwork:
                            status = "使用数据流量，暂停下载";
                            break;
                        case BackgroundTransferStatus.PausedNoNetwork:
                            status = "网络断开";
                            break;
                        case BackgroundTransferStatus.Completed:
                            status = "完成";
                            break;
                        case BackgroundTransferStatus.Canceled:
                            status = "取消";
                            break;
                        case BackgroundTransferStatus.Error:
                            status = "下载错误";
                            break;
                        case BackgroundTransferStatus.PausedSystemPolicy:
                            status = "因系统资源受限暂停";
                            break;
                    }
                    thisPropertyChanged("Status");
                }
            }
        }

        private void pause_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (list_now.SelectedItems.Count > 0)
                {
                    foreach (HandleModel item in list_now.SelectedItems)
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
                    foreach (HandleModel item in list_now.SelectedItems)
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
                foreach (HandleModel item in list_now.SelectedItems)
                {
                    item.cts.Cancel(false);
                    item.cts.Dispose();
                    try
                    {

                        StorageFolder DowFolder = KnownFolders.VideosLibrary;
                        StorageFile file = await DowFolder.GetFileAsync(item.DownOpration.ResultFile.Name);
                        await file.DeleteAsync(StorageDeleteOption.Default);
                        list_now.Items.RemoveAt(list_now.Items.IndexOf(item));
                    }
                    catch (Exception)
                    {
                        
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
                    foreach (HandleModel item in list_now.SelectedItems)
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

        private void donelist_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(donelist.SelectedItem.ToString()))
            {
                Frame.Navigate(typeof(Video), (MyVideo)donelist.SelectedItem);
            }
        }
    }
}
