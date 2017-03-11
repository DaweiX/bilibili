using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.UI.Notifications;

namespace BackgroundTask
{
    // windows运行时组件中，public方法一定要声明为密封类
    public sealed class TileTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Helper.SetValue("_toastquene", "");
            var deferral = taskInstance.GetDeferral();
            await MyTask();
            deferral.Complete();
        }

        private async Task<string> deal()
        {
            List<Feed_Bangumi> list0 = await GetPulls();
            UpdateTile(list0);
            // 这块写得有点难看
            if (Helper.ContainsKey("_toast_m"))
            {
                if ((bool)Helper.GetValue("_toast_m"))
                {
                    SendToast();
                }
            }
            else
            {
                SendToast();
            }
            if (Helper.ContainsKey("_toast_b"))
            {
                if ((bool)Helper.GetValue("_toast_b"))
                {
                    SendToast(list0);
                }
            }
            else
            {
                SendToast(list0);
            }
            return null;
        }

        private IAsyncOperation<string> MyTask()
        {
            try
            {
                if (!Helper.ContainsKey("_downloadcost"))
                {
                    return AsyncInfo.Run(token => deal());
                }
                else
                {
                    if ((bool)Helper.GetValue("_downloadcost"))
                    {
                        ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                        if (!profile.IsWlanConnectionProfile)
                        {
                            return null;
                        }
                        else
                        {
                            return AsyncInfo.Run(token => deal());
                        }
                    }
                    else return AsyncInfo.Run(token => deal());
                }
            }
            catch(Exception e)
            {
                string a = e.Message;
                return null;
            }
        }

        private async Task<List<Feed_Bangumi>> GetPulls()
        {
            List<Feed_Bangumi> list = new List<Feed_Bangumi>();
            string url = "http://api.bilibili.com/x/feed/pull?type=0&_device=wp&_ulv=10000&build=424000&platform=android&appkey=" + Helper.appkey + "&access_key=" + Helper.GetValue("_accesskey").ToString() + "&pn=1&ps=30&rnd=" + new Random().Next(1000, 2000).ToString();
            url += Helper.GetSign(url);
            JsonObject json = await Helper.GetJson(url);
            if (json.ContainsKey("data"))
            {
                json = json["data"].GetObject();
                if (json.ContainsKey("feeds"))
                {
                    JsonArray array = json["feeds"].GetArray();
                    foreach (var item in array)
                    {
                        Feed_Bangumi feed = new Feed_Bangumi();
                        json = item.GetObject();
                        if (json.ContainsKey("addition"))
                        {
                            JsonObject json2 = json["addition"].GetObject();
                            if (json2.ContainsKey("title"))
                            {
                                feed.Title = json2["title"].GetString();
                            }
                            if (json2.ContainsKey("status"))
                            {
                                feed.Status = json2["status"].ToString();
                            }
                            if (json2.ContainsKey("aid"))
                            {
                                feed.Aid = json2["aid"].ToString();
                            }
                            if (json2.ContainsKey("create"))
                            {
                                feed.Time = json2["create"].GetString();
                            }
                            if (json2.ContainsKey("pic"))
                            {
                                feed.Pic = json2["pic"].GetString();
                            }
                        }
                        if (json.ContainsKey("src_id"))
                        {
                            feed.Sid = json["src_id"].ToString();
                        }
                        if (json.ContainsKey("source"))
                        {
                            JsonObject json2 = json["source"].GetObject();
                            if (json2.ContainsKey("new_ep"))
                            {
                                json2 = json2["new_ep"].GetObject();
                                if (json2.ContainsKey("index"))
                                {
                                    feed.New_ep = json2["index"].GetString();
                                }
                            }
                        }
                        var test = list.Find(o => o.Sid == feed.Sid);
                        if (test == null)
                        {
                            list.Add(feed);
                        }
                    }
                    // string temp = string.Empty;
                    string OldQuene = string.Empty;
                    if (Helper.ContainsKey("_toastquene"))
                    {
                        OldQuene = Helper.GetValue("_toastquene").ToString();
                    }
                    foreach (var item in list)
                    {
                        // temp += item.Sid + " ";
                        if (OldQuene.Contains(item.Sid))
                            continue;
                        OldQuene += item.Sid + ",";
                    }
                    // foreach (var str in Regex.Match(OldQuene, @"\d*(?=@)").Groups) 
                    // {
                    //     string value = str.ToString();
                    //     if (!temp.Contains(value))// 最新的推送列表里没有该番剧的信息，它将被删除
                    //         OldQuene = OldQuene.Replace(value + "@,", "");
                    // }
                    Helper.SetValue("_toastquene", OldQuene);
                    return list;
                }
            }
            return null;
        }
        string TileTemplete = @"
 <tile>
  <visual>
    <binding template='TileMedium'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileWide'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileLarge'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
  </visual>
</tile>";
        string ToastTemplete = @"
<toast launch='{0}'>
	<visual>
		<binding template='ToastGeneric'>
			<text>{1}</text>
			<text>{2}</text>
			<image src='{3}'/>
		</binding>
		<audio src='ms-winsoundevent:Notification.Reminder'/>
	</visual>
