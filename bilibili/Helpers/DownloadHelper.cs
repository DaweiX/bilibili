using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace bilibili.Helpers
{
    class DownloadHelper
    {
        /// <summary>
        /// 将网址result转换为字节流
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static public async Task<IBuffer> GetBuffer(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage message = await client.GetAsync(new Uri(url));
                message.EnsureSuccessStatusCode();
                IBuffer buff = await message.Content.ReadAsBufferAsync();
                return buff;
            }
        }
        /// <summary>
        /// 返回指定参数的下载操作
        /// </summary>
        /// <param name="url">请求到的下载地址</param>
        /// <param name="name">要下载的文件名</param>
        /// <returns></returns>
        static public async Task<DownloadOperation> Download(string url, string name, StorageFolder folder)
        {
            try
            {
                BackgroundDownloader downloader = new BackgroundDownloader();
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(KnownFolders.VideosLibrary);
                StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
                DownloadOperation download = downloader.CreateDownload(new Uri(url), file);
                if (SettingHelper.ContainsKey("_downloadcost"))
                {
                    if (SettingHelper.GetValue("_downloadcost").ToString() == "wifionly")
                    {
                        download.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
                    }
                    else if (SettingHelper.GetValue("_downloadcost").ToString() == "wifidata") 
                    {
                        download.CostPolicy = BackgroundTransferCostPolicy.Default;
                    }
                }
                return download;
            }
            catch (Exception ex)
            {
                var a = ex.Message;
                return null;
                //WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                //MessageDialog md = new MessageDialog(ex.Message);
                //await md.ShowAsync();
            }
        }
        /// <summary>
        /// Handler
        /// </summary>
        public class DownloadHandler:INotifyPropertyChanged
        {
            public CancellationTokenSource cts = new CancellationTokenSource();
            public event PropertyChangedEventHandler PropertyChanged;
            void MyPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            DownloadOperation mydownload;
            public DownloadOperation MyDownload
            {
                get { return mydownload; }
                set { mydownload = value; }
            }
            double process;
            public double Process
            {
                get { return process; }
                set { process = value; }
            }
            string size;
            public string Size
            {
                get { return size; }
                set
                {
                    size = (Convert.ToDouble(value) / Math.Pow(1024, 2)).ToString("0.0") + "M/t/" + (Convert.ToDouble(mydownload.Progress.TotalBytesToReceive) / Math.Pow(1024, 2)).ToString("0.0") + "M";
                    MyPropertyChanged("Size");
                }
            }
            public string Guid { get { return mydownload.Guid.ToString(); } }
            string status;
            public string Status
            {
                get
                {
                    //MyPropertyChanged("Status");
                    return status;
                }
                set
                {
                    switch(mydownload.Progress.Status)
                    {
                        case BackgroundTransferStatus.Idle:
                            status = "空闲中";
                            break;
                        case BackgroundTransferStatus.Running:
                            status = "下载中";
                            break;
                        case BackgroundTransferStatus.PausedByApplication:
                            status = "暂停中";
                            break;
                        case BackgroundTransferStatus.PausedCostedNetwork:
                            status = "因网络暂停";
                            break;
                        case BackgroundTransferStatus.PausedNoNetwork:
                            status = "没有连接至网络";
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
                            status = "因系统问题暂停";
                            break;
                        default:
                            status = "Wait...";
                            break;
                    }
                    MyPropertyChanged("Status");
                }
            }
        }
    }
}
