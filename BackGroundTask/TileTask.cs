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
            SendToast(list0);
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
        string TileTemplete = @"<tile branding='name'> 
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

        private void UpdateTile(List<Feed_Bangumi> list)
        {
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();
            int itemcount = 0;
            foreach (var feed in list)
            {
                XmlDocument xml = new XmlDocument();
                string templete = string.Format(TileTemplete, feed.Pic, feed.Title, feed.New_ep);
                xml.LoadXml(templete);
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
                var tmp = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);
                var txtNodes = tmp.GetElementsByTagName("text");
                var imageNodes = tmp.GetElementsByTagName("image");
                if (!(txtNodes == null || txtNodes.Length == 0))
                {
                    var txtnode = txtNodes[0];
                    if (!(imageNodes == null || imageNodes.Length == 0))
                    {
                        var imagenode = imageNodes[0];
                        if (imagenode != null)
                        {
                            var attr = imagenode.Attributes[1].NodeValue = feed.Pic;
                        }
                    }
                    txtnode.InnerText = string.Format("【{0}】{1}", feed.Title, feed.New_ep);
                    ToastNotification toast = new ToastNotification(tmp);
                    toast.Tag = feed.Aid;
                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                    //Helper.SetValue("_toastarg", feed.Aid);
                    UnpulledQuene = UnpulledQuene.Replace(feed.Aid, feed.Aid + "@");//后接@表示已推送过
                }
            }
            Helper.SetValue("_toastquene", UnpulledQuene);
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
