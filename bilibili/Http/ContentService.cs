using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Popups;
using bilibili.Helpers;
using bilibili.Methods;
using bilibili.Models;
using Windows.Web.Http;
using Windows.Data.Xml.Dom;
using System.Text.RegularExpressions;
using System.Net;

namespace bilibili.Http
{
    class ContentServ
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static async Task<List<Content>> GetContentAsync(int tid, int page, int pagesize = 20, int order = 2)
        {
            string ord = string.Empty;
            switch(order)
            {
                case 1: ord = "default";break;
                case 2: ord = "hot"; break;
                case 3: ord = "review"; break;
            }
            string url = "http://api.bilibili.com/list?_device=wp&_ulv=10000&build=424000&platform=android&appkey=" + ApiHelper.appkey + "&tid=" + tid.ToString() + "&page=" + page.ToString() + "&pagesize=" + pagesize.ToString() + "&order=" + ord + "&ver=2";
            url += ApiHelper.GetSign(url);
            JsonObject json = new JsonObject();
            List<Content> contentList = new List<Content>();
            try
            {
                json = await BaseService.GetJson(url);
                if (json.ContainsKey("list"))
                {
                    var list = json["list"];
                    if (list != null)
                    {
                        var json2 = JsonObject.Parse(list.ToString());
                        for (int i = 0; i < 20; i++)
                        {
                            if (json2.ContainsKey(string.Format("{0}", i)))
                            {
                                var item = json2[string.Format("{0}", i)];
                                var json3 = JsonObject.Parse(item.ToString());
                                Content myContent = new Content();
                                if (json3.ContainsKey("title"))
                                {
                                    myContent.Title = json3["title"].GetString();
                                    if (json3.ContainsKey("pic"))
                                        myContent.Pic = json3["pic"].GetString();
                                    if (json3.ContainsKey("aid"))
                                        myContent.Num = json3["aid"].GetString();
                                    if (json3.ContainsKey("play"))
                                        myContent.Play = json3["play"].ToString();
                                    if (json3.ContainsKey("comment"))
                                        myContent.Comment = json3["comment"].ToString();
                                    contentList.Add(myContent);
                                }
                            }
                        }
                    }
                }
                return contentList;
            }
            catch
            {
                return null;
            }
        }

