using System;
using System.Collections.Generic;
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
            var deferral = taskInstance.GetDeferral();
            List<Feed_Bangumi> list0 = await GetPulls();
            UpdateToast(list0);
            deferral.Complete();
        }

        private async Task<List<Feed_Bangumi>> GetPulls()
        {
            List<Feed_Bangumi> list = new List<Feed_Bangumi>();
            string url = "http://api.bilibili.com/x/feed/pull?type=0&_device=wp&_ulv=10000&build=424000&platform=android&appkey=" + Helper.appkey + "&access_key=" + Helper.GetValue("_accesskey").ToString() + "&pn=1&ps=10&rnd=" + new Random().Next(1000, 2000).ToString();
            url += Helper.GetSign(url);
            JsonObject json = await Helper.GetJson(url);
            if (json.ContainsKey("data"))
            {
                json = json["data"].GetObject();
                if (json.ContainsKey("feeds"))
                {
                    JsonArray array = json["feeds"].GetArray();
                    //磁贴最多更新5个
                    for (int i = 0; i < 5; i++)
                    {
                        var item = array[i];
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
                    return list;
                }
            }
            return null;
        }

        private void UpdateToast(List<Feed_Bangumi> list)
        {
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();
            int itemcount = 0;
            foreach (var feed in list)
            {
                XmlDocument xml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150PeekImage01);
                var textNodes = xml.GetElementsByTagName("text");
                textNodes[0].InnerText = feed.Title + "\n" + feed.New_ep + "\n" + feed.Time;
                var imagenodes = xml.GetElementsByTagName("image");
                ((XmlElement)imagenodes[0]).SetAttribute("src", feed.Pic);
                TileNotification tileNoti = new TileNotification(xml);
                //tileNoti.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(15);
                updater.Update(tileNoti);
                if (itemcount++ > 5) break;
            }
        }
        //public static object GetJsonValue(JsonObject json, string key)
        //{
        //    if (!json.ContainsKey(key))
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        var value = json[key];
        //        JsonValueType type = value.ValueType;
        //        switch (type)
        //        {
        //            case JsonValueType.String: return value.GetString();
        //            case JsonValueType.Object: return value.GetObject();
        //            case JsonValueType.Array: return value.GetArray();
        //            case JsonValueType.Null: return null;
        //            default: return value.ValueType;
        //        }
        //    }
        //}

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
