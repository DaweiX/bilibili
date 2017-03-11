using bilibili.Helpers;
using bilibili.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace bilibili.Http.ContentService
{
   /// <summary>
   /// 用户信息相关的数据服务类
   /// </summary>
    class UserRelated
    {
       /// <summary>
       /// 获取基本信息
       /// </summary>
        public static async Task<Site_UserInfo> GetBasicInfoAsync(string mid)
        {
            string url = "http://space.bilibili.com/ajax/member/GetInfo?rnd=" + new Random().Next(1000, 2000).ToString();
            string result = await BaseService.SendPostAsync(url, "mid=" + mid, "http://space.bilibili.com/" + mid);
            Site_UserInfo user = JsonConvert.DeserializeObject<Site_UserInfo>(result);
            return user;
        }

       /// <summary>
       /// 获取用户设置
       /// </summary>
        public static async Task<Site_UserSettings> GetUserSettingAsync(string mid)
        {
            string url = "http://space.bilibili.com/ajax/settings/getSettings?mid=" + mid;
            string result = await BaseService.SentGetAsync(url);
            Site_UserSettings sets = JsonConvert.DeserializeObject<Site_UserSettings>(result);
            return sets;
        }

       /// <summary>
       /// 获取投稿视频
       /// </summary>
        public static async Task<List<MyVideo>> GetMyVideoAsync(string mid, int page, int pagesize = 20)
        {
            string url = "http://space.bilibili.com/ajax/member/getSubmitVideos?mid=" + mid + "&pagesize=" + pagesize + "&page=" + page;
            url += ApiHelper.GetSign(url);
            string result = await BaseService.SentGetAsync(url);
            Site_MyVideo content = JsonConvert.DeserializeObject<Site_MyVideo>(result);
            if (content.Status == true)
            {
                return content.Data.Vlist;
            }
            return null;
        }

       /// <summary>
       /// 获取订阅番剧
       /// </summary>
        public static async Task<Site_Concern> GetConcernBangumiAsync(string mid, int page, bool isself,int pagesize = 20)
        {
            Site_Concern site = new Site_Concern();
            if (isself == false)
            {
                string url = "http://space.bilibili.com/ajax/Bangumi/getList?mid=" + mid + "&pagesize=" + pagesize.ToString() + "&page=" + page.ToString();
                url += ApiHelper.GetSign(url);
                string result = await BaseService.SentGetAsync(url);
                JsonObject json = JsonObject.Parse(result);
                bool status = json["status"].GetBoolean();
                if (status == false)
                {
                    if (json.ContainsKey("data"))
                    {
                        if (json["data"].GetString() == "用户隐私设置未公开")
                        {
                            List<ConcernItem> list = new List<ConcernItem>();
                            list.Add(new ConcernItem { Title = "PRIVATE" });
                            site.Result = list;
                            return site;
                        }
                    }
                    return null;
                }
                else
                {
                    if (json.ContainsKey("data"))
                    {
                        json = json["data"].GetObject();
                        if (json.ContainsKey("result"))
                        {
                            site.Result = JsonConvert.DeserializeObject<List<ConcernItem>>(json["result"].ToString());
                        }
                    }
                    return null;
                }
            }
            else
            {
                string url = "http://bangumi.bilibili.com/api/get_concerned_season?_device=wp&_ulv=10000&build=430000&platform=android&scale=xhdpi&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey + "&page=" + page.ToString() + "&pagesize=" + pagesize.ToString() + "&ts=" + ApiHelper.GetLinuxTS().ToString();
                url += ApiHelper.GetSign(url);
                Site_Concern concern = JsonConvert.DeserializeObject<Site_Concern>(await BaseService.SentGetAsync(url));
                if (concern.Code == "0")
                {
                    site = concern;
                    return site;
                }
                return null;
            }
        }

       /// <summary>
       /// 获取关注的人
       /// </summary>
        public static async Task<List<Friend>> GetFriendsAsync(string mid, int page)
        {
            string url = "http://space.bilibili.com/ajax/friend/GetAttentionList?mid=" + mid + "&pagesize=30&page=" + page.ToString();
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {
                json = json["data"].GetObject();
                if (json.ContainsKey("list"))
                {
                    return JsonConvert.DeserializeObject<List<Friend>>(json["list"].ToString());
                }
            }
            return null;
        }
    }
}