        public async static Task<List<Concern>> GetFriendsCons(string mid,int page)
        {
            List<Concern> mylist = new List<Concern>();
            string url = "http://space.bilibili.com/ajax/Bangumi/getList?mid=" + mid + "&pagesize=20&page=" + page.ToString();
            url += ApiHelper.GetSign(url);
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("status"))
            {
                if (json["status"].GetBoolean() == false)
                {
                    //用户隐私设置为不公开
                    return mylist;
                }
            }
            if (json.ContainsKey("data"))
            {
                json = JsonObject.Parse(json["data"].ToString());
                if (json.ContainsKey("result"))
                {
                    JsonArray array = json["result"].GetArray();
                    foreach (var item in array)
                    {
                        Concern cont = new Concern();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("season_id"))
                            cont.ID = StringDeal.delQuotationmarks(temp["season_id"].ToString());
                        if (temp.ContainsKey("newest_ep_index"))
                            cont.New = temp["newest_ep_index"].ToString();
                        if (temp.ContainsKey("is_finish"))
                            cont.isFinish = temp["is_finish"].ToString();
                        if (temp.ContainsKey("cover"))
                            cont.Cover = temp["cover"].GetString();
                        if (temp.ContainsKey("title"))
                            cont.Title = temp["title"].GetString();
                        mylist.Add(cont);
                    }
                }                
            }
            return mylist;
        }

        /// <summary>
        /// 获取热搜
        /// </summary>
        /// <returns></returns>
        public async static Task<List<KeyWord>> GetHotSearchAsync()
        {
            List<KeyWord> hots = new List<KeyWord>();
            string url = "http://s.search.bilibili.com/main/hotword?appkey=" + ApiHelper.appkey + "&build=427000&platform=wp";
            url += ApiHelper.GetSign(url);
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("list"))
            {
                JsonArray array = json["list"].GetArray();
                foreach (var item in array)
                {
                    KeyWord hot = new KeyWord();
                    JsonObject temp = JsonObject.Parse(item.ToString());
                    if (temp.ContainsKey("keyword"))
                    {
                        hot.Keyword = temp["keyword"].GetString();
                    }
                    if (temp.ContainsKey("status"))
                    {
                        hot.Status = temp["status"].GetString();
                    }
                    hots.Add(hot);
                }
            }
            return hots;
        }

        /// <summary>
        /// 获取投稿视频
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static async Task<List<Content>> GetMyVideoAsync(string mid, int page, int pagesize = 20)
        {
            string url = "http://api.bilibili.com/list?type=json&appkey=" + ApiHelper.appkey + "&mid=" + mid + "&page=" + page.ToString() + "&pagesize=" + pagesize.ToString() + "&platform=wp&rnd=" + new Random().Next(3000, 6000).ToString() + "&access_key=" + ApiHelper.accesskey;
            url += ApiHelper.GetSign(url);
            JsonObject json = new JsonObject();
            List<Content> contentList = new List<Content>();
            try
            {
                json = await BaseService.GetJson(url);
                if (json.ContainsKey("list"))
                {
                    var list = json["list"];
                    if (list != null)
                    {
                        var json2 = JsonObject.Parse(list.ToString());
                        for (int i = 0; i < 20; i++)
                        {
                            if (json2.ContainsKey(string.Format("{0}", i)))
                            {
                                var item = json2[string.Format("{0}", i)];
                                var json3 = JsonObject.Parse(item.ToString());
                                Content myContent = new Content();
                                if (json3.ContainsKey("title"))
                                {
                                    myContent.Title = json3["title"].GetString();
                                    if (json3.ContainsKey("pic"))
                                        myContent.Pic = json3["pic"].GetString();
                                    if (json3.ContainsKey("aid"))
                                        myContent.Num = json3["aid"].GetString();
                                    if (json3.ContainsKey("play"))
                                        myContent.Play = json3["play"].ToString();
                                    if (json3.ContainsKey("comment"))
                                        myContent.Comment = json3["comment"].ToString();
                                    contentList.Add(myContent);
                                }
                            }
                        }
                    }
                }
                return contentList;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取视频详情
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<Details> GetDetailsAsync(string url)
        {
            Details details = new Details();
            details.Tags = new List<string>();
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("code"))
                if (json["code"].ToString() == "-404")
                    return null;
            if (json.ContainsKey("data"))
            {
                List<Pages> pages = new List<Pages>();
                JsonObject json2 = JsonObject.Parse(json["data"].ToString());
                details.Desc = json2.ContainsKey("desc") ? json2["desc"].GetString() : "";
                if (json2.ContainsKey("aid"))
                    details.Aid = json2["aid"].ToString();
                if (json2.ContainsKey("ctime"))
                    details.Time = StringDeal.LinuxToData(json2["ctime"].ToString());
                if (json2.ContainsKey("title"))
                    details.Title = StringDeal.delQuotationmarks(json2["title"].ToString());
                if (json2.ContainsKey("pic"))
                    details.Pic = StringDeal.delQuotationmarks(json2["pic"].ToString());
                if (json2.ContainsKey("duration"))
                    details.Duration = Convert.ToInt32(json2["duration"].ToString());
                if (json2.ContainsKey("req_user"))
                {
                    JsonObject j = json2["req_user"].GetObject();
                    if (j.ContainsKey("favorite"))
                    {
                        details.IsFav = j["favorite"].ToString();
                    }
                }
                if (json2.ContainsKey("pages"))
                {
                    JsonArray array = json2["pages"].GetArray();
                    foreach (var item in array)
                    {
                        Pages page = new Pages();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("cid"))
                            page.Cid = temp["cid"].ToString();
                        if (temp.ContainsKey("part"))
                            page.Part = StringDeal.delQuotationmarks(temp["part"].ToString()).Length == 0 ? details.Title : StringDeal.delQuotationmarks(temp["part"].ToString());
                        pages.Add(page);
                    }
                    details.Ps = pages;

                }
                if (json2.ContainsKey("owner"))
                {
                    JsonObject json2_owner = JsonObject.Parse(json2["owner"].ToString());
                    if (json2_owner.ContainsKey("name"))
                        details.Upzhu = StringDeal.delQuotationmarks(json2_owner["name"].ToString());
                    if (json2_owner.ContainsKey("mid"))
                        details.Mid = json2_owner["mid"].ToString();
                }
                if (json2.ContainsKey("stat"))
                {
                    JsonObject json2_stat = JsonObject.Parse(json2["stat"].ToString());
                    if (json2_stat.ContainsKey("view"))
                        details.View = json2_stat["view"].ToString();
                    if (json2_stat.ContainsKey("danmaku"))
                        details.Danmu = json2_stat["danmaku"].ToString();
                    if (json2_stat.ContainsKey("share"))
                        details.Share = json2_stat["share"].ToString();
                    if (json2_stat.ContainsKey("favorite"))
                        details.Fav = json2_stat["favorite"].ToString();
                    if (json2_stat.ContainsKey("coin"))
                        details.Coins = json2_stat["coin"].ToString();
                    if (json2_stat.ContainsKey("reply"))
                        details.Reply = json2_stat["reply"].ToString();
                }
                if (json2.ContainsKey("tags"))
                {
                    //可能出现tags=(null)
                    try
                    {
                        var a = json2["tags"].GetArray();
                        foreach (var item in json2["tags"].GetArray())
                        {
                            details.Tags.Add(item.GetString());
                        }
                    }
                    catch { }
                }
            }
            return details;
        }
        public enum SearchType
        {
            video,
            bangumi,
            //up,
            //live
        }
        /// <summary>
        /// 获取搜索结果_视频
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<List<SearchResult>> GetSearchResultAsync(string keyword, int page, string pagesize = "20")
        {
            try
            {
                string url = "http://api.bilibili.com/search?_device=wp&_ulv=10000&build=424000&platform=android&appkey=" + ApiHelper.appkey + "&main_ver=v3&page=" + page.ToString() + "&pagesize=" + pagesize + "&search_type=video&source_type=0&keyword=" + keyword;
                url += ApiHelper.GetSign(url);
                List<SearchResult> contentList = new List<SearchResult>();
                JsonObject json = new JsonObject();
                json = await BaseService.GetJson(url);
                if (json.ContainsKey("result"))
                {
                    var Myarray = json["result"].GetArray();
                    if (Myarray != null)
                    {
                        foreach (var item in Myarray)
                        {
                            SearchResult rs = new SearchResult();
                            JsonObject json2 = JsonObject.Parse(item.ToString());
                            if (json2.ContainsKey("aid"))
                                rs.Aid = json2["aid"].ToString();
                            if (json2.ContainsKey("author"))
                                rs.Author = json2["author"].GetString();
                            if (json2.ContainsKey("pic"))
                                rs.Pic = json2["pic"].GetString();
                            if (json2.ContainsKey("play"))
                                rs.View = json2["play"].ToString();
                            if (json2.ContainsKey("typename"))
                                rs.TypeName = json2["typename"].GetString();
                            if (json2.ContainsKey("title"))
                                rs.Title = json2["title"].GetString();
                            if (json2.ContainsKey("description"))
                                rs.Desc = json2["description"].GetString();
                            contentList.Add(rs);
                        }
                    }
                }
                return contentList;
            }
            catch
            {
                return null;
            }
        }
   
        /// <summary>
        /// 获取直播（头部）
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Live>> GetCommentLiveAsync()
        {
            List<Live> list = new List<Live>();
            string url = "http://live.bilibili.com/AppIndex/home?_device=wp&_ulv=10000&access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&build=411005&platform=android&scale=xxhdpi&rnd=" + new Random().Next(1000, 3000).ToString();
            url += ApiHelper.GetSign(url);
            try
            {
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = JsonObject.Parse(json["data"].ToString());
                    if (json.ContainsKey("recommend_data"))
                    {
                        json = JsonObject.Parse(json["recommend_data"].ToString());
                        if (json.ContainsKey("lives"))
                        {
                            JsonArray array = json["lives"].GetArray();
                            foreach (var item in array)
                            {
                                Live live = new Live();
                                json = item.GetObject();
                                if (json.ContainsKey("title"))
                                {
                                    live.Title = json["title"].GetString();
                                }
                                if (json.ContainsKey("playurl"))
                                {
                                    live.PlayUrl = json["playurl"].GetString();
                                }
                                if (json.ContainsKey("online"))
                                {
                                    live.Online = json["online"].ToString();
                                }
                                if (json.ContainsKey("cover"))
                                {
                                    JsonObject json2 = json["cover"].GetObject();
                                    if (json2.ContainsKey("src"))
                                    {
                                        live.Cover = json2["src"].GetString();
                                    }
                                }
                                if (json.ContainsKey("owner"))
                                {
                                    JsonObject json2 = json["owner"].GetObject();
                                    if (json2.ContainsKey("face"))
                                    {
                                        live.Face = json2["face"].GetString();
                                    }
                                    if (json2.ContainsKey("name"))
                                    {
                                        live.Name = json2["name"].GetString();
                                    }
                                }
                                list.Add(live);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return list;
        }

        /// <summary>
        /// 获取搜索结果_番剧
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<List<SearchResult_Bangumi>> GetBangumisAsync(string keyword, int page, string pagesize = "20")
        {
            try
            {
                string url = "http://api.bilibili.com/search?_device=wp&_ulv=10000&build=424000&platform=android&access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&main_ver=v3&page=" + page.ToString() + "&pagesize=" + pagesize + "&search_type=bangumi&source_type=0&keyword=" + keyword;
                url += ApiHelper.GetSign(url);
                List<SearchResult_Bangumi> contentList = new List<SearchResult_Bangumi>();
                JsonObject json = new JsonObject();
                json = await BaseService.GetJson(url);
                if (json.ContainsKey("result"))
                {
                    var Myarray = json["result"].GetArray();
                    foreach (var item in Myarray)
                    {
                        SearchResult_Bangumi bang = new SearchResult_Bangumi();
                        JsonObject json2 = JsonObject.Parse(item.ToString());
                        if (json2.ContainsKey("cover"))
                            bang.Cover = json2["cover"].GetString();
                        if (json2.ContainsKey("is_finish"))
                            bang.IsFinish = json2["is_finish"].ToString();
                        if (json2.ContainsKey("season_id"))
                            bang.ID = json2["season_id"].ToString();
                        if (json2.ContainsKey("title"))
                            bang.Title = json2["title"].GetString();
                        if (json2.ContainsKey("total_count"))
                            bang.Count = json2["total_count"].ToString();
                        if (json2.ContainsKey("evaluate"))
                            bang.Evaluate = json2["evaluate"].GetString();
                        if (bang.ID.Length > 0)
                            contentList.Add(bang);
                    }
                }
                return contentList;
            }
           catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取搜索结果_up主
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<List<UpForSearch>> GetUpsAsync(string keyword, int page, string pagesize = "20")
        {
            string url = "http://app.bilibili.com/x/v2/search/type?keyword=" + keyword + "&pn=" + page.ToString() + "&ps=20&type=2&appkey=" + ApiHelper.appkey + "&build=429001&mobi_app=win&platform=android";
            url += ApiHelper.GetSign(url);
            List<UpForSearch> upList = new List<UpForSearch>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {
                json = JsonObject.Parse(json["data"].ToString());
                if (json.ContainsKey("items"))
                {
                    var Myarray = json["items"].GetArray();
                    foreach (var item in Myarray)
                    {
                        JsonObject json2 = JsonObject.Parse(item.ToString());
                        UpForSearch up = new UpForSearch();
                        if (json2.ContainsKey("archives"))
                        {
                            up.Archives = json2["archives"].ToString();
                        }
                        if (json2.ContainsKey("cover"))
                        {
                            up.Cover = json2["cover"].GetString();
                        }
                        if (json2.ContainsKey("title"))
                        {
                            up.Title = json2["title"].GetString();
                        }
                        if (json2.ContainsKey("fans"))
                        {
                            up.Fans = json2["fans"].ToString();
                        }
                        if (json2.ContainsKey("param"))
                        {
                            up.Param = json2["param"].GetString();
                        }
                        upList.Add(up);
                    }
                }
                return upList;
            }
            return null;
        }

        /// <summary>
        /// 获取番剧详情
        /// </summary>
        /// <param name="url"></param>
        /// <param name="i">1:总览，2:演员，3:列表</param>
        /// <returns></returns>
        public static async Task<object> GetSeasonResultAsync(string url, int i)
        {
            JsonObject json = new JsonObject();
            switch (i)
            {
                case 1:
                    {
                        try
                        {
                            json = await BaseService.GetJson(url);
                            Season_Total season = new Season_Total();
                            if (json.ContainsKey("result"))
                            {
                                JsonObject json2 = JsonObject.Parse(json["result"].ToString());
                                if (json2.ContainsKey("coins"))
                                    season.Coins = StringDeal.delQuotationmarks(json2["coins"].ToString());
                                if (json2.ContainsKey("danmaku_count"))
                                    season.Danmaku = StringDeal.delQuotationmarks(json2["danmaku_count"].ToString());
                                if (json2.ContainsKey("copyright"))
                                    season.Copyright = json2["copyright"].GetString();
                                if (json2.ContainsKey("cover"))
                                    season.Cover = json2["cover"].GetString();
                                if (json2.ContainsKey("bangumi_title"))
                                    season.Title = json2["bangumi_title"].GetString();
                                if (json2.ContainsKey("evaluate"))
                                    season.Brief = json2["evaluate"].GetString();
                                if (json2.ContainsKey("staff"))
                                    season.Staff = json2["staff"].GetString();
                                if (json2.ContainsKey("favorites"))
                                    season.Fav = StringDeal.delQuotationmarks(json2["favorites"].ToString());
                                if (json2.ContainsKey("play_count"))
                                    season.View = StringDeal.delQuotationmarks(json2["play_count"].ToString());
                                if (json2.ContainsKey("weekday"))
                                    season.WeekDay = json2["weekday"].ToString().ToCharArray()[0];
                                if (json2.ContainsKey("pub_time"))
                                    season.Time = StringDeal.delQuotationmarks(json2["pub_time"].ToString());
                                if (json2.ContainsKey("is_finish"))
                                    season.isFinish = json2["is_finish"].ToString() == "0" ? false : true;
                                if (json2.ContainsKey("user_season"))
                                {
                                    JsonObject j = json2["user_season"].GetObject();
                                    if (j.ContainsKey("attention"))
                                    {
                                        season.IsConcerned = j["attention"].GetString();
                                    }
                                }
                                if (json2.ContainsKey("tags"))
                                {
                                    season.Tags = new List<string>();
                                    foreach (var item in json2["tags"].GetArray())
                                    {
                                        JsonObject temp = JsonObject.Parse(item.ToString());
                                        if (temp.ContainsKey("tag_name"))
                                            season.Tags.Add(StringDeal.delQuotationmarks(temp["tag_name"].ToString()));
                                    }
                                }
                            }
                            return season;
                        }
                        catch (Exception e)
                        {
                            await new MessageDialog(e.Message).ShowAsync();
                            return null;
                        }
                    }
                case 2:
                    {
                        try
                        {
                            json = await BaseService.GetJson(url);
                            List<Season_actor> mylist = new List<Season_actor>();
                            if (json.ContainsKey("result"))
                            {
                                JsonObject json2 = JsonObject.Parse(json["result"].ToString());
                                JsonArray Array_ac = json2["actor"].GetArray();
                                foreach (var item in Array_ac)
                                {
                                    Season_actor actor = new Season_actor();
                                    JsonObject json3 = JsonObject.Parse(item.ToString());
                                    if (json3.ContainsKey("actor"))
                                        actor.Actor = StringDeal.delQuotationmarks(json3["actor"].ToString());
                                    if (json3.ContainsKey("role"))
                                        actor.Role = StringDeal.delQuotationmarks(json3["role"].ToString());
                                    mylist.Add(actor);
                                }
                            }
                            return mylist;
                        }
                        catch (Exception e)
                        {
                            await new MessageDialog(e.Message).ShowAsync();
                            return null;
                        }
                    }
                case 3:
                    {
                        try
                        {
                            List<Season_episodes> indexList = new List<Season_episodes>();
                            json = await BaseService.GetJson(url);
                            if (json.ContainsKey("result"))
                            {
                                JsonObject json2 = JsonObject.Parse(json["result"].ToString());
                                Season_episodes season;
                                if (json2.ContainsKey("episodes"))
                                {
                                    JsonArray Array_rs = json2["episodes"].GetArray();
                                    foreach (var item in Array_rs)
                                    {
                                        season = new Season_episodes();
                                        JsonObject json3 = JsonObject.Parse(item.ToString());
                                        if (json3.ContainsKey("av_id"))
                                            season.ID = StringDeal.delQuotationmarks(json3["av_id"].ToString());
                                        if (json3.ContainsKey("coins"))
                                            season.Coins = StringDeal.delQuotationmarks(json3["coins"].ToString());
                                        if (json3.ContainsKey("cover"))
                                            season.Cover = StringDeal.delQuotationmarks(json3["cover"].ToString());
                                        if (json3.ContainsKey("danmaku"))
                                            season.Danmaku = StringDeal.delQuotationmarks(json3["danmaku"].ToString());
                                        if (json3.ContainsKey("index"))
                                            season.Index = StringDeal.delQuotationmarks(json3["index"].ToString());
                                        if (json3.ContainsKey("index_title"))
                                            season.Title = StringDeal.delQuotationmarks(json3["index_title"].ToString());
                                        if (json3.ContainsKey("update_time"))
                                            season.Time = StringDeal.delQuotationmarks(json3["update_time"].ToString()).Split(' ')[0];
                                        indexList.Add(season);
                                    }
                                }
                            }
                            return indexList;
                        }
                        catch (Exception e)
                        {
                            await new MessageDialog(e.Message).ShowAsync();
                            return null;
                        }
                    }
                default: return null;
            }
        }
      /// <summary>
      /// 获取视频源地址及相关信息
      /// </summary>
      /// <param name="cid"></param>
      /// <param name="quality"></param>
      /// <param name="format"></param>
      /// <returns></returns>
        public static async Task<VideoURL> GetVedioURL(string cid, int quality, VideoFormat format)
        {
            VideoURL URL = new VideoURL { Acceptformat = new List<string>(), Acceptquality = new List<int>() };
            JsonObject json = new JsonObject();
            string url = string.Format("http://interface.bilibili.com/playurl?_device=uwp&cid={0}&quality={1}&otype=xml&appkey={2}&_buvid=D57C3D63-7920-41FA-910D-AB6CBD5365F830799infoc&_hwid=0100d4c50200c2a6&platform=uwp_mobile&type=mp4&access_key={3}&mid={4}&ts={5}", cid, quality.ToString(), ApiHelper.appkey, ApiHelper.accesskey, UserHelper.mid, ApiHelper.GetLinuxTS().ToString());
            url += ApiHelper.GetSign(url);
            XmlDocument doc = await XmlDocument.LoadFromUriAsync(new Uri(url));
            URL.Url = doc.GetElementsByTagName("url")[0].InnerText;
            URL.Size = doc.GetElementsByTagName("size")[0].InnerText;
            URL.Length = doc.GetElementsByTagName("length")[0].InnerText;
            #region
            //XmlElement ele = durl as XmlElement;
            //if(ele.)
            //json = await BaseService.GetJson(url);
            //try
            //{
            //    if (json.ContainsKey("accept_format"))
            //    {
            //        foreach (string item in json["accept_format"].GetString().Split(','))
            //        {
            //            URL.Acceptformat.Add(item);
            //        }
            //    }
            //    if (json.ContainsKey("accept_quality"))
            //    {
            //        foreach (var item in json["accept_quality"].GetArray())
            //        {
            //            URL.Acceptquality.Add(Convert.ToInt32(item.ToString()));
            //        }
            //    }
            //    if (json.ContainsKey("durl"))
            //    {
            //        json = JsonObject.Parse(json["durl"].GetArray()[0].ToString());
            //        if (json.ContainsKey("backup_url"))
            //        {
            //            URL.BackupUrl = json["backup_url"].GetArray()[0].GetString();
            //        }
            //        if (json.ContainsKey("url"))
            //        {
            //            URL.Url = json["url"].GetString();
            //        }
            //        if (json.ContainsKey("size"))
            //        {
            //            URL.Size = json["size"].ToString();
            //        }
            //        if (json.ContainsKey("length"))
            //        {
            //            URL.Length = json["length"].ToString();
            //        }
            //    }
            #endregion
            return URL;
            //}
            //catch
            //{
            //    return null;
            //}
        }
        public static async Task<string> GetHashAsync(string url)
        {
            string str = "";
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("hash"))
            {
                str = json["hash"].ToString();
            }
            if (json.ContainsKey("key"))
            {
                str += "@" + json["key"].ToString();
            }
            return str;
        }
        /// <summary>
        /// 获取番剧索引
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<List<Tags>> GetTagsAsync(string url)
        {
            List<Tags> tagList = new List<Tags>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("result"))
            {
                var a = json["result"].GetArray();
                foreach (var item in a)
                {
                    Tags tag = new Tags();
                    JsonObject temp = JsonObject.Parse(item.ToString());
                    if (temp.ContainsKey("cover"))
                        tag.Cover = StringDeal.delQuotationmarks(temp["cover"].ToString());
                    if (temp.ContainsKey("tag_id"))
                        tag.TagID = StringDeal.delQuotationmarks(temp["tag_id"].ToString());
                    if (temp.ContainsKey("tag_name"))
                        tag.TagName = StringDeal.delQuotationmarks(temp["tag_name"].ToString());
                    tagList.Add(tag);
                }
            }
            return tagList;
        }
        /// <summary>
        /// 根据标签获取番剧
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<List<Models.Bangumi>> GetBansByTagAsync(string url)
        {
            List<Models.Bangumi> banList = new List<Models.Bangumi>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("result"))
            {
                JsonObject json2 = JsonObject.Parse(json["result"].ToString());
                if (json2.ContainsKey("list"))
                {
                    var a = json2["list"].GetArray();
                    foreach (var item in a)
                    {
                        Models.Bangumi ban = new Models.Bangumi();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("cover"))
                            ban.Cover = temp["cover"].GetString();
                        if (temp.ContainsKey("is_finish"))
                            ban.IsFinish = StringDeal.delQuotationmarks(temp["is_finish"].ToString()) == "1" ? true : false;
                        if (temp.ContainsKey("brief"))
                            ban.Brief = temp["brief"].GetString();
                        if (temp.ContainsKey("season_id"))
                            ban.ID = StringDeal.delQuotationmarks(temp["season_id"].ToString());
                        if (temp.ContainsKey("total_count"))
                            ban.Count = StringDeal.delQuotationmarks(temp["total_count"].ToString());
                        if (temp.ContainsKey("title"))
                            ban.Title = StringDeal.delQuotationmarks(temp["title"].ToString());
                        if (temp.ContainsKey("pub_time"))
                            ban.Time = StringDeal.delQuotationmarks(temp["pub_time"].ToString()).Split(' ')[0];
                        var ads = temp["is_finish"].ToString();
                        banList.Add(ban);
                    }
                }
            }
            return banList;
        }

        /// <summary>
        /// 获取番剧最近更新
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LastUpdate>> GetLastUpdateAsync()
        {
            List<LastUpdate> list = new List<LastUpdate>();
            string url = "http://bangumi.bilibili.com/api/app_index_page_v2";
            try
            {
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("result"))
                {
                    json = JsonObject.Parse(json["result"].ToString());
                    if (json.ContainsKey("latestUpdate"))
                    {
                        json = JsonObject.Parse(json["latestUpdate"].ToString());
                        if (json.ContainsKey("list"))
                        {
                            JsonArray array = json["list"].GetArray();
                            foreach (var item in array)
                            {
                                LastUpdate last = new LastUpdate();
                                json = JsonObject.Parse(item.ToString());
                                if (json.ContainsKey("cover"))
                                {
                                    last.Cover = json["cover"].GetString();
                                }
                                if (json.ContainsKey("title"))
                                {
                                    last.Title = json["title"].GetString();
                                }
                                if (json.ContainsKey("watchingCount"))
                                {
                                    last.Watch = json["watchingCount"].GetString();
                                }
                                if (json.ContainsKey("newest_ep_index"))
                                {
                                    last.Index = json["newest_ep_index"].GetString();
                                }
                                if (json.ContainsKey("season_id"))
                                {
                                    last.Sid = json["season_id"].GetString();
                                }
                                if (json.ContainsKey("last_time"))
                                {
                                    last.Time = StringDeal.LinuxToData(json["last_time"].GetString());
                                }
                                list.Add(last);
                            }
                        }
                    }
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取话题
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task<List<Topic>> GetTopicListAsync(int page)
        {
            string url = string.Format("http://api.bilibili.com/topic/getlist?access_key={0}&appkey={1}&build=424000&mobi_app=android&page={2}&pagesize=20&platform=android&ts={3}", ApiHelper.accesskey, ApiHelper.appkey, page.ToString(), ApiHelper.GetLinuxTS().ToString());
            url += ApiHelper.GetSign(url);
            List<Topic> list = new List<Topic>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("list"))
            {
                JsonArray array = json["list"].GetArray();
                foreach (var item in array)
                {
                    Topic topic = new Topic();
                    JsonObject temp = JsonObject.Parse(item.ToString());
                    if (temp.ContainsKey("title"))
                        topic.Name = temp["title"].GetString();
                    if (temp.ContainsKey("link"))
                        topic.Url = temp["link"].GetString();
                    if (temp.ContainsKey("cover"))
                        topic.Pic = temp["cover"].GetString();
                    if (topic.Url.Length > 0) 
                        list.Add(topic);
                }
            }
            return list;
        }
        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task<List<Event>> GetEventListAsync(int page)
        {
            string url = string.Format("http://api.bilibili.com/event/getlist?appkey={0}&build=422000&mobi_app=android&page={1}&pagesize=20&platform=android&ts={2}", ApiHelper.appkey, page.ToString(), ApiHelper.GetLinuxTS().ToString());
            url += ApiHelper.GetSign(url);
            List<Event> list = new List<Event>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("list"))
            {
                JsonArray array = json["list"].GetArray();
                foreach (var item in array)
                {
                    Event topic = new Event();
                    JsonObject temp = JsonObject.Parse(item.ToString());
                    if (temp.ContainsKey("title"))
                        topic.Title = temp["title"].GetString();
                    if (temp.ContainsKey("state"))
                        topic.Status = temp["state"].ToString();
                    if (temp.ContainsKey("link"))
                        topic.Link = temp["link"].GetString();
                    if (temp.ContainsKey("cover"))
                        topic.Cover = temp["cover"].GetString();
                    if (topic.Link.Length > 0)
                        list.Add(topic);
                }
            }
            return list;
        }
        /// <summary>
        /// 获取评论
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<List<Models.Reply>> GetReplysAsync(string url)
        {
            List<Models.Reply> reList = new List<Models.Reply>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {

                JsonObject json2 = JsonObject.Parse(json["data"].ToString());
                if (json2.ContainsKey("replies"))
                {
                    var a = json2["replies"].GetArray();
                    foreach (var item in a)
                    {
                        Models.Reply rp = new Models.Reply();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("content"))
                        {
                            JsonObject json3 = JsonObject.Parse(temp["content"].ToString());
                            if (json3.ContainsKey("message"))
                                rp.Message = json3["message"].GetString();
                        }
                        if (temp.ContainsKey("member"))
                        {
                            JsonObject json3 = JsonObject.Parse(temp["member"].ToString());
                            if (json3.ContainsKey("avatar"))
                                rp.Avatar = json3["avatar"].GetString();
                            if (json3.ContainsKey("uname"))
                                rp.Uname = json3["uname"].GetString();
                        }
                        if (temp.ContainsKey("ctime"))
                        {
                            rp.Time = StringDeal.LinuxToData(temp["ctime"].ToString());
                        }
                        if (temp.ContainsKey("like"))
                        {
                            rp.Like = temp["like"].ToString();
                        }
                        if (temp.ContainsKey("replies"))
                        {
                            rp.Res = new List<Reply>();
                            JsonArray aa = temp["replies"].GetArray();
                            foreach (var item1 in aa)
                            {
                                Models.Reply rp1 = new Models.Reply();
                                JsonObject temp1 = JsonObject.Parse(item1.ToString());
                                if (temp1.ContainsKey("content"))
                                {
                                    JsonObject json3 = JsonObject.Parse(temp1["content"].ToString());
                                    if (json3.ContainsKey("message"))
                                        rp1.Message = json3["message"].GetString();
                                }
                                if (temp1.ContainsKey("member"))
                                {
                                    JsonObject json3 = JsonObject.Parse(temp1["member"].ToString());
                                    if (json3.ContainsKey("avatar"))
                                        rp1.Avatar = json3["avatar"].GetString();
                                    if (json3.ContainsKey("uname"))
                                        rp1.Uname = json3["uname"].GetString();
                                }
                                rp.Res.Add(rp1);
                            }
                        }
                        reList.Add(rp);
                    }
                }
            }
            return reList;
        }
        /// <summary>
        /// 获取收藏
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task<List<Content>> GetFavAsync(string fid, int page, int pagesize)
        {
            List<Content> mylist = new List<Content>();
            string url = "http://api.bilibili.com/x/favourite/video?_device=android&_ulv=10000&platform=android&build=424000&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey + "&pn=" + page.ToString() + "&ps=" + pagesize.ToString() + "&fid=" + fid + "&order=ftime&rnd=" + new Random().Next(1000, 3000).ToString();
            url += ApiHelper.GetSign(url);
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {
                JsonObject json2 = JsonObject.Parse(json["data"].ToString());
                if (json2.ContainsKey("videos"))
                {
                    JsonArray array = json2["videos"].GetArray();
                    foreach (var item in array)
                    {
                        Content cont = new Content();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("aid"))
                            cont.Num = temp["aid"].ToString();
                        if (temp.ContainsKey("fav_create_at"))
                            cont.Creat = StringDeal.delQuotationmarks(temp["fav_create_at"].ToString()).Split(' ')[0];
                        if (temp.ContainsKey("pic"))
                            cont.Pic = StringDeal.delQuotationmarks(temp["pic"].ToString());
                        if (temp.ContainsKey("title"))
                            cont.Title = StringDeal.delQuotationmarks(temp["title"].ToString());
                        mylist.Add(cont);
                    }
                }
            }
            return mylist;
        }
        /// <summary>
        /// /获取订阅
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task<List<Concern>> GetConAsync(int page)
        {
            List<Concern> mylist = new List<Concern>();
            string url = "http://bangumi.bilibili.com/api/get_concerned_season?_device=wp&_ulv=10000&build=424000&platform=android&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey + "&page=" + page.ToString() + "&pagesize=30&ts=" + ApiHelper.GetLinuxTS().ToString();
            url += ApiHelper.GetSign(url);
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("result"))
            {
                JsonArray array = json["result"].GetArray();
                foreach (var item in array)
                {
                    Concern cont = new Concern();
                    JsonObject temp = JsonObject.Parse(item.ToString());
                    if (temp.ContainsKey("season_id"))
                        cont.ID = temp["season_id"].GetString();
                    if (temp.ContainsKey("newest_ep_index"))
                        cont.New = temp["newest_ep_index"].GetString(); ;
                    if (temp.ContainsKey("is_finish"))
                        cont.isFinish = temp["is_finish"].GetString();
                    if (temp.ContainsKey("cover"))
                        cont.Cover = temp["cover"].GetString();
                    if (temp.ContainsKey("bangumi_title"))
                        cont.Title = temp["bangumi_title"].GetString();
                    if (temp.ContainsKey("squareCover"))
                        cont.sqCover = temp["squareCover"].GetString();
                    mylist.Add(cont);
                }
            }
            return mylist;
        }
        /// <summary>
        /// 获取相关视频
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<List<Basic>> GetRelatesAsync(string url)
        {
            List<Basic> relates = new List<Basic>();
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {
                json = JsonObject.Parse(json["data"].ToString());
                if(json.ContainsKey("relates"))
                {
                    JsonArray array = json["relates"].GetArray();
                    foreach (var item in array)
                    {
                        Basic basic = new Basic();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("aid"))
                            basic.ID = temp["aid"].ToString();
                        if (temp.ContainsKey("title"))
                            basic.Title = StringDeal.delQuotationmarks(temp["title"].ToString());
                        if (temp.ContainsKey("pic"))
                            basic.Cover = StringDeal.delQuotationmarks(temp["pic"].ToString());
                        relates.Add(basic);
                    }
                }
            }
            return relates;
        }
        /// <summary>
        /// 获取通知条数(需登录)
        /// </summary>
        /// <returns></returns>
        public static async Task<Count> GetCountAsync()
        {
            try
            {
                Count count = new Count();
                string url = "http://message.bilibili.com/api/notify/query.notify.count.do?access_key=" + ApiHelper.accesskey + "&actionKey=appkey" + "&appkey=422fd9d7289a1dd9&build=427000&platform=android&ts=" + ApiHelper.GetLinuxTS().ToString();
                url += ApiHelper.GetSign(url);
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = JsonObject.Parse(json["data"].ToString());
                    if (json.ContainsKey("at_me"))
                        count.At_me = json["at_me"].ToString();
                    if (json.ContainsKey("chat_me"))
                        count.Chat_me = json["chat_me"].ToString();
                    if (json.ContainsKey("notify_me"))
                        count.Notify_me = json["notify_me"].ToString();
                    if (json.ContainsKey("praise_me"))
                        count.Praise_me = json["praise_me"].ToString();
                    if (json.ContainsKey("reply_me"))
                        count.Reply_me = json["reply_me"].ToString();
                }
                return count;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取回复(需登录)
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Chat>> GetChatsAsync()
        {
            try
            {
                List<Chat> chats = new List<Chat>();
                string url = "http://message.bilibili.com/api/notify/query.replyme.list.do?access_key=" + ApiHelper.accesskey + "&actionKey=appkey" + "&appkey=422fd9d7289a1dd9&build=427000&platform=android&data_type=1&ts=" + ApiHelper.GetLinuxTS().ToString();
                url += ApiHelper.GetSign(url);
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    JsonArray array = json["data"].GetArray();
                    foreach (var item in array)
                    {
                        Chat chat = new Chat();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("content"))
                        {
                            chat.Content = Regex.Match(temp["content"].GetString(), @"(?<={).*?(?=})").Value;
                            chat.Aid = Regex.Match(temp["content"].GetString(), @"(?<=av)\d+").Value;
                        }
                        if (temp.ContainsKey("title"))
                        {
                            chat.Title = Regex.Match(temp["title"].GetString(), @"(?<={).*?(?=})").Value + "\t" + Regex.Match(Regex.Match(temp["title"].GetString(), @"(?<=}).*?$").Value, @"(?<=}).*?$").Value;
                        }
                        if (temp.ContainsKey("time_at"))
                            chat.Time = StringDeal.delQuotationmarks(temp["time_at"].ToString());
                        if (temp.ContainsKey("publisher"))
                        {
                            json = JsonObject.Parse(temp["publisher"].ToString());
                            if (json.ContainsKey("face"))
                                chat.Face = StringDeal.delQuotationmarks(json["face"].ToString());
                            if (json.ContainsKey("mid"))
                                chat.Mid = json["mid"].ToString();
                            if (json.ContainsKey("name"))
                                chat.Name = StringDeal.delQuotationmarks(json["name"].ToString());
                        }
                        chats.Add(chat);
                    }
                }
                return chats;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取通知(需登录)
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Notify>> GetNotiAsync()
        {
            try
            {
                List<Notify> notis = new List<Notify>();
                string url = "http://message.bilibili.com/api/notify/query.sysnotify.list.do?access_key=" + ApiHelper.accesskey + "&actionKey=appkey" + "&appkey=422fd9d7289a1dd9&build=427000&platform=android&data_type=1&ts=" + ApiHelper.GetLinuxTS().ToString();
                url += ApiHelper.GetSign(url);
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    JsonArray array = json["data"].GetArray();
                    foreach (var item in array)
                    {
                        Notify noti = new Notify();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("content"))
                            noti.Content = StringDeal.delQuotationmarks(temp["content"].ToString());
                        if (temp.ContainsKey("title"))
                            noti.Title = StringDeal.delQuotationmarks(temp["title"].ToString());
                        if (temp.ContainsKey("time_at"))
                            noti.Time = StringDeal.delQuotationmarks(temp["time_at"].ToString());
                        notis.Add(noti);
                    }
                }
                return notis;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取私信(需登录)
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Wisper>> GetWisperAsync()
        {
            try
            {
                List<Wisper> wiss = new List<Wisper>();
                string url = "http://message.bilibili.com/api/msg/query.room.list.do?access_key=" + ApiHelper.accesskey + "&actionKey=appkey" + "&appkey=422fd9d7289a1dd9&build=427000&platform=android&page_no=1&ts=" + ApiHelper.GetLinuxTS().ToString();
                url += ApiHelper.GetSign(url);
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    JsonArray array = json["data"].GetArray();
                    foreach (var item in array)
                    {
                        Wisper wis = new Wisper();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("room_name"))
                            wis.Room = StringDeal.delQuotationmarks(temp["room_name"].ToString());
                        if (temp.ContainsKey("last_msg"))
                            wis.LastMsg = temp["last_msg"].GetString();
                        if (temp.ContainsKey("last_time"))
                        {
                            wis.Time = temp["last_time"].ToString();
                        }
                        if (temp.ContainsKey("avatar_url"))
                            wis.Face = temp["avatar_url"].GetString();
                        if (temp.ContainsKey("rid"))
                            wis.rid = StringDeal.delQuotationmarks(temp["rid"].ToString());
                        wiss.Add(wis);
                    }
                }
                return wiss;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取推荐番剧
        /// </summary>
        /// <returns></returns>
        public static async Task<List<HotBangumi>> GetHotBangumiAsync(string url)
        {
            try
            {
                List<HotBangumi> hots = new List<HotBangumi>();
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("result"))
                {
                    JsonArray array = json["result"].GetArray();
                    foreach (var item in array)
                    {
                        HotBangumi hot = new HotBangumi();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("cursor"))
                            hot.Cursor = temp["cursor"].ToString();
                        if (temp.ContainsKey("cover"))
                            hot.Cover = temp["cover"].GetString(); ;
                        if (temp.ContainsKey("desc"))
                            hot.Desc = temp["desc"].GetString();
                        if (temp.ContainsKey("title"))
                            hot.Title = temp["title"].GetString();
                        if (temp.ContainsKey("link"))
                        {
                            hot.Link = temp["link"].GetString();
                        }
                        hots.Add(hot);
                    }
                }
                return hots;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取播放历史（需登录）
        /// </summary>
        public static async Task<List<History>> GetHistoryAsync(string url)
        {
            try
            {
                List<History> hs = new List<History>();
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    JsonArray array = json["data"].GetArray();
                    foreach (var item in array)
                    {
                        History h = new History();
                        JsonObject temp = JsonObject.Parse(item.ToString());
                        if (temp.ContainsKey("aid"))
                            h.Aid = temp["aid"].ToString();
                        if (temp.ContainsKey("pic"))
                            h.Pic = StringDeal.delQuotationmarks(temp["pic"].ToString());
                        if (temp.ContainsKey("title"))
                            h.Title = StringDeal.delQuotationmarks(temp["title"].ToString());
                        if (temp.ContainsKey("view_at")) 
                        {
                            h.Time = StringDeal.LinuxToData(StringDeal.delQuotationmarks(temp["view_at"].ToString()));
                        }
                        hs.Add(h);
                    }
                }
                return hs;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取收藏文件夹（需登录）
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Folder>> GetFavFolders()
        {
            List<Folder> myFolder = new List<Folder>();
            string url_folder = "http://api.bilibili.com/x/app/favourite/folder?_device=android&_ulv=10000&access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&build=427000&platform=android&vmid=" + UserHelper.mid + "&rnd=" + new Random().Next(1000, 2000).ToString();
            url_folder += ApiHelper.GetSign(url_folder);
            JsonObject json_f = await BaseService.GetJson(url_folder);
            if (json_f.ContainsKey("data"))
            {
                JsonArray favs = json_f["data"].GetArray();
                foreach (var item in favs)
                {
                    JsonObject json2 = JsonObject.Parse(item.ToString());
                    Folder fav = new Folder();
                    if (json2.ContainsKey("ctime"))
                        fav.Ctime = StringDeal.LinuxToData(json2["ctime"].ToString());
                    if (json2.ContainsKey("cur_count"))
                        fav.Count = json2["cur_count"].ToString();
                    if (json2.ContainsKey("fid"))
                        fav.Fid = json2["fid"].ToString();
                    if (json2.ContainsKey("max_count"))
                        fav.MCount = json2["max_count"].ToString();
                    if (json2.ContainsKey("name"))
                        fav.Name = StringDeal.delQuotationmarks(json2["name"].ToString());
                    myFolder.Add(fav);
                }
                return myFolder;
            }
            return null;
        }
        public static async Task<User> GetUserinfoAsync(string mid)
        {
            string url = "http://space.bilibili.com/ajax/member/GetInfo?rnd=" + new Random().Next(0, 1000).ToString();
            string result = await BaseService.SendPostAsync(url, "mid=" + mid, "http://space.bilibili.com/" + mid);
            JsonObject json = JsonObject.Parse(result);
            if (json.ContainsKey("data"))
            {
                User user = new User();
                user.Attentions = new List<string>();
                json = JsonObject.Parse(json["data"].ToString());
                if (json.ContainsKey("article"))
                    user.Article = json["article"].ToString();
                if (json.ContainsKey("attention"))
                    user.Attention = json["attention"].ToString();
                if (json.ContainsKey("attentions"))
                {
                    JsonArray ar = json["attentions"].GetArray();
                    foreach (var item in ar)
                    {
                        user.Attentions.Add(item.GetNumber().ToString());
                    }
                }
                if (json.ContainsKey("birthday"))
                    user.BirthDay = json["birthday"].GetString();
                if (json.ContainsKey("place"))
                    user.Place = json["place"].GetString();
                if (json.ContainsKey("coins"))
                    user.Coins = json["coins"].ToString();
                if (json.ContainsKey("sign"))
                    user.Sign = StringDeal.delQuotationmarks(json["sign"].ToString());
                if (json.ContainsKey("coins"))
                    user.Coins = json["coins"].ToString();
                if (json.ContainsKey("discription"))
                    user.Discription = StringDeal.delQuotationmarks(json["discription"].ToString());
                if (json.ContainsKey("fans"))
                    user.Fans = json["fans"].ToString();
                if (json.ContainsKey("face"))
                    user.Face = StringDeal.delQuotationmarks(json["face"].ToString());
                if (json.ContainsKey("friend"))
                    user.Friend = json["friend"].ToString();
                if (json.ContainsKey("im9_sign"))
                    user.Im9_sign = StringDeal.delQuotationmarks(json["im9_sign"].ToString());
                if (json.ContainsKey("mid"))
                    user.Mid = json["mid"].ToString();
                if (json.ContainsKey("name"))
                    user.Name = StringDeal.delQuotationmarks(json["name"].ToString());
                if (json.ContainsKey("regtime"))
                {
                    string a = json["regtime"].ToString();
                    user.RegTime = StringDeal.LinuxToData(json["regtime"].ToString());
                }
                if (json.ContainsKey("toutu"))
                    user.Toutu = StringDeal.delQuotationmarks(json["toutu"].ToString()); ;
                if (json.ContainsKey("toutuId"))
                    user.ToutuId = json["toutuId"].ToString();
                if (json.ContainsKey("sex"))
                    user.Sex = StringDeal.delQuotationmarks(json["sex"].ToString());
                if (json.ContainsKey("level_info"))
                {
                    JsonObject json2 = JsonObject.Parse(json["level_info"].ToString());
                    if (json2.ContainsKey("current_exp"))
                        user.Current_exp = json2["current_exp"].ToString();
                    if (json2.ContainsKey("next_exp"))
                        user.Next_exp = json2["next_exp"].ToString();
                    if (json2.ContainsKey("current_min"))
                        user.Current_min = json2["current_min"].ToString();
                    if (json2.ContainsKey("current_level"))
                        user.Current_level = json2["current_level"].ToString();
                }
                return user;
            }
            return null;
        }

        /// <summary>
        /// 获取硬币记录
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CoinHs>> GetCoinHistoryAsync()
        {
            try
            {
                List<CoinHs> list = new List<CoinHs>();
                string url = "http://account.bilibili.com/site/GetMoneyLog";
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = JsonObject.Parse(json["data"].ToString());
                    if (json.ContainsKey("result"))
                    {
                        JsonArray array = json["result"].GetArray();
                        foreach (var item in array)
                        {
                            CoinHs hs = new CoinHs();
                            json = JsonObject.Parse(item.ToString());
                            if (json.ContainsKey("delta"))
                            {
                                hs.Delta = json["delta"].ToString();
                            }
                            if (json.ContainsKey("reason"))
                            {
                                hs.Reason = json["reason"].GetString();
                            }
                            if (json.ContainsKey("time"))
                            {
                                hs.Time = json["time"].GetString();
                            }
                            list.Add(hs);
                        }
                    }
                    return list;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取关注列表
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public static async Task<List<Friend>> GetFriendsAsync(string mid)
        {
            string url = "http://space.bilibili.com/ajax/friend/getAttentionList?mid=" + mid + "&page=1&rnd=" + new Random().Next(1000, 3000).ToString();
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {
                List<Friend> friends = new List<Friend>();
                //这个地方应该能写得更漂亮点
                if (StringDeal.delQuotationmarks(json["data"].ToString()) == "关注列表中没有值")
                {
                    return friends;
                }
                else
                {
                    json = JsonObject.Parse(json["data"].ToString());
                    if (json.ContainsKey("list"))
                    {
                        JsonArray ja = json["list"].GetArray();
                        foreach (var item in ja)
                        {
                            Friend friend = new Friend();
                            JsonObject temp = JsonObject.Parse(item.ToString());
                            if (temp.ContainsKey("face"))
                                friend.Face = StringDeal.delQuotationmarks(temp["face"].ToString());
                            if (temp.ContainsKey("uname"))
                                friend.Uname = StringDeal.delQuotationmarks(temp["uname"].ToString());
                            if (temp.ContainsKey("fid"))
                                friend.Fid = temp["fid"].ToString();
                            friends.Add(friend);
                        }
                        return friends;
                    }
                }             
            }
            return null;
        }

        public static async Task<List<FlipItem>> GetFilpItems()
        {
            List<FlipItem> items = new List<FlipItem>();
            string url = "http://bangumi.bilibili.com/api/app_index_page_v2";
            JsonObject json = await BaseService.GetJson(url);
            if (json["code"].ToString() == "0")
            {
                if (json.ContainsKey("result"))
                {
                    json = JsonObject.Parse(json["result"].ToString());
                    if (json.ContainsKey("banners"))
                    {
                        JsonArray array = json["banners"].GetArray();
                        foreach (var temp in array)
                        {
                            FlipItem item = new FlipItem();
                            json = JsonObject.Parse(temp.ToString());
                            if (json.ContainsKey("img"))
                            {
                                item.Img = json["img"].GetString();
                            }
                            if (json.ContainsKey("link"))
                            {
                                item.Link = json["link"].GetString();
                            }
                            if (json.ContainsKey("title"))
                            {
                                item.Title = json["title"].GetString();
                            }
                            items.Add(item);
                        }
                    }
                }
                return items;
            }
            else
            {
                return null;
            }
        }
    }
}