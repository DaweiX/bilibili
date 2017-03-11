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
using FFmpegInterop;
using Windows.Media.Core;
using Windows.UI.Xaml.Controls.Primitives;

//  “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Video : Page
    {
        FFmpegInteropMSS FFmpegMSS;
        VideoURL URL;
        Purl _currentP;
        bool isX = false;
        DispatcherTimer timer_danmaku = new DispatcherTimer();
        DispatcherTimer timer_repeat = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        DispatcherTimer timer = new DispatcherTimer();
        DisplayRequest displayRq = new DisplayRequest();
        List<int> Flags = new List<int>();
        string part = string.Empty;
        string folder = string.Empty;
        string quality = "2";
        VideoFormat format;
        string cid = string.Empty;
        string aid = string.Empty;
        int R_1, R_2;
        string danmakuMode = "1";
        bool? isRepeat = null;
        List<VideoInfo> infos;
        StorageFile file = null;
        List<string> strs = new List<string>();
        bool isHideDanmu = false;
        List<DanmuModel> DanmuPool;
        bool isInited = false;
        bool? isLocal = null;
        bool isMouseMoving = false;
        bool force_a = false;
        bool force_v = false;
        int Index = 0;
        bool isPropInit = false;
        bool lightinit = false;
        int _offsettime;
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
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape; // 横向屏幕
            if (!WebStatusHelper.IsOnline())
            {
                txt_mydanmu.PlaceholderText = "没有联网哦，不能发弹幕";
                txt_mydanmu.IsEnabled = false;
            }
            displayRq.RequestActive();      // 保持屏幕常亮
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
            if (SettingHelper.DeviceType == DeviceType.PC)
            {
                force_a = force_v = true;
            }
            if (SettingHelper.ContainsKey("_faudio"))
            {
                force_a = (bool)SettingHelper.GetValue("_faudio");
            }
            if (SettingHelper.ContainsKey("_fvideo"))
            {
                force_v = (bool)SettingHelper.GetValue("_fvideo");
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
                            icon_play = new SymbolIcon(Symbol.Play);
                        }
                        else if (media.CurrentState == MediaElementState.Paused)
                        {
                            media.Play();
                            danmaku.ClearStaticDanmu();
                            danmaku.IsPauseDanmaku(false);
                            icon_play = new SymbolIcon(Symbol.Pause);
                        }
                    }
                    break;
                case VirtualKey.Left:
                    {
                        sli_main.Value -= 5;
                        messagepop.Show(media.Position.Hours.ToString("00") + ":" + media.Position.Minutes.ToString("00") + ":" + media.Position.Seconds.ToString("00"));
                    }
                    break;
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
                    }
                    break;
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
        /// 发送弹幕（自动）
        /// </summary>
        private void Timer_danmaku_Tick(object sender, object e)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                if (DanmuPool != null)
                {
                    foreach (var item in DanmuPool)
                    {
                        if (Convert.ToInt32(item.Time) == Convert.ToInt32(media.Position.TotalSeconds + _offsettime))
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
            if (FFmpegMSS != null)
            {
                FFmpegMSS.Dispose();
            }
            GC.Collect();
            timer.Stop();
            // 撤销常亮请求  
            displayRq.RequestRelease();     
        }

        /// <summary>
        /// 动态刷新基本信息
        /// </summary>
        private async void Timer_Tick(object sender, object e)
        {
            switch (WebStatusHelper.GetConnType())
            {
                case ConnectionType.DataConn: txt_web.Text = "数据流量"; break;
                case ConnectionType.WlanConn: txt_web.Text = "WiFi"; break;
                case ConnectionType.PPPoE: txt_web.Text = "宽带"; break;
                case ConnectionType.NoConn: txt_web.Text = "无连接"; break;
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
            // 窗口模式就不隐藏了，不然很不方便
            if (!ApplicationView.GetForCurrentView().IsFullScreenMode) return;
            if (SettingHelper.DeviceType == DeviceType.PC && isMouseMoving == false && grid_top.Visibility == Visibility.Collapsed)
            {
                await Task.Delay(3000);
                // 隐藏光标（将指针设为空即可）
                Window.Current.CoreWindow.PointerCursor = null;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter.GetType() == typeof(List<VideoInfo>))
            {
                // 读取在线视频信息
                isLocal = false;
                infos = e.Parameter as List<VideoInfo>;
                Index = Convert.ToInt32(infos[0].Cid) + 1;
                if (Index == 1)
                    left.Visibility = Visibility.Collapsed;
                if (infos.Count <= 2 || Index == infos.Count - 1)
                {
                    right.Visibility = Visibility.Collapsed;
                }
                if (SettingHelper.ContainsKey("_videoformat"))
                {
                    format = SettingHelper.GetValue("_videoformat").ToString() == "0"
                    ? VideoFormat.mp4
                    : VideoFormat.flv;
                }
                // 默认的格式：mp4
                else format = VideoFormat.mp4;
                await read(Index);
            }
            else
            {
                isLocal = true;
                btn_Switchffmpeg.Visibility = Visibility.Visible;
                JsonObject json = new JsonObject();      
                if (e.Parameter.GetType() == typeof(string))
                {
                    // 文件关联
                    string a = e.Parameter.ToString();
                    if (a[0] == '@')
                    {
                        left.Visibility = right.Visibility = Visibility.Collapsed;
                        string path = e.Parameter.ToString().Substring(1);
                        file = await StorageFile.GetFileFromPathAsync(path);
                        await GetInfoAsync();
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                        status.Text += "正在读取弹幕...";
                        var danmupool = await GetDanmu(file);
                        status.Text += Environment.NewLine + "正在读取视频...";
                        txt_title.Text = file.DisplayName;
                        var stream = await file.OpenAsync(FileAccessMode.Read);
                        media.SetSource(stream, file.ContentType);
                        status.Text += "完毕";
                        await Task.Delay(500);
                        return;
                    }
                }
                // 读取本地视频
                if (e.Parameter.GetType() == typeof(StorageFile))
                {
                    left.Visibility = right.Visibility = Visibility.Collapsed;
                    file = e.Parameter as StorageFile;
                    await GetInfoAsync();
                    if (file != null)
                    {
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                        status.Text += "正在读取弹幕...";
                        var danmupool = await GetDanmu(file);
                        status.Text += Environment.NewLine + "正在读取视频...";
                        txt_title.Text = file.DisplayName;
                        // 系统原生支持的类型
                        if (VideoHelper.videoExtensions_sys.Contains(file.FileType))
                        {
                            var stream = await file.OpenAsync(FileAccessMode.Read);
                            media.SetSource(stream, file.ContentType);
                            status.Text += "完毕";
                            await Task.Delay(500);
                            return;
                        }
                        else
                        {
                            await SetFFmpegSource(file);
                            status.Text += "完毕";
                            await Task.Delay(500);
                            return;
                        }
                    }
                }
                // 下载列表
                if (e.Parameter.GetType() == typeof(LocalVideo))
                {
                    LocalVideo myVideo = e.Parameter as LocalVideo;
                    if (myVideo != null)
                    {
                        left.Visibility = right.Visibility = Visibility.Collapsed;
                        part = myVideo.Part;
                        folder = myVideo.Folder;
                        StorageFolder myfolder = await KnownFolders.VideosLibrary.GetFolderAsync("哔哩哔哩");
                        myfolder = await myfolder.GetFolderAsync(folder);
                        // 文件的原本名称是 part_index, 而弹幕文件是 part, 不冲突
                        string filename = $"{part}{myVideo.Format}";
                        file = await myfolder.GetFileAsync(filename);
                        await GetInfoAsync();
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                        status.Text += "正在读取弹幕...";
                        var danmupool = await GetDanmu(file);
                        if (danmupool != null)
                        {
                            DanmuPool = danmupool;
                        }
                        status.Text += Environment.NewLine + "正在读取视频...";
                        txt_title.Text = part.Split('_')[0];
                        // 如果是系统原生支持的格式，直接设置媒体源
                        if (VideoHelper.videoExtensions_sys.Contains(file.FileType))
                        {
                            var stream = await file.OpenAsync(FileAccessMode.Read);
                            media.SetSource(stream, file.ContentType);
                            status.Text += "完毕";
                            await Task.Delay(500);
                            return;
                        }
                        // 否则，使用ffmpeg
                        else
                        {
                            await SetFFmpegSource(file);
                            return;
                        }
                    }
                }
            }
        }

        async Task GetInfoAsync()
        {
            try
            {
                JsonObject json = new JsonObject();
                StorageFile file_list = await KnownFolders.VideosLibrary.GetFileAsync("list.json");
                if (file_list != null)
                {
                    using (Stream file0 = await file_list.OpenStreamForReadAsync())
                    {
                        StreamReader reader = new StreamReader(file0);
                        string txt = await reader.ReadToEndAsync();
                        json = JsonObject.Parse(txt);
                        JsonArray array = json["data"].GetArray();
                        string title = file.DisplayName.Split('_')[0];
                        var a = from item in array where item.GetObject().ContainsKey(title) select item;
                        json = a.First().GetObject();
                        json = json[title].GetObject();
                        cid = json["cid"].GetString();
                    }
                }
            }
            catch { }
        }

        async Task read(int index)
        {
            danmaku.ClearDanmu();
            media.Visibility = Visibility.Collapsed;
            grid_status.Visibility = Visibility.Visible;
            cid = infos[index].Cid;
            aid = infos[0].Title;
            status.Text = "获取视频地址...";
            URL = await ContentServ.GetVedioURL(cid, quality, format);
            status.Text += (URL == null)
                ? "失败"
                : $"{URL.Ps.Count}个{format}分段";
            if (URL == null) return;
            status.Text += Environment.NewLine + "加载弹幕数据...";
            try
            {
                DanmuPool = await GetDanmu(cid);
            }
            catch (Exception e)
            {
                status.Text += "失败" + e.Message;
            }
            if (format == VideoFormat.mp4)
            {
                // MP4只有一个分段（根据经验(￣▽￣)"）
                media.Source = new Uri(URL.Ps[0].Url);
            }
            else if (format == VideoFormat.flv)
            {
                // 先读第一个分段
                SetFFmpegSource(URL.Ps[0].Url);
            }
            if (URL.Ps.Count > 1)
            {
                // Flag该立的时候也得立啊
                // GetFlags();
            }
            _currentP = URL.Ps[0];
            txt_title.Text = infos[index].Title;
            if (URL.Acceptquality.Contains("1")) q1.Visibility = Visibility.Visible;
            if (URL.Acceptquality.Contains("2")) q2.Visibility = Visibility.Visible;
            if (URL.Acceptquality.Contains("3")) q3.Visibility = Visibility.Visible;
            status.Text += Environment.NewLine + "正在缓冲视频...";
            loading.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 设置FFmpeg媒体源
        /// </summary>
        private void SetFFmpegSource(string url)
        {
            FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromUri(url, force_a, force_v);
            MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();
            if (mss != null)
            {
                media.SetMediaStreamSource(mss);
            }
            // 更新当前分段
            int index = URL.Ps.FindIndex(p => p.Url == url);
            _currentP = URL.Ps[index];
            ///更新时间偏移量，用于<see cref="Timer_danmaku_Tick(object, object)"/>
            _offsettime = 0;
            for (int i = index; i > 0; i--)
            {
                _offsettime += (int)(URL.Ps[i - 1].Length / 1000);
            }
        }

        /// <summary>
        /// 设置FFmpeg媒体源
        /// </summary>
        private async Task SetFFmpegSource(StorageFile file)
        {
            var stream = await file.OpenAsync(FileAccessMode.Read);
            media.SetSource(stream, file.ContentType);
            FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(stream, force_a, force_v);
            MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();
            if (mss != null)
            {
                media.SetMediaStreamSource(mss);
            }
        }

        void ReverseVisibility()
        {
            grid_top.Visibility = grid_bottom.Visibility = grid_center.Visibility =
                grid_bottom.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private async void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            grid_status.Visibility = Visibility.Collapsed;
            media.Visibility = Visibility.Visible;
            if (isLocal == true)
            {
                cb_format.Visibility = cb_quality.Visibility = Visibility.Collapsed;
            }
            if (!isHideDanmu)
            {
                timer_danmaku.Start();
            }
            if (isLocal == null) return;
            TimeSpan ts;
            if (isLocal != true)
            {
                ts = new TimeSpan(0, 0, (int)URL.TotalLength / 1000);
                sli_main.Maximum = URL.TotalLength / 1000;
            }
            else
            {
                if (FFmpegMSS != null)
                {
                    ts = FFmpegMSS.GetMediaStreamSource().Duration;
                }
                else
                {
                    var pro = await file.Properties.GetVideoPropertiesAsync();
                    ts = pro.Duration;
                }
                sli_main.Maximum = ts.TotalSeconds;
            }
            if (ts.Hours == 0)
                txt_total.Text = ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            else
                txt_total.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            // 分段长度之和
            await RefreshInfo();
            await Task.Delay(500);
            grid_top.Visibility = grid_bottom.Visibility = grid_center.Visibility = Visibility.Collapsed;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                media.Pause();
                danmaku.IsPauseDanmaku(true);
                icon_play = new SymbolIcon(Symbol.Play);
            }
            else if (media.CurrentState == MediaElementState.Paused)
            {
                media.Play();
                danmaku.ClearStaticDanmu();
                danmaku.IsPauseDanmaku(false);
                icon_play = new SymbolIcon(Symbol.Pause);
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
            if (lightinit)
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

        /// <summary>
        /// 发送弹幕（手动）
        /// </summary>
        async void SendDanmu()
        {
            if (!ApiHelper.IsLogin() || !WebStatusHelper.IsOnline())
            {
                return;
            }
            if (string.IsNullOrEmpty(cid))
            {
                messagepop.Show("只有来自b站的视频才能发送弹幕哦！");
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
                int a = (byte.Parse(sli_r.Value.ToString()) << 16) + (byte.Parse(sli_g.Value.ToString()) << 8) + (byte.Parse(sli_b.Value.ToString()) << 0);// 位运算的优先级最低！
                string color = a.ToString();
                string Args = "mid=" + UserHelper.Mid + "&type=json" + "&cid=" + cid + "&playTime=" + media.Position.TotalSeconds.ToString() + "&color=" + color + "&msg=" + txt_mydanmu.Text + "&fontsize=25&mode=" + danmakuMode + "&pool=0&rnd=" + new Random().Next(1000, 2000).ToString();
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
            catch (Exception e)
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
                    }
                    break;
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
                /*------------单段播放完的操作------------*/
                // 如果不是最后一个分段
                if (URL.Ps.Last() != _currentP)
                {
                    SetFFmpegSource(URL.Ps[URL.Ps.IndexOf(_currentP) + 1].Url);
                    return;
                }
                // 否则
                /*------------整集播放完的操作------------*/
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
                {
                    Frame.GoBack();
                }
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
                    // DanSendTime = haha[4],
                    //  DanPool = haha[5],
                    // DanID = haha[6],
                    // DanRowID = haha[7],
                    Message = item.InnerText
                });
            }
            list = list.OrderBy(f => f.Time).ToList();
            if (list != null && (bool)SettingHelper.GetValue("_autokill") == true)
            {
                StartKill();
            }
            status.Text += list.Count + "条";
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
                // http://comment.bilibili.com/10631099.xml?rnd=991
                if (WebStatusHelper.IsOnline() && cid.Length > 0) 
                {
                    string url = "http://comment.bilibili.com/" + cid + ".xml?rnd=" + new Random().Next(500, 1000);
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
                videofile = await folder.GetFileAsync(videofile.DisplayName.Split('_')[0] + ".xml");
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
            // e.Handled = true;
            double Y = e.Delta.Translation.Y;
            double X = e.Delta.Translation.X;
            if (Math.Abs(X) > (Math.Abs(Y) + 5))
            {
                isX = true;
                media.Pause();
                icon_play = new SymbolIcon(Symbol.Play);
                double actual = X / this.ActualWidth;
                // 横跨屏幕的TimeSpan:90s（一分半）
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
            // e.Handled = true;
            if (isX)
            {
                media.Play();
                icon_play = new SymbolIcon(Symbol.Pause);
            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            SendDanmu();
            ReverseVisibility();
        }

        async Task RefreshInfo()
        {
            if (isLocal == false)
            {
                info.Text = "视频大小:" + (_currentP.Size / Math.Pow(1024, 2)).ToString("0.0") + "M" + Environment.NewLine +
                     "视频尺寸:" + media.NaturalVideoWidth.ToString() + "×" + media.NaturalVideoHeight.ToString() + Environment.NewLine +
                     "分段数:" + URL.Ps.Count.ToString();
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ham.IsPaneOpen = !ham.IsPaneOpen;
            if (isPropInit == false)
            {
                await RefreshInfo();
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
                icon_play = new SymbolIcon(Symbol.Play);
            }
            else if (media.CurrentState == MediaElementState.Paused)
            {
                media.Play();
                danmaku.IsPauseDanmaku(false);
                danmaku.ClearStaticDanmu();
                icon_play = new SymbolIcon(Symbol.Pause);
            }
        }

        private void media_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            switch (media.CurrentState)
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
            // 注意:由于预定义了value值，初始化时会引发ValueChanged事件
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
            switch (item.Tag.ToString())
            {
                case "mp4": format = VideoFormat.mp4; break;
                // case "hdmp4": format = VideoFormat.hdmp4; break;
                case "flv": format = VideoFormat.flv; break;
            }
            await read(Index);
        }

        private void hidepane_Click(object sender, RoutedEventArgs e)
        {
            ham.IsPaneOpen = false;
        }

        private void rating_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            media.PlaybackRate = rating.Value;
        }

        /// <summary>
        /// 这个事件仅用于提供多分段下调戏进度条的功能！(DRAG)
        /// </summary>
        private void sli_main_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // double currentPos = sli_main.Value * 1000;
            //// 注意：从零开始的index
            // int currentindex = URL.Ps.IndexOf(_currentP);
            // int goalindex = 0;
            // for (int i = Flags.Count; i > 0; i--)
            // {
            //     if (currentPos > Flags[i - 1])
            //     {
            //         goalindex = i - 1;
            //     }
            // }
        }

        void GetFlags()
        {
            // 间断点数量 = 分段数 - 1
            for (int i = 0; i < URL.Ps.Count - 1; i++)
            {
                int f = 0;
                for (int j = i; i >= 0; j--)
                {
                    f += (int)(URL.Ps[j].Length / 1000);
                }
                Flags.Add(f);
            }
        }

        private async void Switchffmpeg_Click(object sender, RoutedEventArgs e)
        {
            if (isLocal == true)
            {
                await SetFFmpegSource(file);
            }
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            switch (isRepeat)
            {
                case null:
                    R_1 = (int)media.Position.TotalMilliseconds;
                    isRepeat = true;
                    icon_repeat = new SymbolIcon(Symbol.ZoomIn);
                    break;
                case true:
                    R_2 = (int)media.Position.TotalMilliseconds;
                    media.Position = TimeSpan.FromMilliseconds(R_1);
                    isRepeat = false;
                    timer_repeat.Start();
                    icon_repeat = new SymbolIcon(Symbol.ZoomOut);
                    break;
                case false:
                    isRepeat = null;
                    timer_repeat.Stop();
                    icon_repeat = new SymbolIcon(Symbol.RepeatAll);
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
