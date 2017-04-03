using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using bilibili.Helpers;
using bilibili.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.System.Display;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using static bilibili.Helpers.DownloadHelper;
using System.Linq;
using System.Diagnostics;

//  “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Download : Page, IDisposable
    {
        static bool Isdonelistloaded = false;

        // 用于取消下载操作
        CancellationTokenSource cts = new CancellationTokenSource();

        // 下载任务的集合
        ObservableCollection<TransferModel> transfers = new ObservableCollection<TransferModel>();

        DisplayRequest displayRq = null;
        public Download()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            if (SettingHelper.DeviceType == DeviceType.PC)
            {
                dragarea.Visibility = Visibility.Visible;
            }
        }     

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!Isdonelistloaded)
            {
                try
                {
                    StorageFolder folder = await GetMyFolderAsync();
                    foreach (StorageFolder folder1 in await folder.GetFoldersAsync())
                    {
                        try
                        {
                            foreach (StorageFile file in await folder1.GetFilesAsync())
                            {
                                var pro = await file.GetBasicPropertiesAsync();
                                if (file != null && file.ContentType.Split('/')[0] == "video" && pro.Size != 0)
                                    donelist.Items.Add(new LocalVideo { Part = file.DisplayName, Folder = folder1.DisplayName, Format = file.FileType });
                            }
                        }
                        catch { }
                    }
                    Isdonelistloaded = true;
                }
                catch { }            
            }
            if (list_now.ItemsSource == null)
            {
                list_now.ItemsSource = transfers;
            }
            await DiscoverDownloadsAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (displayRq != null)
            {
                displayRq.RequestRelease();
            }
        }

        /// <summary>
        /// 加载下载任务
        /// </summary>
        private async Task DiscoverDownloadsAsync()
        {
            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                // 不建议在下载中关闭应用.该操作
                // 可能导致下面的语句删除之前已
                // 部分下载的文件.
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                if (downloads.Count > 0)
                {
                    List<Task> tasks = new List<Task>();
                    foreach (DownloadOperation download in downloads)
                    {
                        tasks.Add(HandleDownloadAsync(download, false));
                    }
                    await Task.WhenAll(tasks);
                }
                else
                {
                    txt.Visibility = Visibility.Visible;
                }
            }
            catch{ }
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                string name = download.ResultFile.Name.Split('.')[0];
                TransferModel transfer = new TransferModel
                {
                    DownOpration = download,
                    Size = download.Progress.BytesReceived.ToString(),
                    TotalSize = download.Progress.TotalBytesToReceive.ToString(),
                    Process = 0,
                    Name = name.Remove(name.LastIndexOf('_'))
                };
                transfers.Add(transfer);
                DownloadProgress(download);
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                await download.AttachAsync().AsTask(cts.Token, progressCallback);
            }
            catch (TaskCanceledException)
            {
                messagepop.Show("取消： " + download.Guid);
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

        /// <summary>
        /// 监视下载进度
        /// </summary>
        private void DownloadProgress(DownloadOperation download)
        {
            try
            {
                TransferModel transfer = transfers.First(p => p.DownOpration == download);
                transfer.Size = download.Progress.BytesReceived.ToString();
                transfer.TotalSize = download.Progress.TotalBytesToReceive.ToString();
                transfer.Process = ((double)download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * 100;
                transfer.Status = download.Progress.Status.ToString();
                if (download.Progress.BytesReceived == download.Progress.TotalBytesToReceive && download.Progress.BytesReceived > 0)
                {
                    var a = list_now.Items.ToList();
                    var item = a.Find((p => ((TransferModel)p).DownOpration == download)) as TransferModel;
                    Sendtoast("下载完成", item.Name);
                    transfers.Remove(item);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
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
            // BackgroundTransfer貌似自带成功和失败发送的Toast，也可以用那个
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

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            TransferModel model = (sender as Windows.UI.Xaml.Controls.Primitives.ToggleButton).DataContext as TransferModel;
            if (model.DownOpration.Progress.Status == BackgroundTransferStatus.Running)
            {
                model.DownOpration.Pause();
            }
            else if (model.DownOpration.Progress.Status == BackgroundTransferStatus.PausedByApplication)
            {
                model.DownOpration.Resume();
            }
        }

        private async void Del_Click(object sender, RoutedEventArgs e)
        {
            TransferModel model = (sender as Button).DataContext as TransferModel;
            model.CTS.Cancel(false);
            model.CTS.Dispose();
            try
            {
                // 删除目标文件
                await model.DownOpration.ResultFile.DeleteAsync();
                model.CTS.Dispose();
                transfers.Remove(model);
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
            }
        }

        private void High_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TransferModel model = (sender as Button).DataContext as TransferModel;
                model.DownOpration.Priority = model.DownOpration.Priority == BackgroundTransferPriority.High ?
                    BackgroundTransferPriority.Default : BackgroundTransferPriority.High;
            }
            catch { }
        }

        private async void Local_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };
            picker.FileTypeFilter.Add("");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Frame.Navigate(typeof(Video), file);
            }
            else return;
        }

        private void Donelist_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (donelist.SelectionMode == ListViewSelectionMode.Multiple) return;
            if (e.ClickedItem is LocalVideo mv)
            {
                Frame.Navigate(typeof(Video), mv);
            }
        }

        private void Display_Click(object sender, RoutedEventArgs e)
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

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            donelist.SelectionMode = donelist.SelectionMode == ListViewSelectionMode.None ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
        }

        // 拖动进入目标区域时发生
        private async void Draggrid_DragEnter(object sender, DragEventArgs e)
        {
            var deferral = e.GetDeferral();
            DataPackageView dataview = e.DataView;
            if (dataview.Contains(StandardDataFormats.StorageItems))
            {
                var items = await dataview.GetStorageItemsAsync();
                if (items != null)
                {
                    IStorageItem item = items[0];
                    if (item.IsOfType(StorageItemTypes.File))
                    {
                        e.AcceptedOperation = DataPackageOperation.Link;
                        StorageFile file = (StorageFile)item;
                        // 使用using块避免出现Thumbnail不可用的异常
                        using (StorageItemThumbnail img = await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.VideosView))
                        {
                            if (img != null)
                            {
                                BitmapImage bmp = new BitmapImage()
                                {
                                    DecodePixelWidth = 150
                                };
                                bmp.SetSource(img);
                                e.DragUIOverride.SetContentFromBitmapImage(bmp);
                            }
                        }
                        // e.DragUIOverride.Caption = file.DisplayName;
                        // e.DragUIOverride.IsCaptionVisible = false;    
                        if (file.ContentType.Split('/')[0] == "video")
                        {
                            dragfile = file;
                        }
                    }
                    else
                    {
                        // 不用 DataPackageOperation.None 原因：不是约定的用法
                        deferral.Complete();
                        return;
                    }
                }
            }
            else
            {
                deferral.Complete();
                return;
            }
            // dragarea.Background.Opacity = 0;
            deferral.Complete();
        }

        // 拖动离开目标区域时发生
        private void Draggrid_DragLeave(object sender, DragEventArgs e)
        {
            // AccessViolationException
            var deferral = e.GetDeferral();
            // dragarea.Background.Opacity = 1;
            deferral.Complete();
        }

        StorageFile dragfile;

        // 拖动操作降落时发生
        private void Draggrid_Drop(object sender, DragEventArgs e)
        {
            var deferral = e.GetDeferral();
            if (dragfile != null)
            {
                Frame.Navigate(typeof(Video), dragfile);
            }
            // 不报告完成：电脑假死
            deferral.Complete();
        }
    }
}
