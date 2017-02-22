using bilibili.Helpers;
using bilibili.Http;
using bilibili.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Devices.Input;
using Windows.Devices.Power;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static bilibili.Controls.Danmaku;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Video : Page
    {
        VideoURL URL = null;
        bool isX = false;
        DispatcherTimer timer_danmaku = new DispatcherTimer();
        DispatcherTimer timer_repeat = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        DispatcherTimer timer = new DispatcherTimer();
        DisplayRequest displayRq = new DisplayRequest();
        string part = string.Empty;
        string folder = string.Empty;
        string quality = "2";
        VideoFormat format = VideoFormat.mp4;
        string cid = string.Empty;
        string aid = string.Empty;
        int R_1, R_2;
        string danmakuMode = "1";
        bool? isRepeat = null;
        List<VideoInfo> infos = new List<VideoInfo>();
        StorageFile file = null;
        List<string> strs = new List<string>();
        bool isHideDanmu = false;
        List<DanmuModel> DanmuPool;
        bool isInited = false;
        bool? isLocal = null;
        bool isMouseMoving = false;
        DeviceType type;
        int Index = 0;
        bool isPropInit = false;
        bool lightinit = false;
        public Video()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            isInited = true;
            this.DataContext = this;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer_danmaku.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer_repeat.Tick += Timer_repeat_Tick;
            timer_danmaku.Tick += Timer_danmaku_Tick;
            timer.Start();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape; //横向屏幕
            if (!WebStatusHelper.IsOnline())
            {
                txt_mydanmu.PlaceholderText = "没有联网哦，不能发弹幕";
                txt_mydanmu.IsEnabled = false;
            }
            displayRq.RequestActive();      //保持屏幕常亮
            if (SettingHelper.DeviceType == DeviceType.PC)
            {
                menu_full.Visibility = Visibility.Visible;
                MouseDevice.GetForCurrentView().MouseMoved += Video_MouseMoved;
                CoreWindow.GetForCurrentThread().KeyDown += Video_KeyDown;
            }
            SettingInit();
        }

        private void SettingInit()
        {
            if (SettingHelper.ContainsKey("_autofull"))
            {
                if ((bool)SettingHelper.GetValue("_autofull") == true)
                {
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                }
            }
            else
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                SettingHelper.SetValue("_autofull", true);
            }
            if (SettingHelper.ContainsKey("_vol"))
            {
                sli_vol.Value = Convert.ToInt32(SettingHelper.GetValue("_vol"));
            }
            if (SettingHelper.ContainsKey("_light"))
            {
                sli_light.Value = double.Parse(SettingHelper.GetValue("_light").ToString());
            }
            else
            {
                sli_light.Value = 1;
            }
            lightinit = true;
            border.Opacity = 1 - sli_light.Value;
            if (SettingHelper.ContainsKey("_space"))
            {
                sli_space.Value = int.Parse(SettingHelper.GetValue("_space").ToString());
            }
            if (SettingHelper.ContainsKey("_speed"))
            {
                sli_speed.Value = int.Parse(SettingHelper.GetValue("_speed").ToString());
            }
            else
            {
                sli_space.Value = 8;
            }
            if (SettingHelper.ContainsKey("_fontsize"))
            {
                sli_fontsize.Value = int.Parse(SettingHelper.GetValue("_fontsize").ToString());
            }
            else
            {
                sli_fontsize.Value = 1;
            }
            cb_font.Items.Add("默认");
            cb_font.Items.Add("宋体");
            cb_font.Items.Add("等线");
            cb_font.Items.Add("楷体");
            if (SettingHelper.DeviceType == DeviceType.PC)
            {
                cb_font.Items.Add("幼圆");
            }
            if (SettingHelper.ContainsKey("_danmufont"))
            {
                cb_font.SelectedIndex = int.Parse(SettingHelper.GetValue("_danmufont").ToString());
            }
            if (SettingHelper.ContainsKey("_quality"))
            {
                quality = SettingHelper.GetValue("_quality").ToString();
            }
            if (SettingHelper.ContainsKey("_isdanmaku"))
            {
                if ((bool)SettingHelper.GetValue("_isdanmaku")) 
                {
                    kill_all.IsChecked = true;
                    isHideDanmu = true;
                    stk.Visibility = Visibility.Collapsed;
                }
            }
            cb_quality.SelectedIndex = 1;
            cb_format.SelectedIndex = 0;
            if (SettingHelper.ContainsKey("_quality"))
            {
                cb_quality.SelectedIndex = int.Parse(SettingHelper.GetValue("_quality").ToString()) - 1;
            }
            if (SettingHelper.ContainsKey("_videoformat"))
            {
                cb_format.SelectedIndex = int.Parse(SettingHelper.GetValue("_videoformat").ToString());
            }
        }

        private void Video_MouseMoved(MouseDevice sender, MouseEventArgs args)
        {
            isMouseMoving = true;
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            isMouseMoving = false;
        }

        private void Timer_repeat_Tick(object sender, object e)
        {
            if ((int)media.Position.TotalMilliseconds >= R_2)
            {
                media.Position = TimeSpan.FromMilliseconds(R_1);
            }
        }

        /// <summary>
        /// 常用键位操作（PC）
        /// </summary>
        private void Video_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = true;
            switch (args.VirtualKey)
            {
                case VirtualKey.Space:
                    {
                        if (media.CurrentState == MediaElementState.Playing)
                        {
                            media.Pause();
                            danmaku.IsPauseDanmaku(true);
                            pause.Icon = new SymbolIcon(Symbol.Play);
                        }
                        else if (media.CurrentState == MediaElementState.Paused)
                        {
                            media.Play();
                            danmaku.ClearStaticDanmu();
                            danmaku.IsPauseDanmaku(false);
                            pause.Icon = new SymbolIcon(Symbol.Pause);
                        }
                    }
                    break;
                case VirtualKey.Left:
                    {
                        sli_main.Value -= 5;
                        messagepop.Show(media.Position.Hours.ToString("00") + ":" + media.Position.Minutes.ToString("00") + ":" + media.Position.Seconds.ToString("00"));
                    }break;
                case VirtualKey.Right:
                    {
                        sli_main.Value += 5;
                        messagepop.Show(media.Position.Hours.ToString("00") + ":" + media.Position.Minutes.ToString("00") + ":" + media.Position.Seconds.ToString("00"));
                    }
                    break;
                case VirtualKey.Up:
                    {
                        sli_vol.Value++;
                        messagepop.Show("音量:" + ((int)sli_vol.Value * 10).ToString() + "%");
                    }
                    break;
                case VirtualKey.Down:
                    {
                        sli_vol.Value--;
                        messagepop.Show("音量:" + ((int)sli_vol.Value * 10).ToString() + "%");
                    }
                    break;
                case VirtualKey.Escape:
                    {
                        ApplicationView.GetForCurrentView().ExitFullScreenMode();
                    }break;
                case VirtualKey.Enter:
                    {
                        if (ApplicationView.GetForCurrentView().IsFullScreenMode)
                        {
                            ApplicationView.GetForCurrentView().ExitFullScreenMode();
                        }
                        else
                        {
                            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 显示弹幕
        /// </summary>
        private void Timer_danmaku_Tick(object sender, object e)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                if (DanmuPool != null)
                {
                    foreach (var item in DanmuPool)
                    {
                        if(Convert.ToInt32(item.Time) == Convert.ToInt32(media.Position.TotalSeconds))
                        {
                            foreach (string word in strs)
                            {
                                if (Regex.IsMatch(item.Message, word) && word.Length > 0) 
                                {
                                    return;
                                }
                            }
                            if (item.Mode == "1")
                            {
                                danmaku.AddBasic(item, false);
                            }
                            if (item.Mode == "4" || item.Mode == "5")
                            {
                                danmaku.AddTop(item, false);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 退出前逻辑
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
            media.Stop();
            media.Source = null;
            GC.Collect();
            timer.Stop();
            displayRq.RequestRelease();     //撤销常亮请求  
        }

        /// <summary>
        /// 动态刷新基本信息
        /// </summary>
        private async void Timer_Tick(object sender, object e)
        {
            switch(WebStatusHelper.GetConnType())
            {
                case ConnectionType.DataConn: txt_web.Text = "数据流量";break;
                case ConnectionType.WlanConn: txt_web.Text = "WiFi"; break;
                case ConnectionType.PPPoE: txt_web.Text = "宽带"; break;
                case ConnectionType.NoConn:txt_web.Text = "无连接";break;
            }
            txt_now.Text = DateTime.Now.Hour.ToString("00") + " ：" + DateTime.Now.Minute.ToString("00");
            txt_bat.Text = ((double)Battery.AggregateBattery.GetReport().RemainingCapacityInMilliwattHours / (double)Battery.AggregateBattery.GetReport().FullChargeCapacityInMilliwattHours * 100).ToString("00") + "%";
            if (SettingHelper.ContainsKey("_cursor"))
            {
                if ((bool)SettingHelper.GetValue("_cursor") == false)
                    return;
            }
            await HideCursor();
        }

        async Task HideCursor()
        {
            if (type == DeviceType.PC && isMouseMoving == false && grid_top.Visibility == Visibility.Collapsed) 
            {
                await Task.Delay(3000);
                //隐藏光标
                Window.Current.CoreWindow.PointerCursor = null;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter.GetType() == typeof(string))
            {
                //文件关联
                string a = e.Parameter.ToString();
                if (a[0] == '@')
                {
                    isLocal = true;
                    left.Visibility = right.Visibility = Visibility.Collapsed;
                    string path = e.Parameter.ToString().Substring(1);
                    file = await StorageFile.GetFileFromPathAsync(path);
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    status.Text += "正在读取弹幕...";
                    var danmupool = await GetDanmu(file);
                    if (danmupool != null)
                    {
                        DanmuPool = danmupool;
                        status.Text += "完毕";
                    }
                    status.Text += Environment.NewLine + "正在读取视频...";
                    txt_title.Text = file.DisplayName;
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    media.SetSource(stream, file.ContentType);
                    status.Text += "完毕";
                    await Task.Delay(500);
                    return;
                }
            }
            //读取本地视频
            if (e.Parameter.GetType() == typeof(StorageFile))                 
            {
                isLocal = true;
                left.Visibility = right.Visibility = Visibility.Collapsed;
                file = e.Parameter as StorageFile;
                if (file != null)
                {
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    status.Text += "正在读取弹幕...";
                    var danmupool = await GetDanmu(file);
                    if (danmupool != null)
                    {
                        DanmuPool = danmupool;
                        if (DanmuPool.Count > 0)
                        {
                            status.Text += DanmuPool.Count.ToString() + "条";
                        }
                        else
                        {
                            status.Text += "完毕";
                        }
                    }
                    status.Text += Environment.NewLine + "正在读取视频...";
                    txt_title.Text = file.DisplayName;
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    media.SetSource(stream, file.ContentType);
                    status.Text += "完毕";
                    await Task.Delay(500);
                    return;
                }             
            }
            //下载列表
            if (e.Parameter.GetType() == typeof(LocalVideo))
            {
                LocalVideo myVideo = e.Parameter as LocalVideo;
                if (myVideo != null)
                {
                    isLocal = true;
                    left.Visibility = right.Visibility = Visibility.Collapsed;
                    part = myVideo.Part;
                    folder = myVideo.Folder;
                    StorageFolder myfolder = await KnownFolders.VideosLibrary.GetFolderAsync("哔哩哔哩");
                    myfolder = await myfolder.GetFolderAsync(folder);
                    file = await myfolder.GetFileAsync(part + ".mp4");
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    status.Text += "正在读取弹幕...";
                    var danmupool = await GetDanmu(file);
                    if (danmupool != null)
                    {
                        DanmuPool = danmupool;
                        status.Text += DanmuPool.Count.ToString() + "完毕";
                    }
                    status.Text += Environment.NewLine + "正在读取视频...";
                    txt_title.Text = part;
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    media.SetSource(stream, file.ContentType);
                    status.Text += "完毕";
                    await Task.Delay(500);
                    return;
                }
            }
            //读取在线视频信息
            isLocal = false;
            infos = e.Parameter as List<VideoInfo>;
            Index = Convert.ToInt32(infos[0].Cid) + 1;
            if (Index == 1) 
                left.Visibility = Visibility.Collapsed;
            if (infos.Count <= 2 || Index == infos.Count - 1) 
            {
                right.Visibility = Visibility.Collapsed;
            }
            await read(Index);
        }

        async Task read(int index)
        {
            danmaku.ClearDanmu();
            media.Visibility = Visibility.Collapsed;
            status.Visibility = Visibility.Visible;
            cid = infos[index].Cid;
            aid = infos[0].Title;
            status.Text = "获取视频地址...";
            URL = await ContentServ.GetVedioURL(cid, quality, format);
            string url = URL.Url;
            status.Text += (URL == null) ? "失败" : "完毕";
            if (URL == null) return;
            status.Text += Environment.NewLine + "加载弹幕数据...";
            try
            {
                DanmuPool = await GetDanmu(cid);
                status.Text += DanmuPool.Count.ToString() + "条";
            }
            catch
            {
                status.Text += "失败";
            }
            media.Source = new Uri(url);
            txt_title.Text = infos[index].Title;
            if (URL.Acceptquality.Contains("1")) q1.Visibility = Visibility.Visible;
            if (URL.Acceptquality.Contains("2")) q2.Visibility = Visibility.Visible;
            if (URL.Acceptquality.Contains("3")) q3.Visibility = Visibility.Visible;
            status.Text += Environment.NewLine + "正在缓冲视频...";
            loading.Visibility = Visibility.Visible;
        }

        void ReverseVisibility()
        {
            grid_top.Visibility = grid_bottom.Visibility = grid_center.Visibility = grid_bottom.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            status.Visibility = Visibility.Collapsed;
            media.Visibility = Visibility.Visible;
            if (isLocal == true)
            {
                cb_format.Visibility = cb_quality.Visibility = Visibility.Collapsed;
            }
            if (!isHideDanmu)
            {
                timer_danmaku.Start();
            }
            TimeSpan ts = new TimeSpan(0, 0, (int)media.NaturalDuration.TimeSpan.TotalSeconds);
            if (ts.Hours == 0)
                txt_total.Text = ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            else
                txt_total.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            sli_main.Maximum = ts.TotalSeconds;
            await Task.Delay(500);
            grid_top.Visibility = grid_bottom.Visibility = grid_center.Visibility = Visibility.Collapsed;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                media.Pause();
                danmaku.IsPauseDanmaku(true);
                pause.Icon = new SymbolIcon(Symbol.Play);
            }
            else if (media.CurrentState == MediaElementState.Paused)
            {
                media.Play();
                danmaku.ClearStaticDanmu();
                danmaku.IsPauseDanmaku(false);
                pause.Icon = new SymbolIcon(Symbol.Pause);
            }
        }

        /// <summary>
        /// 横纵比
        /// </summary>
        private void full_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            string tag = item.Tag.ToString();
            if (tag == "0")
            {
                media.Stretch = Stretch.Uniform;
            }
            else if (tag == "1") 
            {
                media.Stretch = Stretch.Fill;
            }
            else if (tag == "2")
            {
                if (ApplicationView.GetForCurrentView().IsFullScreenMode)
                {
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
                }
                else
                {
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                }
            }
        }
        /// <summary>
        /// 音量调节
        /// </summary>
        private void sli_vol_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (isInited)
            {
                media.Volume = sli_vol.Value * 0.1;
                SettingHelper.SetValue("_vol", sli_vol.Value);
            }
        }
        /// <summary>
        /// 亮度调节
        /// </summary>
        private void sli_light_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if(lightinit)
            {
                border.Opacity = 1 - sli_light.Value;
                SettingHelper.SetValue("_light", sli_light.Value);
            }
        }

        private void border_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ReverseVisibility();
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void media_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {
            pro_down.Value = media.DownloadProgress * 100;
        }

        async void SendDanmu()
        {
            if (!ApiHelper.IsLogin() || !WebStatusHelper.IsOnline()) 
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(txt_mydanmu.Text))
            {
                messagepop.Show("弹幕内容不能为空哦！");
                return;
            }
            try
            {
                /*
                 int v = int.Parse(Color, System.Globalization.NumberStyles.HexNumber);
                        SolidColorBrush solid = new SolidColorBrush(new Color()
                        {
                            A = Convert.ToByte(255),
                            R = Convert.ToByte((v >> 16) & 255),
                            G = Convert.ToByte((v >> 8) & 255),
                            B = Convert.ToByte((v >> 0) & 255)
                        });
                */
                string url = "http://api.bilibili.com/comment/post?_device=wp&_ulv=10000&build=430000&access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&aid=" + aid + "&cid=" + cid + "&pid=1&platform=android&scale=xhdpi";
                url += ApiHelper.GetSign(url);
                int a = (byte.Parse(sli_r.Value.ToString()) << 16) + (byte.Parse(sli_g.Value.ToString()) << 8) + (byte.Parse(sli_b.Value.ToString()) << 0);//位运算的优先级最低！
                string color = a.ToString();
                string Args = "mid=" + UserHelper.Mid + "&type=json" + "&cid=" + cid + "&playTime=" + media.Position.TotalSeconds.ToString() +"&color=" + color+ "&msg=" + txt_mydanmu.Text + "&fontsize=25&mode=" + danmakuMode + "&pool=0&rnd=" + new Random().Next(1000, 2000).ToString();
                string code = await BaseService.SendPostAsync(url, Args, "http://api.bilibili.com");
                JsonObject json = JsonObject.Parse(code);
                if (json.ContainsKey("code"))
                {
                    if (json["code"].ToString() == "0") 
                    {
                        switch (danmakuMode)
                        {
                            case "1": danmaku.AddBasic(new DanmuModel { Message = txt_mydanmu.Text, Color = color, Size = "25" }, true); break;
                            case "4": danmaku.AddTop(new DanmuModel { Message = txt_mydanmu.Text, Color = color, Size = "25", Mode = "5" }, true); break;
                            case "5": danmaku.AddTop(new DanmuModel { Message = txt_mydanmu.Text, Color = color, Size = "25", Mode = "4" }, true); break;
                        }
                        return;
                    }
                }
            }
            catch(Exception e)
            {
                messagepop.Show("发送失败" + e.Message);
            }
            finally
            {
                txt_mydanmu.Text = string.Empty;
            }
        }

        /// <summary>
        /// 切换P
        /// </summary>
        private async void SwitchP_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "0":
                    {
                        Index--;
                        if (Index == 1)
                            left.Visibility = Visibility.Collapsed;
                        await read(Index);
                    }break;
                case "1":
                    {
                        left.Visibility = Visibility.Visible;
                        Index++;
                        if (Index == infos.Count - 1)
                            right.Visibility = Visibility.Collapsed;
                        await read(Index);
                        break;
                    }
            }
        }

        private async void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (infos == null)
            {
                Frame.GoBack();
            }
            else
            {
                if (Index < infos.Count - 1)
                {
                    status.Text = "本选集已播放完毕，即将进入下一选集…";
                    left.Visibility = Visibility.Visible;
                    Index++;
                    if (Index == infos.Count - 1)
                        right.Visibility = Visibility.Collapsed;
                    await read(Index);
                }
                else
                    Frame.GoBack();
            }             
        }

        /// <summary>
        /// 弹幕文档解析
        /// </summary>
        /// <param name="doc">弹幕XML文档</param>
        /// <returns></returns>
        private List<DanmuModel> AnalysisDanmaku(XmlDocument doc)
        {
            List<DanmuModel> list = new List<DanmuModel>();
            var a = doc.GetElementsByTagName("d");
            foreach (XmlElement item in a)
            {
                string heheda = item.GetAttribute("p");
                string[] haha = heheda.Split(',');
                list.Add(new DanmuModel
                {
                    Time = decimal.Parse(haha[0]),
                    Mode = haha[1],
                    Size = haha[2],
                    Color = haha[3],
                    //DanSendTime = haha[4],
                    // DanPool = haha[5],
                    //DanID = haha[6],
                    //DanRowID = haha[7],
                    Message = item.InnerText
                });
            }
            list = list.OrderBy(f => f.Time).ToList();
            if (list != null && (bool)SettingHelper.GetValue("_autokill") == true)
            {
                StartKill();
            }
            return list;
        }
        /// <summary>
        /// 获取弹幕数据(网络)
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        async Task<List<DanmuModel>> GetDanmu(string cid)
        {
            List<DanmuModel> list = new List<DanmuModel>();
            XmlDocument doc = new XmlDocument();
            try
            {
                //http://comment.bilibili.com/10631099.xml
                if (WebStatusHelper.IsOnline() && cid.Length > 0) //离线视频需要加个cid
                {
                    string url = "http://comment.bilibili.com/" + cid + ".xml";
                    string txt = await BaseService.SentGetAsync(url);
                    doc.LoadXml(txt);
                    if (DanmuPool != null && (bool)SettingHelper.GetValue("_autokill") == true)
                    {
                        StartKill();
                    }
                    return AnalysisDanmaku(doc);
                }
                else
                {
                    status.Text += "没有下载弹幕且当前无网络连接，跳过";
                    return null;
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 读取本地弹幕
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        async Task<List<DanmuModel>> GetDanmu(StorageFile videofile)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                StorageFolder folder = await videofile.GetParentAsync();
                videofile = await folder.GetFileAsync(videofile.DisplayName + ".xml");
                if (videofile != null)
                {
                    string xml = string.Empty;
                    using (Stream file0 = await videofile.OpenStreamForReadAsync())
                    {
                        using (StreamReader read = new StreamReader(file0))
                        {
                            xml = read.ReadToEnd();
                        }
                    }
                    doc.LoadXml(xml);
                    return AnalysisDanmaku(doc);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 弹幕开关
        /// </summary>
        private void DanmuSwitch_Click(object sender, RoutedEventArgs e)
        {
            isHideDanmu = !isHideDanmu;
            if (isHideDanmu)
            {
                timer_danmaku.Stop();
                danmaku.ClearDanmu();
            }
            else
            {
                timer_danmaku.Start();
            }
            stk.Visibility = (bool)kill_all.IsChecked ? Visibility.Collapsed : Visibility.Visible;
        }
        /// <summary>
        /// 手势
        /// </summary>
        private void border_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            //e.Handled = true;
            double Y = e.Delta.Translation.Y;
            double X = e.Delta.Translation.X;
            if (Math.Abs(X) > (Math.Abs(Y) + 5))
            {
                isX = true;
                media.Pause();
                pause.Icon = new SymbolIcon(Symbol.Play);
                double actual = X / this.ActualWidth;
                //横跨屏幕的TimeSpan:90s（一分半）
                sli_main.Value += actual * 90;
                TimeSpan time = new TimeSpan(0, 0, (int)sli_main.Value);
                string posttime = string.Empty;
                if (time.Hours > 0)
                {
                    posttime = time.Hours.ToString("00") + ":" + time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");
                }
                else
                {
                    posttime = time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");

                }
                messagepop.Show(posttime);
            }
            else if ((Math.Abs(Y) > (Math.Abs(X) + 5))) 
            {
                isX = false;
                sli_light.Value -= 0.01 * (int)(Y / 10);
                messagepop.Show("亮度：" + (sli_light.Value * 100).ToString("00") + "%");
            }
        }

        private void border_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            //e.Handled = true;
            if (isX)
            {
                media.Play();
                pause.Icon = new SymbolIcon(Symbol.Pause);
            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            SendDanmu();
            ReverseVisibility();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ham.IsPaneOpen = !ham.IsPaneOpen;
            if (isPropInit == false)
            {
                if (isLocal == false)
                {
                    info.Text = "视频大小:" + (int.Parse(URL.Size) / 1024 / 1024).ToString() + "MB" + Environment.NewLine +
                         "视频尺寸:" + media.NaturalVideoWidth.ToString() + "×" + media.NaturalVideoHeight.ToString() + Environment.NewLine +
                         "最高清晰度:" + URL.Acceptquality.Count.ToString();
                }
                else if (isLocal == true)
                {
                    var property = await file.GetBasicPropertiesAsync();
                    var property2 = await file.Properties.GetVideoPropertiesAsync();
                    var property3 = await file.Properties.GetMusicPropertiesAsync();
                    info.Text = "视频大小:" + (property.Size / Math.Pow(1024, 2)).ToString("0.0") + "M" + Environment.NewLine +
                        "修改时间:" + property.DateModified.ToLocalTime().ToString() + Environment.NewLine +
                        "视频尺寸:" + property2.Width.ToString() + "×" + property2.Height.ToString() + Environment.NewLine +
                        "总比特率:" + (property2.Bitrate / 1000).ToString() + "kbps";
                }
                isPropInit = true;
            }         
        }

        private void rgb_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            try
            {
                rec.Fill = new SolidColorBrush(Color.FromArgb(255, byte.Parse(r.Text), byte.Parse(g.Text), byte.Parse(b.Text)));
            }
            catch { }
        }

        private void border_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                media.Pause();
                danmaku.IsPauseDanmaku(true);
                pause.Icon = new SymbolIcon(Symbol.Play);
            }
            else if (media.CurrentState == MediaElementState.Paused)
            {
                media.Play();
                danmaku.IsPauseDanmaku(false);
                danmaku.ClearStaticDanmu();
                pause.Icon = new SymbolIcon(Symbol.Pause);
            }
        }

        private void media_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            switch(media.CurrentState)
            {
                case MediaElementState.Buffering:
                    {
                        loading.Visibility = Visibility.Visible;
                        txt_load.Text = media.BufferingProgress.ToString("00" + "%");
                    }
                    break;
                default: loading.Visibility = Visibility.Collapsed; break;
            }
        }

        private void sli_space_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("_space", sli_space.Value);
            //注意:由于预定义了value值，初始化时会引发ValueChanged事件
            if (isInited) danmaku.ChangeSpace((int)sli_space.Value);
        }

        private void sli_speed_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("_speed", sli_speed.Value);
            if (isInited) danmaku.ChangeSpeed(15 - (int)sli_speed.Value);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (isInited)
            {
                CheckBox cb = sender as CheckBox;
                switch (cb.Tag.ToString())
                {
                    case "g": danmaku.ClearGun((bool)cb.IsChecked); break;
                    case "t": danmaku.ClearTop((bool)cb.IsChecked); break;
                    case "b": danmaku.ClearBottom((bool)cb.IsChecked); break;
                }
            }
        }

        private void sli_fontsize_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            int value = (int)sli_fontsize.Value;
            SettingHelper.SetValue("_fontsize", value);
            if (isInited) danmaku.ChangeSize(value);
        }

        private void Kill_Click(object sender, RoutedEventArgs e)
        {
            StartKill();
        }

        private void StartKill()
        {
            strs.Clear();
            if (!string.IsNullOrWhiteSpace(txt_word.Text))
            {
                foreach (var word in txt_word.Text.Split(' '))
                {
                    if (word.Length == 0) continue;
                    strs.Add(word);
                }
            }
            if (wordInclude.IsChecked != true)
            {
                if (SettingHelper.ContainsKey("_words"))
                {
                    string[] words = SettingHelper.GetValue("_words").ToString().Split(' ');
                    foreach (string word in words)
                    {
                        if (word.Length > 0) strs.Add(word);
                    }
                }
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            danmakuMode = item.Tag.ToString();
            SendMode.Content = item.Text.ToString();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string font = string.Empty;
            font = cb_font.SelectedItem.ToString();
            if (danmaku != null)
            {
                SettingHelper.SetValue("_danmufont", cb_font.SelectedIndex);
                danmaku.Setfont(font);
            }
        }

        private async void ComboBoxItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            quality = item.Tag.ToString();
            await read(Index);
        }

        private async void ComboBoxItem_Tapped_1(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            switch(item.Tag.ToString())
            {
                case "mp4": format = VideoFormat.mp4;break;
                //case "hdmp4": format = VideoFormat.hdmp4; break;
                case "flv": format = VideoFormat.flv; break;
            }
            await read(Index);
        }

        private void hidepane_Click(object sender, RoutedEventArgs e)
        {
            ham.IsPaneOpen = false;
        }

        private void rating_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            media.PlaybackRate = rating.Value;
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            switch(isRepeat)
            {
                case null:
                    R_1 = (int)media.Position.TotalMilliseconds;
                    isRepeat = true;
                    repeat.Icon = new SymbolIcon(Symbol.ZoomIn);
                    break;
                case true:
                    R_2 = (int)media.Position.TotalMilliseconds;
                    media.Position = TimeSpan.FromMilliseconds(R_1);
                    isRepeat = false;
                    timer_repeat.Start();
                    repeat.Icon = new SymbolIcon(Symbol.ZoomOut);
                    break;
                case false:
                    isRepeat = null;
                    timer_repeat.Stop();
                    repeat.Icon = new SymbolIcon(Symbol.RepeatAll);
                    break;
            }
        }
    }
    public sealed class SecToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan a;
            try
            {
                a = (TimeSpan)value;
            }
            catch
            {
                a = new TimeSpan(0, 0, System.Convert.ToInt32(value));
            }
            int i = (int)a.TotalSeconds;
            TimeSpan ts = new TimeSpan(0, 0, i);
            if (ts.Hours == 0)
                return ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            else
                return ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public sealed class PositionCVT : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var a = (TimeSpan)value;
            int i = (int)a.TotalSeconds;
            return i;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return new TimeSpan(0, 0, System.Convert.ToInt32(value));
        }
    }
}
