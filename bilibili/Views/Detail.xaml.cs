using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using bilibili.Helpers;
using bilibili.Http;
using bilibili.Methods;
using bilibili.Models;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.StartScreen;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Detail : Page
    {
        public delegate void TryLogin();
        public event TryLogin trylogin;
        Season aa = null;
        bool isPinned = false;
        static string cid = string.Empty;
        string sid = string.Empty;
        bool isCollection = false;
        public Detail()
        {
            this.InitializeComponent();
            this.DataContext = this;
            var isdirect = SettingHelper.GetValue("_isdirect");
            if (isdirect != null)
            {
                directly.IsChecked = (bool)isdirect;
            }
            if (!ApiHelper.IsLogin())
            {
                trylogin?.Invoke();//这块可能要改
            }
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            sid = e.Parameter.ToString();
            aa = await ContentServ.GetSeasonResultAsync(sid);
            BitmapImage bmp = new BitmapImage();
            Uri url = new Uri(aa.Cover);
            //var streamReference = RandomAccessStreamReference.CreateFromUri(url);
            //IRandomAccessStream stream = await streamReference.OpenReadAsync();
            //MyImage myimage;
            //if (stream != null)
            //{
            //    BitmapImage img = new BitmapImage(url);
            //    WriteableBitmap bmp = new WriteableBitmap(480, 640);
            //    await bmp.SetSourceAsync(stream);
            //    myimage = new MyImage(bmp);
            //    GaussianBlurFilter filter = new GaussianBlurFilter();
            //    myimage = filter.process(myimage);
            //    pic.Source = myimage.image;
            //}
            bmp.UriSource = url;
            pic.Source = bmp;
            title.Text = aa.Title;
            up.Text = aa.Copyright;
            desc.Text = aa.Brief;
            count.Text = "播放：" + aa.View + "\n" + "收藏：" + aa.Fav + "\n" + "弹幕：" + aa.Danmaku + "\n" + "硬币：" + aa.Coins;
            time.Text = aa.Time;
            staff.Text = aa.Staff ?? string.Empty;
            if (aa.IsConcerned == "1")
            {
                addfav.Icon = new SymbolIcon(Symbol.UnFavorite);
                addfav.Label = "取消订阅";
            }
            foreach (var item in aa.Tags)
            {
                if (item.Length > 0)
                {
                    list_tags.Items.Add(new Tags { Tag = item });
                }
            }
            List<Episodes> ep = aa.EPS;
            if (ep.Count > 0)
            {
                string id_0 = ep[0].ID;
                foreach (var item in ep)
                {
                    if (item.ID == id_0)
                    {
                        continue;
                    }
                    else
                    {
                        foreach (var item1 in ep)
                        {
                            mylist.Items.Add(item1);
                        }
                        break;
                    }
                }
                if (mylist.Items.Count == 0)    //合集
                {
                    isCollection = true;
                    mylist.Items.Add(new Episodes { Title = aa.Title + "(合集)", ID = ep[0].ID, Cover = ep[0].Cover });
                }
            }
            if (SecondaryTile.Exists("tile" + sid)) 
            {
                pin.Icon = new SymbolIcon(Symbol.UnPin);
                pin.Label = "取消固定";
                isPinned = true;
            }
            foreach (var item in aa.CVlist) 
            {
                cvlist.Items.Add(new Actors { Actor = item.Actor, Role = item.Role });
            }
            //if (UserHelper.concernList.FindIndex(o => o.ID == sid) != -1)
            //{
            //    addfav.Icon = new SymbolIcon(Symbol.UnFavorite);
            //    addfav.Label = "取消订阅";
            //}
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
        class Actors
        {
            public string Actor { get; set; }
            public string Role { get; set; }
        }

        async Task GetDanmu()
        {
            string url = "http://comment.bilibili.com/" + cid + ".xml";
            XmlDocument doc = new XmlDocument();
            string txt = await BaseService.SentGetAsync(url);
            doc.LoadXml(txt);
            var a = doc.GetElementsByTagName("d");
            foreach (XmlElement item in a)
            {
                //comm.Items.Add(item.InnerText + item.GetAttribute("p"));
            }
        }

        //保存封面
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeChoices.Add("图片", new List<string>() { ".jpg" });
            picker.SuggestedFileName = title.Text;
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null & aa != null)
            {
                IBuffer buffer = await DownloadHelper.GetBuffer(aa.Cover);
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, buffer);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                messagepop.Show("保存成功", 3000);
            }
        }

        private void MoreDesc_Click(object sender, RoutedEventArgs e)
        {
            desc.MaxLines = desc.MaxLines == 3 ? 0 : 3;
            more.Content = desc.MaxLines == 3 ? "展开" : "收起";
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            switch (item.Tag.ToString())
            {
                case "0":
                    {
                        DataPackage pack = new DataPackage();
                        pack.SetText(string.Format("http://bangumi.bilibili.com/anime/{0}", sid));
                        Clipboard.SetContent(pack);
                        Clipboard.Flush();
                        messagepop.Show("已将链接复制到剪贴板", 3000);
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

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = "来自哔哩哔哩的分享";
            request.Data.Properties.Description = "分享当前番剧";
            //IRandomAccessStreamReference bitmapRef = await new BitmapImage(new Uri(details.Pic));
            request.Data.SetText(string.Format("我在bilibili上正在追番【{0}】\n链接：http://bangumi.bilibili.com/anime/{1}", title.Text, sid));
        }

        private async void fav_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                if (addfav.Label == "订阅")
                {
                    string url = "http://bangumi.bilibili.com/api/concern_season?_device=wp&_ulv=10000&build=424000&platform=android&appkey=422fd9d7289a1dd9&access_key=" + ApiHelper.accesskey + "&ts=" + ApiHelper.GetLinuxTS().ToString() + "&type=bangumi&season_id=" + sid;
                    url += ApiHelper.GetSign(url);
                    JsonObject json = await BaseService.GetJson(url);
                    if (json.ContainsKey("message"))
                    {
                        if (StringDeal.delQuotationmarks(json["message"].ToString()) == "success")
                        {
                            addfav.Icon = new SymbolIcon(Symbol.UnFavorite);
                            addfav.Label = "取消订阅";
                        }
                        else
                            messagepop.Show("订阅失败！" + json["message"].ToString());
                    }
                }
                else if (addfav.Label == "取消订阅")
                {
                    string url = "http://bangumi.bilibili.com/api/unconcern_season?_device=wp&_ulv=10000&build=424000&platform=android&appkey=422fd9d7289a1dd9&access_key=" + ApiHelper.accesskey + "&ts=" + ApiHelper.GetLinuxTS().ToString() + "&type=bangumi&season_id=" + sid;
                    url += ApiHelper.GetSign(url);
                    JsonObject json = await BaseService.GetJson(url);
                    if (json.ContainsKey("message"))
                    {
                        if (StringDeal.delQuotationmarks(json["message"].ToString()) == "success")
                        {
                            addfav.Icon = new SymbolIcon(Symbol.Add);
                            addfav.Label = "订阅";
                        }
                        else
                            messagepop.Show("取消订阅失败！" + json["message"].ToString());
                    }
                }
            }
        }

        private void list_tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Search), (e.ClickedItem as Tags).Tag, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private async void mylist_ItemClick(object sender, ItemClickEventArgs e)
        {
            Episodes ep = e.ClickedItem as Episodes;
            if (directly.IsChecked == true && isCollection == false)
            {
                string url = "http://app.bilibili.com/x/view?_device=android&_ulv=10000&plat=0&build=424000&aid=" + ep.ID + "&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey;
                url += ApiHelper.GetSign(url);
                Details details = await ContentServ.GetDetailsAsync(url);
                List<VideoInfo> list = new List<VideoInfo>();
                list.Add(new VideoInfo { Title = ep.Title, Cid = "0" });
                list.Add(new VideoInfo { Title = ep.Title, Cid = details.Ps[0].Cid });
                Frame.Navigate(typeof(Video),list);
            }
            else
            {
                Frame.Navigate(typeof(Detail_P), ep.ID, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //需要调
            if (this.ActualWidth < 640)
            {
                width.Width = ActualWidth - 32;
            }
            else
            {
                int i = Convert.ToInt32(this.ActualWidth / 400);
                width.Width = (this.ActualWidth / i) - 8 * i - 16;
            }
        }

        private void directly_Click(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_isdirect", (bool)directly.IsChecked);
        }

        private void pic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(pic);
        }

        private async void Pin_Click(object sender, RoutedEventArgs e)
        {
            if (isPinned == false)
            {
                string filename = sid + ".bmp";
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                //using (IRandomAccessStream ss = await file.OpenAsync(FileAccessMode.ReadWrite))
                //{
                //    await RandomAccessStream.CopyAndCloseAsync(pic, ss);
                //}
                //((XmlElement)picNodes[0]).SetAttribute("src", "ms-appdata:///local/pic.bmp");
                IBuffer buffer = await DownloadHelper.GetBuffer(aa.SquareCover);
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, buffer);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                string tileID = "tile" + sid;
                string displayName = aa.Title;
                string args = sid;
                Uri logoUri = new Uri("ms-appdata:///local/" + filename);
                var size = TileSize.Square150x150;
                SecondaryTile tile = new SecondaryTile(tileID, aa.Title, args, logoUri, size);
                tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                tile.VisualElements.Square150x150Logo = logoUri;
                bool isCreated = await tile.RequestCreateAsync();
                if (isCreated)
                {
                    await tile.UpdateAsync();
                    pin.Icon = new SymbolIcon(Symbol.UnPin);
                    pin.Label = "取消固定";
                    await new ContentDialog { Content = "固定成功！", SecondaryButtonText = "确定" }.ShowAsync();
                }
                isPinned = true;
            }
            else
            {
                SecondaryTile tile = new SecondaryTile();
                IReadOnlyList<SecondaryTile> tiles = await SecondaryTile.FindAllAsync();
                foreach (var item in tiles)
                {
                    if (item.Arguments == sid)
                    {
                        tile = item;
                        break;
                    }
                }
                //有趣了，这个“‘从开始屏幕取消固定’浮出控件”压根就没有浮出过
                bool isDeleted = await tile.RequestDeleteAsync();
                if (isDeleted)
                {
                    pin.Icon = new SymbolIcon(Symbol.Pin);
                    pin.Label = "固定磁贴";
                    await new ContentDialog { Content = "已取消固定", SecondaryButtonText = "确定" }.ShowAsync();
                    try
                    {
                        StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(sid + ".bmp");
                        await file.DeleteAsync();
                    }
                    catch { }
                }
                isPinned = false;
            }
        }
    }
}
