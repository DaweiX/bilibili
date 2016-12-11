using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using bilibili.Helpers;
using bilibili.Http;
using bilibili.Methods;
using bilibili.Models;
using System.IO;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using bilibili.Dialogs;
using Windows.UI.Xaml.Controls.Primitives;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Detail_P : Page
    {
        public delegate void PageNavi(string text);
        public event PageNavi pageNavi;
        Details details = new Details();
        int page = 1;
        static string cid = string.Empty;
        string aid = string.Empty;
        public Detail_P()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ReadyList.Visibility = Visibility.Visible;
            desc.Visibility = Visibility.Visible;
            aid = e.Parameter.ToString();
            string url = "http://app.bilibili.com/x/view?_device=android&_ulv=10000&plat=0&build=424000&aid=" + aid + "&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey;
            url += ApiHelper.GetSign(url);
            details = await ContentServ.GetDetailsAsync(url);
            if (details != null)
            {
                pageNavi("AV" + details.Aid);
                BitmapImage bmp = new BitmapImage();
                bmp.UriSource = new Uri(details.Pic);
                pic.Source = bmp;
                title.Text = details.Title;
                up.Content = details.Upzhu;
                desc.Text = details.Desc;
                count.Text = "播放：" + details.View + "\t" + "收藏：" + details.Fav + "\t" + "弹幕：" + details.Danmu + "\t" + "硬币：" + details.Coins + "\t" + "评论：" + details.Reply;
                time.Text = details.Time;
                if (details.IsFav == "1")
                {
                    btn_addfav.Icon = new SymbolIcon(Symbol.UnFavorite);
                    btn_addfav.Label = "取消收藏";
                }
                foreach (var item in details.Tags)
                {
                    if (item.Length > 0)
                        list_tags.Items.Add(new Tags { Tag = item });
                }
                foreach (var item in details.Ps)
                {
                    ReadyList.Items.Add(item);
                }
                //if (UserHelper.concernList.FindIndex(o => o.ID == cid) != -1)
                //{
                //    btn_addfav.Icon = new SymbolIcon(Symbol.UnFavorite);
                //    btn_addfav.Label = "取消收藏";
                //}
            }
           else
            {
                messagepop.Show("视频不存在或已被删除");
            }
            //if (UserHelper.favList.FindIndex(o => o.Num == aid) != -1)
            //{
            //    btn_addfav.Icon = new SymbolIcon(Symbol.UnFavorite);
            //    btn_addfav.Label = "取消收藏";
            //}
        }

        async Task<bool> load(int page, string aid)
        {
            string url = "http://api.bilibili.com/x/reply?_device=wp&_ulv=10000&build=424000&platform=android&appkey=422fd9d7289a1dd9&oid=" + aid + "&sort=0&type=1&pn=" + page.ToString() + "&ps=20";
            url += ApiHelper.GetSign(url);
            List<Models.Reply> replys = new List<Models.Reply>();
            replys = await ContentServ.GetReplysAsync(url);
            foreach (var item in replys)
            {
                listview.Items.Add(item);
            }
            if (replys.Count < 20)
            {
                return true;
            }
            return false;
        }
        bool isLoading = false;
        private void listview_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var scroll = Load.FindChildOfType<ScrollViewer>(listview);
            var text = Load.FindChildOfType<TextBlock>(listview);
            scroll.ViewChanged += async (s, a) =>
            {
                if (scroll.VerticalOffset == scroll.ScrollableHeight && !isLoading) 
                {   
                    text.Visibility = Visibility.Visible;
                    page++;
                    isLoading = true;
                    bool isDone = await load(page, aid);
                    if (isDone) 
                    {
                        text.Text = "评论装填完毕！";
                        return;
                    }
                    text.Visibility = Visibility.Collapsed;
                    isLoading = false;
                }
            };
        }
        class Tags
        {
            public string Tag { get; set; }
        }
        class Cont
        {
            public string ID { get; set; }
            public string Pic { get; set; }
            public string Title { get; set; }
            public string Index { get; set; }
        }

        private async void ReadyList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ReadyList.SelectionMode != ListViewSelectionMode.Multiple)
            {
                List<VideoInfo> list = new List<VideoInfo>();
                list.Add(new VideoInfo { Title = details.Aid, Cid = ReadyList.SelectedIndex.ToString() });
                foreach (var item in ReadyList.Items)
                {
                    var temp = item as Pages;
                    list.Add(new VideoInfo { Title = temp.Part, Cid = temp.Cid });
                }
                //添加播放历史
                string url_report = "http://api.bilibili.com/x/history/add?_device=wp&_ulv=10000&access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&build=411005&platform=android";
                url_report += ApiHelper.GetSign(url_report);
                string code = await BaseService.SendPostAsync(url_report, "aid=" + aid);
                Frame.Navigate(typeof(Video), list, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }          
        } 

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReadyList.SelectionMode = ListViewSelectionMode.Single;
            btn_addfav.Visibility = btn_like.Visibility = btn_down.Visibility = btn_share.Visibility = Visibility.Visible;
            btn_all.Visibility = btn_ok.Visibility = btn_cal.Visibility = Visibility.Collapsed;
            PivotItem item = (PivotItem)pivot.SelectedItem;
            string tag = item.Header.ToString();
            if (tag == "评论" && listview.Items.Count == 0)
            {
                if (aid.Length > 0)
                {
                    bool isDone = await load(1, aid);
                    if (isDone)
                    {
                        var text = Load.FindChildOfType<TextBlock>(listview);
                        text.Text = "评论装填完毕！";                             
                    }
                }
                else
                {
                    //load(1,season_id)
                }
            }
            if (tag == "相关视频" && list_relates.Items.Count == 0)
            {
                if (aid.Length > 0) 
                {
                    string url = "http://app.bilibili.com/x/view?_device=android&_ulv=10000&plat=0&build=424000&aid=" + aid;
                    list_relates.ItemsSource = await ContentServ.GetRelatesAsync(url);
                }
                else
                {
                    //load(1,season_id)
                }
            }
        }

        private void MoreDesc_Click(object sender, RoutedEventArgs e)
        {
            desc.MaxLines = desc.MaxLines == 3 ? 0 : 3;
            more.Content = desc.MaxLines == 3 ? "展开" : "收起";
        }
        //保存封面
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeChoices.Add("图片", new List<string>() { ".jpg" });
            picker.SuggestedFileName = title.Text;
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                IBuffer buffer = await DownloadHelper.GetBuffer(details.Pic);
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, buffer);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                messagepop.Show("保存成功", 3000);
            }
        }
        /// <summary>
        /// 投币
        /// </summary>
        private async void coin_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem btn = sender as MenuFlyoutItem;
            if (ApiHelper.accesskey.Length > 2 && ApiHelper.IsLogin())  
            {
                try
                {
                    string url = "http://www.bilibili.com/plus/comment.php";
                    string message = "aid=" + aid + "&rating=100&player=1&multiply=" + btn.Tag.ToString() + "&access_key=" + ApiHelper.accesskey;
                    string result = await BaseService.SendPostAsync(url, message);
                    if (result == "OK")
                    {
                        messagepop.Show("投币成功！", 3000);
                    }
                    else
                    {
                        messagepop.Show("投币失败！" + result, 3000);
                    }
                }
                catch (Exception ex)
                {
                    messagepop.Show("错误:" + ex.Message, 3000);
                }
            }
        }
        /// <summary>
        /// 收藏
        /// </summary>
        private async void addfav_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                if (btn_addfav.Label == "收藏")
                {
                    AddFav addfavDialog = new AddFav();
                    await addfavDialog.ShowAsync();
                    if (addfavDialog.Tag != null)
                    {
                        if (addfavDialog.Tag.GetType() == typeof(List<string>))
                        {
                            List<string> fids = addfavDialog.Tag as List<string>;
                            foreach (var fid in fids)
                            {
                                string url = "http://api.bilibili.com/x/app/favourite/video/add?_device=wp&_ulv=10000&build=424000&platform=android&fid=" + fid + "&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey;
                                url += ApiHelper.GetSign(url);
                                string result = await BaseService.SendPostAsync(url, "aid=" + aid);
                                JsonObject json = JsonObject.Parse(result);
                                if (json.ContainsKey("code"))
                                {
                                    if (json["code"].ToString() == "0")
                                    {
                                        btn_addfav.Icon = new SymbolIcon(Symbol.UnFavorite);
                                        btn_addfav.Label = "取消收藏";
                                    }
                                    else
                                        messagepop.Show("收藏失败！" + json["message"].ToString());
                                }
                            }
                        }
                    }                
                }
                else if (btn_addfav.Label == "取消收藏")
                {
                    string url = "http://api.bilibili.com/x/favourite/video/del?_device=android&_ulv=10000&platform=android&build=427000&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey;
                    url += ApiHelper.GetSign(url);
                    string result = await BaseService.SendPostAsync(url, "aid=" + aid);
                    JsonObject json = JsonObject.Parse(result);
                    if (json.ContainsKey("code"))
                    {
                        if (json["code"].ToString() == "0")
                        {
                            btn_addfav.Icon = new SymbolIcon(Symbol.Add);
                            btn_addfav.Label = "收藏";
                        }
                        else
                            messagepop.Show("取消收藏失败！" + json["message"].ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 分享
        /// </summary>
        #region 分享
        private void Share_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            switch (item.Tag.ToString())
            {
                case "0":
                    {
                        DataPackage pack = new DataPackage();
                        pack.SetText(string.Format("http://www.bilibili.com/av{0}", aid));
                        Clipboard.SetContent(pack);
                        Clipboard.Flush();
                        messagepop.Show("已将链接复制到剪贴板", 3000);
                    }break;
                case "1":
                    {
                        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                        dataTransferManager.DataRequested += DataTransferManager_DataRequested;
                        DataTransferManager.ShowShareUI();
                    }
                    break;
            }
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = "来自哔哩哔哩的分享";
            request.Data.Properties.Description = "分享当前视频";
            //IRandomAccessStreamReference bitmapRef = await new BitmapImage(new Uri(details.Pic));
            request.Data.SetText(string.Format("我在bilibili上向你推荐视频【{0}】\n链接：http://www.bilibili.com/av{1}", title.Text, aid));
        }
        #endregion
        async void SendComment(string txt)
        {
            if (ApiHelper.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/reply/add");
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    string Str = "plat=6&jsonp=jsonp&message=" + Uri.EscapeDataString(txt) + "&type=1&oid=" + aid;
                    var response = await client.PostAsync(ReUri, new HttpStringContent(Str, UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JsonObject json = JsonObject.Parse(result);
                    if (json["code"].ToString() == "0")
                    {
                        messagepop.Show("已发送评论!", 3000);
                        listview.Items.Clear();
                        await load(1, aid);
                    }
                    else
                    {
                        messagepop.Show("评论失败\t" + result, 3000);
                    }

                }
                catch (Exception ex)
                {
                    messagepop.Show("评论时发生错误\t" + ex.Message, 3000);
                }
            }
            else
            {
                messagepop.Show("请先登录", 3000);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            SendReply sendDialog = new SendReply();
            sendDialog.Send += SendComment;
            await sendDialog.ShowAsync();
        }

        private void download_Click(object sender, RoutedEventArgs e)
        {
            ReadyList.SelectionMode = ListViewSelectionMode.Multiple;
            btn_addfav.Visibility = btn_like.Visibility = btn_down.Visibility = btn_share.Visibility = Visibility.Collapsed;
            btn_all.Visibility = btn_ok.Visibility =btn_cal.Visibility = Visibility.Visible;
        }

        private async void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (Pages item in ReadyList.SelectedItems)
            {
                try
                {
                    cid = item.Cid;
                    VideoURL url = await ContentServ.GetVedioURL(cid, 2, VideoFormat.mp4);
                    string name = StringDeal.RemoveSpecial(title.Text);
                    string part = StringDeal.RemoveSpecial(item.Part);
                    StorageFolder folder = await KnownFolders.VideosLibrary.CreateFolderAsync("哔哩哔哩", CreationCollisionOption.OpenIfExists);
                    StorageFolder f1 = await folder.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists);
                    var download = await DownloadHelper.Download(url.Url, part + ".mp4", f1);
                    if (SettingHelper.ContainsKey("_downdanmu"))
                    {
                        if ((bool)SettingHelper.GetValue("_downdanmu") == true)
                        {
                            string xml = await BaseService.SentGetAsync("http://comment.bilibili.com/" + cid + ".xml");
                            StorageFile file = await f1.CreateFileAsync(part + ".xml");
                            using (Stream file0 = await file.OpenStreamForWriteAsync())
                            {
                                using (StreamWriter writer = new StreamWriter(file0))
                                {
                                    writer.Write(xml);
                                }
                            }
                        }
                    }
                    else
                    {
                        string xml = await BaseService.SentGetAsync("http://comment.bilibili.com/" + cid + ".xml");
                        StorageFile file = await f1.CreateFileAsync(name + ".xml");
                        using (Stream file0 = await file.OpenStreamForWriteAsync())
                        {
                            using (StreamWriter writer = new StreamWriter(file0))
                            {
                                writer.Write(xml);
                            }
                        }
                    }
                    //如果await，那么执行完第一个StartAsync()后即退出循环.GetCurrentDownloadsAsync()方法同样会遇到此问题.(Download页)
                    IAsyncOperationWithProgress<DownloadOperation, DownloadOperation> start = download.StartAsync();
                    i++;
                    messagepop.Show(i.ToString() + "个视频已加入下载队列");
                }
                catch (Exception err)
                {
                    messagepop.Show(err.Message);
                }
            }         
        }

        private void btn_all_Click(object sender, RoutedEventArgs e)
        {
            ReadyList.SelectAll();
        }

        private void btn_cal_Click(object sender, RoutedEventArgs e)
        {
            ReadyList.SelectionMode = ListViewSelectionMode.Single;
            btn_addfav.Visibility = btn_like.Visibility = btn_down.Visibility = btn_share.Visibility = Visibility.Visible;
            btn_all.Visibility = btn_ok.Visibility = btn_cal.Visibility = Visibility.Collapsed;
        }

        private void list_tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Search), (e.ClickedItem as Tags).Tag, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void list_relates_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail_P), (e.ClickedItem as Basic).ID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void up_Click(object sender, RoutedEventArgs e)
        {
            if (details.Mid.Length > 0)
            {
                Frame.Navigate(typeof(Friends), details.Mid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }

        private void pic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(pic);
        }
    }
}
