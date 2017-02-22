using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using bilibili.Animation.Root;
using bilibili.Dialogs;
using bilibili.Helpers;
using bilibili.Http;
using bilibili.Methods;
using bilibili.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

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
        bool isReply = false;
        string quality = string.Empty;
        VideoFormat format;
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
            try
            {
                base.OnNavigatedTo(e);
                SwitchCommandBar(false);
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
                    pic_blur.Source = bmp;
                    if (AnimationExtensions.IsBlurSupported)
                    {
                        pic_blur.Blur(duration: 3000, value: 20).Start();
                    }
                    title.Text = details.Title;
                    up.Content = details.Upzhu;
                    desc.Text = details.Desc;
                    c_play.Text = "播放：" + details.View + '\t';
                    c_fav.Text = "收藏：" + details.Fav + '\t';
                    c_danmaku.Text = "弹幕：" + details.Danmu + '\t';
                    c_coin.Text = "硬币：" + details.Coins + '\t';
                    c_comment.Text = "评论：" + details.Reply + '\t';
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
                    if (!string.IsNullOrEmpty(details.Sid))
                    {
                        bangumi.Content = details.BangumiTitle;
                        bangumi.Click += Bangumi_Click;
                        stk_bangumi.Visibility = Visibility.Visible;
                    }
                    //if (UserHelper.concernList.FindIndex(o => o.ID == cid) != -1)
                    //{
                    //    btn_addfav.Icon = new SymbolIcon(Symbol.UnFavorite);
                    //    btn_addfav.Label = "取消收藏";
                    //}
                    if (SettingHelper.ContainsKey("_quality"))
                    {
                        (FindName("q" + SettingHelper.GetValue("_quality").ToString()) as RadioButton).IsChecked = true;
                    }
                    if (SettingHelper.ContainsKey("_videoformat"))
                    {
                        (FindName("f" + SettingHelper.GetValue("_videoformat").ToString()) as RadioButton).IsChecked = true;
                    }
                }
                else
                {
                    await popup.Show("视频不存在或已被删除");
                }
            }
            catch 
            {
                await popup.Show("加载失败啦~");
            }           
        }

        private void Bangumi_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Detail), details.Sid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        async Task<bool> load(int page, string aid)
        {
            string url = "http://api.bilibili.com/x/reply?_device=wp&_ulv=10000&build=424000&platform=android&appkey=" + ApiHelper.appkey + "&oid=" + aid + "&sort=0&type=1&pn=" + page.ToString() + "&ps=20";
            url += ApiHelper.GetSign(url);
            List<Reply> replys = new List<Reply>();
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
                    if (isDone && text != null)  
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

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem item = (PivotItem)pivot.SelectedItem;
            string tag = item.Header.ToString();
            if (tag == "评论" && listview.Items.Count == 0)
            {
                if (aid.Length > 0)
                {
                    var text = Load.FindChildOfType<TextBlock>(listview);
                    bool isDone = await load(1, aid);
                    if (isDone && text != null) 
                    {
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
                     list_relates.ItemsSource = await ContentServ.GetRelatesAsync(aid);
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
                await popup.Show("保存成功！");
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
                        await popup.Show("投币成功！");
                    }
                    else
                    {
                        await popup.Show("投币失败！" + result);
                    }
                }
                catch (Exception ex)
                {
                    await popup.Show("错误:" + ex.Message);
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
                                        await popup.Show("收藏失败！" + json["message"].ToString());
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
                        {
                            await popup.Show("取消收藏失败！" + json["message"].ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 分享
        /// </summary>
        #region 分享
        private async void Share_Click(object sender, RoutedEventArgs e)
        {
            string url = "http://app.bilibili.com/x/v2/view/share/add";
            string args = "access_key=" + ApiHelper.accesskey + "&aid=" + aid + "&appkey=" + ApiHelper.appkey + "&build=431001&mobi_app=android&platform=android&ts=" + ApiHelper.GetLinuxTS().ToString();
            args += ApiHelper.GetSign(url + "?" + args);
            try
            {
                string result = await BaseService.SendPostAsync(url, args, "http://app.bilibili.com");
                JsonObject json = JsonObject.Parse(result);
            }
            catch
            {

            }
            finally
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
                            await popup.Show("已将链接复制到剪贴板");
                        }
                        break;
                    case "1":
                        {
                            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
                            DataTransferManager.ShowShareUI();
                        }
                        break;
                }
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
        async Task SendComment(string txt, bool isRep)
        {
            if (ApiHelper.IsLogin())
            {
                try
                {
                    //发送新评论
                    if (isRep == false)
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
                            txt_main.Text = string.Empty;
                            await RefreshReply();
                        }
                        else
                        {
                            await popup.Show("评论失败:" + result);
                        }
                    }
                    //回复评论
                    else
                    {
                        try
                        {
                            string url = "http://api.bilibili.com/x/reply/add?_device=wp&build=429001&platform=android&scale=xhdpi&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey;
                            url += ApiHelper.GetSign(url);
                            string Args = "parent=" + reply.Parent + "&root=" + reply.Root + "&rpid=" + reply.Rpid + "&oid=" + reply.Oid + "&plat=4&type=1&message=" + "回复 @" + reply.Uname + " :" + txt;
                            JsonObject json = JsonObject.Parse(await BaseService.SendPostAsync(url, Args, "http://app.bilibili.com"));
                            if (json["code"].ToString() == "0")
                            {
                                isReply = false;
                                stk_reply.Visibility = Visibility.Collapsed;
                                txt_main.Text = string.Empty;
                                await RefreshReply();
                            }
                        }
                        catch (Exception)
                        {

                        }                      
                    }

                }
                catch (Exception ex)
                {
                    await popup.Show("评论时发生错误:" + ex.Message);
                }
            }
            else
            {
                await popup.Show("请先登录");
            }
        }

        private async Task RefreshReply()
        {
            listview.Items.Clear();
            var text = Load.FindChildOfType<TextBlock>(listview);
            bool isDone = await load(1, aid);
            if (isDone && text != null)
            {
                text.Text = "评论装填完毕！";
            }
            page = 1;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await SendComment(txt_main.Text, isReply);
        }

        private void download_Click(object sender, RoutedEventArgs e)
        {
            SwitchCommandBar(true);
        }

        private void SwitchCommandBar(bool isDownload)
        {
            ReadyList.SelectionMode = isDownload ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            btn_addfav.Visibility = btn_like.Visibility = btn_down.Visibility = btn_share.Visibility = isDownload ? Visibility.Collapsed : Visibility.Visible;
            btn_all.Visibility = btn_ok.Visibility = btn_cal.Visibility = isDownload ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            flyout_download.Hide();
            foreach (Pages item in ReadyList.SelectedItems)
            {
                try
                {
                    cid = item.Cid;
                    VideoURL url = await ContentServ.GetVedioURL(cid, quality, format);
                    string name = StringDeal.RemoveSpecial(title.Text);
                    string part = StringDeal.RemoveSpecial(item.Part);
                    StorageFolder folder = await DownloadHelper.GetMyFolderAsync();
                    StorageFolder f1 = await folder.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists);                   
                    var download = await DownloadHelper.Download(url.Url, part + ".mp4", f1);
                    //如果await，那么执行完第一个StartAsync()后即退出循环.GetCurrentDownloadsAsync()方法同样会遇到此问题.(Download页)
                    IAsyncOperationWithProgress<DownloadOperation, DownloadOperation> start = download.StartAsync();
                    i++;
                    await popup.Show(i.ToString() + "个视频已加入下载队列");
                    if (SettingHelper.ContainsKey("_downdanmu"))
                    {
                        if ((bool)SettingHelper.GetValue("_downdanmu") == false)
                        {
                            continue;
                        }
                    }
                    await DownloadHelper.DownloadDanmakuAsync(cid, part, f1);
                }
                catch (Exception err)
                {
                    await popup.Show(err.Message);
                }
            }
            SwitchCommandBar(false);
        }

        private void btn_all_Click(object sender, RoutedEventArgs e)
        {
            ReadyList.SelectAll();
        }

        private void btn_cal_Click(object sender, RoutedEventArgs e)
        {
            SwitchCommandBar(false);
        }

        private void list_tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Search), (e.ClickedItem as Tags).Tag, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void list_relates_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail_P), (e.ClickedItem as RelateVideo).ID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
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


        Reply reply = new Reply();
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            flyout.ShowAt(listview, e.GetPosition(listview));
            var r = (sender as Grid).DataContext as Reply;
            reply = r;
        }

        private async void like_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = "http://api.bilibili.com/x/reply/action";
                JsonObject json = JsonObject.Parse(await BaseService.SendPostAsync(url, "jsonp=jsonp&oid=" + reply.Oid + "&type=1&rpid=" + reply.Rpid + "&action=1"));
                if (json.ContainsKey("code"))
                {
                    if (json["code"].ToString() == "0")
                    {
                        await popup.Show("赞同成功");
                    }
                }
            }
            catch(Exception err)
            {
                await popup.Show("失败：" + err.Message);
            }
        }

        private void Space_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(reply.Mid))
            {
                Frame.Navigate(typeof(Friends), reply.Mid, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo());
            }
        }

        private void reply_Click(object sender, RoutedEventArgs e)
        {
            txt_reply.Text = "回复    " + reply.Uname + ":";
            isReply = true;
            stk_reply.Visibility = Visibility.Visible;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = WidthFit.GetWidth(ActualWidth);
        }

        private void ReplyBack_Click(object sender, RoutedEventArgs e)
        {
            isReply = false;
            stk_reply.Visibility = Visibility.Collapsed;
        }

        private void quality_Checked(object sender, RoutedEventArgs e)
        {
            quality = (sender as RadioButton).Tag.ToString();
        }

        private void format_Checked(object sender, RoutedEventArgs e)
        {
            int arg = int.Parse((sender as RadioButton).Tag.ToString());
            if (arg == 0) format = VideoFormat.mp4;
            else if (arg == 1) format = VideoFormat.flv;
        }

        //列表改成ItemClick后SelectedIndex恒为-1，故此处使用Tapped事件
        private async void ReadyList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ReadyList.SelectionMode == ListViewSelectionMode.Single && ReadyList.SelectedIndex > -1) 
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
                await BaseService.SendPostAsync(url_report, "aid=" + aid);
                Frame.Navigate(typeof(Video), list, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }
    }
}