</toast>";
        private void UpdateTile(List<Feed_Bangumi> list)
        {
            if (Helper.ContainsKey("_tile"))
            {
                if ((bool)Helper.GetValue("_tile") == false) return;   
            }
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();
            int itemcount = 0;
            foreach (var feed in list)
            {
                XmlDocument xml = new XmlDocument();
                string templete = string.Format(TileTemplete, feed.Pic, feed.Title, feed.New_ep + "\n" + feed.Time);
                xml.LoadXml(templete);
                TileNotification noti = new TileNotification(xml);
                updater.Update(noti);
                if (itemcount++ > 5) break;
            }
         }

        private void SendToast(List<Feed_Bangumi> list)
        {
            if (Helper.GetValue("_toast") == null)
            {
                Helper.SetValue("_toast", true);
            }
            if ((bool)Helper.GetValue("_toast") == false)
                return;
            if (Helper.GetValue("_toastquene") == null)
            {
                Helper.SetValue("_toastquene", string.Empty);
            }
            string UnpulledQuene = Helper.GetValue("_toastquene").ToString();
            foreach (var feed in list)
            {
                if (UnpulledQuene.Contains(feed.Sid))
                {
                    string a = Regex.Match(UnpulledQuene,feed.Sid + @":\d+").Value;
                    if (string.IsNullOrEmpty(a))
                    {
                        // 之前没有推送过该番剧
                        PullToast(feed.Aid, feed.Title, feed.New_ep, feed.Pic);
                        UnpulledQuene = UnpulledQuene.Replace(feed.Sid, feed.Sid + ":" + feed.Aid);
                    }
                    else
                    {
                        // 之前已经推送过
                        string aid = a.Split(':')[1];
                        if (feed.Aid == aid)
                        {
                            // 还是之前的选集
                            continue;
                        }
                        else
                        {
                            PullToast(feed.Aid, feed.Title, feed.New_ep, feed.Pic);
                            UnpulledQuene = UnpulledQuene.Replace(a, feed.Sid + ":" + feed.Aid);
                        }
                    }
                }
            }
            Helper.SetValue("_toastquene", UnpulledQuene);
        }

       /// <summary>
       /// 推送Toast通知
       /// </summary>
       /// <param name="Args">启动参数</param>
       /// <param name="Title">标题</param>
       /// <param name="SubTitle">副标题</param>
       /// <param name="Pic">图片（可选）</param>
        private void PullToast(string Args, string Title, string SubTitle, string Pic = "")
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(string.Format(ToastTemplete, Args, Title, SubTitle, Pic));
            ToastNotification toast = new ToastNotification(xml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async void SendToast()
        {
            try
            {
                string url = "http://message.bilibili.com/api/notify/query.notify.count.do?access_key=" + Helper.GetValue("_accesskey").ToString() + "&actionKey=appkey" + "&appkey=422fd9d7289a1dd9&build=427000&platform=android&ts=" + Helper.GetLinuxTS().ToString();
                url += Helper.GetSign(url);
                JsonObject json = await Helper.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = json["data"].GetObject();
                    if (json.ContainsKey("reply_me"))
                    {
                        string Reply_me = json["reply_me"].ToString();
                        if (Reply_me != "0")
                        {
                            PullToast("m0", string.Format("您收到了{0}条回复", Reply_me), "点击查看");
                        }
                    }
                    if (json.ContainsKey("chat_me"))
                    {
                        string Chat_me = json["chat_me"].ToString();
                        if (Chat_me != "0")
                        {
                            PullToast("m1", string.Format("您收到了{0}条私信", Chat_me), "点击查看");
                        }
                    }
                    if (json.ContainsKey("notify_me"))
                    {
                        string Notify_me = json["notify_me"].ToString();
                        if (Notify_me != "0")
                        {
                            PullToast("m2", string.Format("您收到了{0}条系统通知", Notify_me), "点击查看");
                        }
                    }
                    if (json.ContainsKey("at_me"))
                    {
                        string At_me = json["at_me"].ToString();
                        if (At_me != "0")
                        {
                            PullToast("m3", string.Format("您收到了{0}条@", At_me), "点击查看");
                        }
                    }
                    if (json.ContainsKey("praise_me"))
                    {
                        string Praise_me = json["praise_me"].ToString();
                        if (Praise_me != "0")
                        {
                            PullToast("m3", string.Format("您的评论收到了{0}条赞", Praise_me), "点击查看");
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }

        class Feed_Bangumi
        {
            private string status;
            private string new_ep;
            public string Aid { get; set; }
            public string Pic { get; set; }
            public string Title { get; set; }
            public string Time { get; set; }
            public string New_ep
            {
                get
                {
                    if (new_ep == string.Empty) return string.Empty;
                    if (status == "0") return "更新到" + new_ep + "话";
                    else if (status == "1") return new_ep + "话全";
                    else return "最新话" + new_ep;
                }
                set { new_ep = value; }
            }
            public string Status
            {
                get { return status; }
                set { status = value; }
            }
            public string Sid { get; set; }
        }
    }
}
