using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BackgroundTask
{
    //windows运行时组件中，public方法一定要声明为密封类
    public sealed class TileTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //Helper.SetValue("_toastquene", "");
            var deferral = taskInstance.GetDeferral();
            List<Feed_Bangumi> list0 = await GetPulls();
            UpdateTile(list0);
            //这块写得有点难看
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
            deferral.Complete();
        }

        private async Task<List<Feed_Bangumi>> GetPulls()
        {
            List<Feed_Bangumi> list = new List<Feed_Bangumi>();
            string url = "http://api.bilibili.com/x/feed/pull?type=0&_device=wp&_ulv=10000&build=424000&platform=android&appkey=" + Helper.appkey + "&access_key=" + Helper.GetValue("_accesskey").ToString() + "&pn=1&ps=20&rnd=" + new Random().Next(1000, 2000).ToString();
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
                        }
                        if (json.ContainsKey("source"))
                        {
                            JsonObject json2 = json["source"].GetObject();
                            if (json2.ContainsKey("new_ep"))
                            {
                                json2 = json2["new_ep"].GetObject();
                                if (json2.ContainsKey("av_id"))
                                {
                                    feed.Aid = json2["av_id"].GetString();
                                }
                                if (json2.ContainsKey("cover"))
                                {
                                    feed.Pic = json2["cover"].GetString();
                                }
                                if (json2.ContainsKey("update_time"))
                                {
                                    feed.Time = json2["update_time"].GetString();
                                }
                                if (json2.ContainsKey("index"))
                                {
                                    feed.New_ep = json2["index"].GetString();
                                }
                                list.Add(feed);
                            }
                        }
                    }
                    string temp = string.Empty;
                    string OldQuene = Helper.GetValue("_toastquene").ToString();
                    foreach (var item in list)
                    {
                        temp += item.Aid + " ";
                        if (OldQuene.Contains(item.Aid))
                            continue;
                        OldQuene += item.Aid + ",";
                    }
                    foreach (var str in Regex.Match(OldQuene, @"\d*(?=@)").Groups) 
                    {
                        string value = str.ToString();
                        if (!temp.Contains(value))//最新的推送列表里没有该番剧的信息，它将被删除
                            OldQuene = OldQuene.Replace(value + "@,", "");
                    }
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
                return;
            if ((bool)Helper.GetValue("_toast") == false)
                return;
            if (Helper.GetValue("_toastquene") == null)
                return;
            string UnpulledQuene = Helper.GetValue("_toastquene").ToString();
            foreach (var feed in list)
            {
                if (UnpulledQuene.Contains(feed.Aid + "@"))
                    continue;
                //Helper.SetValue("_toastarg", feed.Aid);
                PullToast(feed.Aid, feed.Title, feed.New_ep, feed.Pic);
                UnpulledQuene = UnpulledQuene.Replace(feed.Aid, feed.Aid + "@");//后接@表示已推送过
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
        }
    }
}
