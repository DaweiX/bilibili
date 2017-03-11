using bilibili.Http;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
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
                // 获取文件夹权限（可选）
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
                DownloadOperation download = downloader.CreateDownload(new Uri(url), file);
                if (SettingHelper.ContainsKey("_downloadcost"))
                {
                    if (SettingHelper.GetValue("_downloadcost").ToString() == "wifionly")
                    {
                        download.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
                    }
                }
                return download;
            }
            catch (Exception ex)
            {
                var a = ex.Message;
                return null;
                // WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                // MessageDialog md = new MessageDialog(ex.Message);
                // await md.ShowAsync();
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
            public DownloadOperation DownOpration
            {
                get { return mydownload; }
                set { mydownload = value; }
            }
            public double Process { get; set; }
            string size;
            public string Size
            {
                get { return size; }
                set
                {
                    size = (Convert.ToDouble(value) / Math.Pow(1024, 2)).ToString("0.0") + "M";
                    MyPropertyChanged("Size");
                }
            }
            public string Name { get; set; }
            public string TotalSize
            {
                get { return (mydownload.Progress.TotalBytesToReceive / Math.Pow(1024, 2)).ToString("0.0") + "M"; }
            }
            public string Guid { get { return mydownload.Guid.ToString(); } }
            string status;
            public string Status
            {
                get { return status; }
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
                            status = "等待...";
                            break;
                    }
                    MyPropertyChanged("Status");
                }
            }
        }
       /// <summary>
       /// 获取存储文件夹
       /// </summary>
        public async static Task<StorageFolder> GetMyFolderAsync()
        {
            StorageFolder folder = null;
            string path = string.Empty;
            if (SettingHelper.ContainsKey("_path"))
            {
                path = SettingHelper.GetValue("_path").ToString();
            }
            StorageFolder defaultfolder = await KnownFolders.VideosLibrary.CreateFolderAsync("哔哩哔哩", CreationCollisionOption.OpenIfExists);
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    folder = await StorageFolder.GetFolderFromPathAsync(path);
                }
                catch
                {
                    folder = defaultfolder;
                }
            }
            else
            {
                folder = defaultfolder;
            }
            return folder;
        }

       /// <summary>
       /// 下载弹幕文档
       /// </summary>
        public async static Task DownloadDanmakuAsync(string cid, string name, StorageFolder folder)
        {
            string xml = await BaseService.SentGetAsync("http://comment.bilibili.com/" + cid + ".xml?rnd=" + new Random().Next(500, 1000));
            StorageFile file = await folder.CreateFileAsync(name + ".xml");
            using (Stream file0 = await file.OpenStreamForWriteAsync())
            {
                using (StreamWriter writer = new StreamWriter(file0))
                {
                    await writer.WriteAsync(xml);
                }
            }
        }

        public async static Task AddVideoInfo(string title, string cid, string sid = null)
        {
            JsonObject json = new JsonObject();
            JsonObject jsonAppend = new JsonObject
            {
                { title, new JsonObject
                    {
                        { "cid", JsonValue.CreateStringValue(cid) },
                        { "sid", sid == null 
                                      ? JsonValue.CreateNullValue()
                                      : JsonValue.CreateStringValue(sid) },
                    }
                }
            };
            StorageFile file = await KnownFolders.VideosLibrary.CreateFileAsync("list.json", CreationCollisionOption.OpenIfExists);
            using (Stream file0 = await file.OpenStreamForReadAsync())
            {
                StreamReader reader = new StreamReader(file0);
                string txt = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(txt))
                {
                    json.Add("data", new JsonArray());
                    json.Add("mid", UserHelper.Mid == null 
                                                    ? JsonValue.CreateNullValue()
                                                    : JsonValue.CreateStringValue(UserHelper.Mid));
                    json.Add("count", JsonValue.CreateNumberValue(0));
                }
                else
                {
                    json = JsonObject.Parse(txt);
                }
                JsonArray array = json["data"].GetArray();
                array.Add(jsonAppend);
                json["count"] = JsonValue.CreateNumberValue(json["count"].GetNumber() + 1);
            }
            using (Stream file1 = await file.OpenStreamForWriteAsync())
            {
                using (StreamWriter writer = new StreamWriter(file1))
                {
                    await writer.WriteAsync(json.ToString());
                }
            }
        }
    }
}
