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
using bilibili.Http;

namespace bilibili.Http
{
    class ContentServ
    {
        public delegate void ReportStatus(string status);
        public static event ReportStatus report;
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
            if (!WebStatusHelper.IsOnline())
            {
                report("无网络连接");
                return null;
            }
            string ord = string.Empty;
            switch (order)
            {
                case 1: ord = "default"; break;
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
                        var json2 = list.GetObject();
                        for (int i = 0; i < 20; i++)
                        {
                            if (json2.ContainsKey(string.Format("{0}", i)))
                            {
                                var item = json2[string.Format("{0}", i)];
                                var json3 = item.GetObject();
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
            catch(Exception e)
            {
                report(e.Message);
                return null;
            }
        }

        //public async static Task<List<Concern>> GetFriendsCons(string mid, int page)
        //{
        //    List<Concern> mylist = new List<Concern>();
        //    string url = "http://space.bilibili.com/ajax/Bangumi/getList?mid=" + mid + "&pagesize=20&page=" + page.ToString();
        //    url += ApiHelper.GetSign(url);
        //    JsonObject json = await BaseService.GetJson(url);
        //    if (json.ContainsKey("status"))
        //    {
        //        if (json["status"].GetBoolean() == false)
        //        {
        //            //用户隐私设置为不公开
        //            return mylist;
        //        }
        //    }
        //    if (json.ContainsKey("data"))
        //    {
        //        json = json["data"].GetObject();
        //        if (json.ContainsKey("result"))
        //        {
        //            JsonArray array = json["result"].GetArray();
        //            foreach (var item in array)
        //            {
        //                Concern cont = new Concern();
        //                JsonObject temp = item.GetObject();
        //                if (temp.ContainsKey("season_id"))
        //                    cont.ID = StringDeal.delQuotationmarks(temp["season_id"].ToString());
        //                if (temp.ContainsKey("newest_ep_index"))
        //                    cont.New = temp["newest_ep_index"].ToString();
        //                if (temp.ContainsKey("is_finish"))
        //                    cont.isFinish = temp["is_finish"].ToString();
        //                if (temp.ContainsKey("cover"))
        //                    cont.Cover = temp["cover"].GetString();
        //                if (temp.ContainsKey("title"))
        //                    cont.Title = temp["title"].GetString();
        //                mylist.Add(cont);
        //            }
        //        }
        //    }
        //    return mylist;
        //}

        /// <summary>
        /// 获取热搜
        /// </summary>
        /// <returns></returns>
        public async static Task<List<KeyWord>> GetHotSearchAsync()
        {
            if (!WebStatusHelper.IsOnline()) return null;
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
                    JsonObject temp = item.GetObject();
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
        //public static async Task<List<Content>> GetMyVideoAsync(string mid, int page, int pagesize = 20)
        //{
        //    string url = "http://api.bilibili.com/list?type=json&appkey=" + ApiHelper.appkey + "&mid=" + mid + "&page=" + page.ToString() + "&pagesize=" + pagesize.ToString() + "&platform=wp&rnd=" + new Random().Next(3000, 6000).ToString() + "&access_key=" + ApiHelper.accesskey;
        //    url += ApiHelper.GetSign(url);
        //    JsonObject json = new JsonObject();
        //    List<Content> contentList = new List<Content>();
        //    try
        //    {
        //        json = await BaseService.GetJson(url);
        //        if (json.ContainsKey("list"))
        //        {
        //            var list = json["list"];
        //            if (list != null)
        //            {
        //                var json2 = list.GetObject();
        //                for (int i = 0; i < 20; i++)
        //                {
        //                    if (json2.ContainsKey(string.Format("{0}", i)))
        //                    {
        //                        var item = json2[string.Format("{0}", i)];
        //                        var json3 = item.GetObject();
        //                        Content myContent = new Content();
        //                        if (json3.ContainsKey("title"))
        //                        {
        //                            myContent.Title = json3["title"].GetString();
        //                            if (json3.ContainsKey("pic"))
        //                                myContent.Pic = json3["pic"].GetString();
        //                            if (json3.ContainsKey("aid"))
        //                                myContent.Num = json3["aid"].GetString();
        //                            if (json3.ContainsKey("play"))
        //                                myContent.Play = json3["play"].ToString();
        //                            if (json3.ContainsKey("comment"))
        //                                myContent.Comment = json3["comment"].ToString();
        //                            contentList.Add(myContent);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        return contentList;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        /// <summary>
        /// 获取视频详情
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<Details> GetDetailsAsync(string url)
        {
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            Details details = new Details();
            details.Tags = new List<string>();
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("code"))
                if (json["code"].ToString() == "-404")
                {
                    report("该视频不存在或已被删除");
                    return null;
                }
            if (json.ContainsKey("data"))
            {
                List<Pages> pages = new List<Pages>();
                json = json["data"].GetObject();
                details.Desc = json.ContainsKey("desc") ? json["desc"].GetString() : "";
                if (json.ContainsKey("aid"))
                    details.Aid = json["aid"].ToString();
                if (json.ContainsKey("ctime"))
                    details.Time = StringDeal.LinuxToData(json["ctime"].ToString());
                if (json.ContainsKey("title"))
                    details.Title = StringDeal.delQuotationmarks(json["title"].ToString());
                if (json.ContainsKey("pic"))
                    details.Pic = StringDeal.delQuotationmarks(json["pic"].ToString());
                if (json.ContainsKey("duration"))
                    details.Duration = Convert.ToInt32(json["duration"].ToString());
                if (json.ContainsKey("req_user"))
                {
                    JsonObject j = json["req_user"].GetObject();
                    if (j.ContainsKey("favorite"))
                    {
                        details.IsFav = j["favorite"].ToString();
                    }
                }
                if (json.ContainsKey("pages"))
                {
                    JsonArray array = json["pages"].GetArray();
                    foreach (var item in array)
                    {
                        Pages page = new Pages();
                        JsonObject temp = item.GetObject();
                        if (temp.ContainsKey("cid"))
                            page.Cid = temp["cid"].ToString();
                        if (temp.ContainsKey("part"))
                            page.Part = StringDeal.delQuotationmarks(temp["part"].ToString()).Length == 0 ? details.Title : StringDeal.delQuotationmarks(temp["part"].ToString());
                        pages.Add(page);
                    }
                    details.Ps = pages;

                }
                if (json.ContainsKey("owner"))
                {
                    JsonObject json_owner = json["owner"].GetObject();
                    if (json_owner.ContainsKey("name"))
                        details.Upzhu = StringDeal.delQuotationmarks(json_owner["name"].ToString());
                    if (json_owner.ContainsKey("mid"))
                        details.Mid = json_owner["mid"].ToString();
                }
                if (json.ContainsKey("stat"))
                {
                    JsonObject json_stat = json["stat"].GetObject();
                    if (json_stat.ContainsKey("view"))
                        details.View = json_stat["view"].ToString();
                    if (json_stat.ContainsKey("danmaku"))
                        details.Danmu = json_stat["danmaku"].ToString();
                    if (json_stat.ContainsKey("share"))
                        details.Share = json_stat["share"].ToString();
                    if (json_stat.ContainsKey("favorite"))
                        details.Fav = json_stat["favorite"].ToString();
                    if (json_stat.ContainsKey("coin"))
                        details.Coins = json_stat["coin"].ToString();
                    if (json_stat.ContainsKey("reply"))
                        details.Reply = json_stat["reply"].ToString();
                }
                if (json.ContainsKey("season"))
                {
                    JsonObject json2 = json["season"].GetObject();
                    if (json2.ContainsKey("title"))
                        details.BangumiTitle = json2["title"].GetString();
                    if (json2.ContainsKey("season_id"))
                        details.Sid = json2["season_id"].GetString();
                }
                if (json.ContainsKey("tags"))
                {
                    //可能出现tags=(null)
                    try
                    {
                        var a = json["tags"].GetArray();
                        foreach (var item in json["tags"].GetArray())
                        {
                            details.Tags.Add(item.GetString());
                        }
                    }
                    catch (Exception e)
                    {
                        report(e.Message);
                    }
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
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
                            JsonObject json2 = item.GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            List<Live> list = new List<Live>();
            string url = "http://live.bilibili.com/AppIndex/home?_device=wp&_ulv=10000&access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&build=411005&platform=android&scale=xxhdpi&rnd=" + new Random().Next(1000, 3000).ToString();
            url += ApiHelper.GetSign(url);
            try
            {
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = json["data"].GetObject();
                    if (json.ContainsKey("recommend_data"))
                    {
                        json = json["recommend_data"].GetObject();
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
            catch(Exception e)
            {
                report(e.Message);
                return null;
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
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
                        JsonObject json2 = item.GetObject();
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
            catch (Exception e)
            {
                report(e.Message);
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            string url = "http://app.bilibili.com/x/v2/search/type?keyword=" + keyword + "&pn=" + page.ToString() + "&ps=20&type=2&appkey=" + ApiHelper.appkey + "&build=429001&mobi_app=win&platform=android";
            url += ApiHelper.GetSign(url);
            List<UpForSearch> upList = new List<UpForSearch>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {
                json = json["data"].GetObject();
                if (json.ContainsKey("items"))
                {
                    var Myarray = json["items"].GetArray();
                    foreach (var item in Myarray)
                    {
                        JsonObject json2 = item.GetObject();
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
        /// <param name="sid"></param>
        /// <returns></returns>
        public static async Task<Season> GetSeasonResultAsync(string sid)
        {
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            string url = "http://bangumi.bilibili.com/api/season_v2?_device=wp&build=424000&platform=android&access_key=" + ApiHelper.accesskey + "&appkey=422fd9d7289a1dd9&ts=" + ApiHelper.GetLinuxTS().ToString() + "&type=bangumi&season_id=" + sid;
            url += ApiHelper.GetSign(url);
            JsonObject json = new JsonObject();
            Season season = new Season();
            try
            {
                json = await BaseService.GetJson(url);
                if (json.ContainsKey("result"))
                {
                    json = json["result"].GetObject();
                    if (json.ContainsKey("coins"))
                        season.Coins = StringDeal.delQuotationmarks(json["coins"].ToString());
                    if (json.ContainsKey("danmaku_count"))
                        season.Danmaku = StringDeal.delQuotationmarks(json["danmaku_count"].ToString());
                    if (json.ContainsKey("copyright"))
                        season.Copyright = json["copyright"].GetString();
                    if (json.ContainsKey("cover"))
                        season.Cover = json["cover"].GetString();
                    if (json.ContainsKey("bangumi_title"))
                        season.Title = json["bangumi_title"].GetString();
                    if (json.ContainsKey("evaluate"))
                        season.Brief = json["evaluate"].GetString();
                    if (json.ContainsKey("staff"))
                        season.Staff = json["staff"].GetString();
                    if (json.ContainsKey("favorites"))
                        season.Fav = StringDeal.delQuotationmarks(json["favorites"].ToString());
                    if (json.ContainsKey("play_count"))
                        season.View = StringDeal.delQuotationmarks(json["play_count"].ToString());
                    if (json.ContainsKey("weekday"))
                        season.WeekDay = json["weekday"].GetString();
                    if (json.ContainsKey("pub_time"))
                        season.Time = json["pub_time"].GetString().Split(' ')[0];
                    if (json.ContainsKey("is_finish"))
                        season.isFinish = json["is_finish"].ToString() == "0" ? true : false;
                    if (json.ContainsKey("squareCover"))
                        season.SquareCover = json["squareCover"].GetString();
                    if (json.ContainsKey("user_season"))
                    {
                        JsonObject j = json["user_season"].GetObject();
                        if (j.ContainsKey("attention"))
                        {
                            season.IsConcerned = j["attention"].GetString();
                        }
                    }
                    if (json.ContainsKey("tags"))
                    {
                        season.Tags = new List<string>();
                        foreach (var item in json["tags"].GetArray())
                        {
                            JsonObject temp = item.GetObject();
                            if (temp.ContainsKey("tag_name"))
                                season.Tags.Add(StringDeal.delQuotationmarks(temp["tag_name"].ToString()));
                        }
                    }
                    if (json.ContainsKey("actor"))
                    {
                        List<Cast> cvlist = new List<Cast>();
                        JsonArray Array_ac = json["actor"].GetArray();
                        foreach (var item in Array_ac)
                        {
                            Cast cast = new Cast();
                            JsonObject json3 = item.GetObject();
                            if (json3.ContainsKey("actor"))
                                cast.Actor = json3["actor"].GetString();
                            if (json3.ContainsKey("role"))
                                cast.Role = json3["role"].GetString();
                            cvlist.Add(cast);
                        }
                        season.CVlist = cvlist;
                    }
                    if (json.ContainsKey("episodes"))
                    {
                        List<Episodes> indexList = new List<Episodes>();
                        JsonArray Array_rs = json["episodes"].GetArray();
                        foreach (var item in Array_rs)
                        {
                            Episodes episode = new Episodes();
                            JsonObject json3 = item.GetObject();
                            if (json3.ContainsKey("av_id"))
                                episode.ID = StringDeal.delQuotationmarks(json3["av_id"].ToString());
                            if (json3.ContainsKey("coins"))
                                episode.Coins = StringDeal.delQuotationmarks(json3["coins"].ToString());
                            if (json3.ContainsKey("cover"))
                                episode.Cover = json3["cover"].GetString();
                            if (json3.ContainsKey("danmaku"))
                                episode.Danmaku = StringDeal.delQuotationmarks(json3["danmaku"].ToString());
                            if (json3.ContainsKey("index"))
                                episode.Index = StringDeal.delQuotationmarks(json3["index"].ToString());
                            if (json3.ContainsKey("index_title"))
                                episode.Title = json3["index_title"].GetString();
                            if (json3.ContainsKey("update_time"))
                                episode.Time = json3["update_time"].GetString().Split(' ')[0];
                            indexList.Add(episode);
                        }
                        season.EPS = indexList;
                    }
                    if (json.ContainsKey("seasons"))
                    {
                        List<RelateSeason> seasons = new List<RelateSeason>();
                        JsonArray array = json["seasons"].GetArray();
                        foreach (var item in array)
                        {
                            RelateSeason rs = new RelateSeason();
                            json = item.GetObject();
                            if (json.ContainsKey("title"))
                            {
                                rs.Title = json["title"].GetString();
                            }
                            if (json.ContainsKey("season_id"))
                            {
                                rs.Sid = json["season_id"].GetString();
                            }
                            seasons.Add(rs);
                        }
                        season.Related = seasons;
                    }
                }
                return season;
            }
            catch (Exception e)
            {
                report(e.Message);
                return null;
            }
        }
    
      /// <summary>
      /// 获取视频源地址及相关信息
      /// </summary>
      /// <param name="cid"></param>
      /// <param name="quality"></param>
      /// <param name="format"></param>
      /// <returns></returns>
        public static async Task<VideoURL> GetVedioURL(string cid, string quality, VideoFormat format)
        {
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            #region XML
            VideoURL URL = new VideoURL { Acceptformat = new List<string>(), Acceptquality = new List<string>() };
            JsonObject json = new JsonObject();
            string url = string.Format("http://interface.bilibili.com/playurl?_device=uwp&cid={0}&quality={1}&otype=xml&appkey={2}&_buvid=A7A15F70-8D92-4441-B941-0E4EF9F21B6319763infoc&_hwid=03008e90050092d8&platform=uwp_desktop&type={3}&access_key={4}&mid={5}&ts={6}", cid, quality, ApiHelper.appkey, format.ToString(), ApiHelper.accesskey, UserHelper.mid, ApiHelper.GetLinuxTS().ToString());
            url += ApiHelper.GetSign(url);
            XmlDocument doc = await XmlDocument.LoadFromUriAsync(new Uri(url));
            URL.Url = doc.GetElementsByTagName("url")[0].InnerText;
            URL.Size = doc.GetElementsByTagName("size")[0].InnerText;
            URL.Length = doc.GetElementsByTagName("length")[0].InnerText;
            URL.Acceptquality = doc.GetElementsByTagName("accept_quality")[0].InnerText.Split(',').ToList();
            URL.Acceptformat = doc.GetElementsByTagName("accept_format")[0].InnerText.Split(',').ToList();
            #endregion
            #region JSON
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
            //        json = json["durl"].GetArray()[0].ToString());
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
            //http://interface.bilibili.com/playurl?_device=uwp&cid=4651392&quality=2&otype=xml&appkey=422fd9d7289a1dd9&_buvid=A7A15F70-8D92-4441-B941-0E4EF9F21B6319763infoc&_hwid=03008e90050092d8&platform=uwp_desktop&type=mp4&access_key=d0c76c033585f2b1332845538fb20712&mid=33034956&ts=1484143990&sign=3d2b7a1d6d03cb3642338639d6956544
            //http://interface.bilibili.com/playurl?_device=uwp&cid={0}&quality={1}&otype=xml&appkey={2}&_buvid=D57C3D63-7920-41FA-910D-AB6CBD5365F830799infoc&_hwid=0100d4c50200c2a6&platform=uwp_mobile&type=mp4&access_key={3}&mid={4}&ts={5}", cid, quality.ToString(), ApiHelper.appkey, ApiHelper.accesskey, UserHelper.mid, ApiHelper.GetLinuxTS().ToString());
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            List<Tags> tagList = new List<Tags>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("result"))
            {
                var a = json["result"].GetArray();
                foreach (var item in a)
                {
                    Tags tag = new Tags();
                    JsonObject temp = item.GetObject();
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
        public static async Task<List<Bangumi>> GetBansByTagAsync(string url)
        {
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            List<Bangumi> banList = new List<Bangumi>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("result"))
            {
                JsonObject json2 = json["result"].GetObject();
                if (json2.ContainsKey("list"))
                {
                    var a = json2["list"].GetArray();
                    foreach (var item in a)
                    {
                        Bangumi ban = new Bangumi();
                        JsonObject temp = item.GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                return null;
            }
            List<LastUpdate> list = new List<LastUpdate>();
            string url = "http://bangumi.bilibili.com/api/app_index_page";
            try
            {
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("result"))
                {
                    json = json["result"].GetObject();
                    if (json.ContainsKey("latestUpdate"))
                    {
                        json = json["latestUpdate"].GetObject();
                        if (json.ContainsKey("list"))
                        {
                            JsonArray array = json["list"].GetArray();
                            foreach (var item in array)
                            {
                                LastUpdate last = new LastUpdate();
                                json = item.GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
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
                    JsonObject temp = item.GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
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
                    JsonObject temp = item.GetObject();
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
        public static async Task<List<Reply>> GetReplysAsync(string url)
        {
            if (!WebStatusHelper.IsOnline())
            {
                return null;
            }
            List<Reply> reList = new List<Reply>();
            JsonObject json = new JsonObject();
            json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {

                JsonObject json2 = json["data"].GetObject();
                //一层回复
                if (json2.ContainsKey("replies"))
                {
                    var a = json2["replies"].GetArray();
                    foreach (var item in a)
                    {
                        Reply rp = new Reply();
                        JsonObject temp = item.GetObject();
                        if (temp.ContainsKey("content"))
                        {
                            JsonObject json3 = temp["content"].GetObject();
                            if (json3.ContainsKey("message"))
                                rp.Message = json3["message"].GetString();
                        }
                        if (temp.ContainsKey("member"))
                        {
                            JsonObject json3 = temp["member"].GetObject();
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
                        if (temp.ContainsKey("mid"))
                        {
                            rp.Mid = temp["mid"].ToString();
                        }
                        if (temp.ContainsKey("oid"))
                        {
                            rp.Oid = temp["oid"].ToString();
                        }
                        if (temp.ContainsKey("parent"))
                        {
                            rp.Parent = temp["parent"].ToString();
                        }
                        if (temp.ContainsKey("oid"))
                        {
                            rp.Root = temp["root"].ToString();
                        }
                        if (temp.ContainsKey("rpid"))
                        {
                            rp.Rpid = temp["rpid"].ToString();
                        }
                        //回复的回复
                        if (temp.ContainsKey("replies"))
                        {
                            List<Reply> list = new List<Reply>();
                            GetReKids getrekid = new GetReKids();
                            getrekid.GetReplyKids(temp, true);
                            list = getrekid.list;
                            rp.Res = list;
                        }
                        reList.Add(rp);
                    }
                }
            }
            return reList;
        }
        public class GetReKids
        {
            public List<Reply> list;
            static JsonArray aa;
            static bool isFirst;
            public GetReKids()
            {
                isFirst = true;
                aa = new JsonArray();
                list = new List<Reply>();
            }
            /// <summary>
            /// 获取评论的回复
            /// </summary>
            /// <param name="json"></param>
            /// <returns></returns>
            public void GetReplyKids(JsonObject json, bool issecond)
            {
                if (isFirst == true) 
                {
                    isFirst = false;
                    aa = json["replies"].GetArray();
                    list.Clear();
                }
                foreach (var item1 in aa)
                {
                    Reply rp1 = new Reply();
                    issecond = true;
                    JsonObject temp1 = item1.GetObject();
                    if (temp1.ContainsKey("content"))
                    {
                        JsonObject json3 = temp1["content"].GetObject();
                        if (json3.ContainsKey("message"))
                            rp1.Message = json3["message"].GetString();
                    }
                    if (temp1.ContainsKey("member"))
                    {
                        JsonObject json3 = temp1["member"].GetObject();
                        if (json3.ContainsKey("avatar"))
                            rp1.Avatar = json3["avatar"].GetString();
                        if (json3.ContainsKey("uname"))
                            rp1.Uname = json3["uname"].GetString();
                    }
                    if (temp1.ContainsKey("like"))
                    {
                        rp1.Like = temp1["like"].ToString();
                    }
                    if (temp1.ContainsKey("mid"))
                    {
                        rp1.Mid = temp1["mid"].ToString();
                    }
                    if (temp1.ContainsKey("oid"))
                    {
                        rp1.Oid = temp1["oid"].ToString();
                    }
                    if (temp1.ContainsKey("parent"))
                    {
                        rp1.Parent = temp1["parent"].ToString();
                    }
                    if (temp1.ContainsKey("root"))
                    {
                        rp1.Root = temp1["root"].ToString();
                    }
                    if (temp1.ContainsKey("rpid"))
                    {
                        rp1.Rpid = temp1["rpid"].ToString();
                    }
                    if (issecond == true)
                    {
                        issecond = false;
                        rp1.Baseon = string.Empty;
                    }
                    else
                    {
                        rp1.Baseon = rp1.Uname;
                    }
                    list.Add(rp1);
                    try
                    {
                        if (temp1["replies"].GetArray().Count > 0)
                        {
                            GetReplyKids(temp1, false);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        /// <summary>
        /// 获取收藏
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task<List<Content>> GetFavAsync(string fid, int page, int pagesize)
        {
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            if (!ApiHelper.IsLogin())
            {
                report("请先登录");
                return null;
            }
            List<Content> mylist = new List<Content>();
            string url = "http://api.bilibili.com/x/favourite/video?_device=android&_ulv=10000&platform=android&build=424000&appkey=" + ApiHelper.appkey + "&access_key=" + ApiHelper.accesskey + "&pn=" + page.ToString() + "&ps=" + pagesize.ToString() + "&fid=" + fid + "&order=ftime&rnd=" + new Random().Next(1000, 3000).ToString();
            url += ApiHelper.GetSign(url);
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {
                json = json["data"].GetObject();
                if (json.ContainsKey("videos"))
                {
                    try
                    {
                        JsonArray array = json["videos"].GetArray();
                        foreach (var item in array)
                        {
                            Content cont = new Content();
                            JsonObject temp = item.GetObject();
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
                    catch { }
                }
            }
            return mylist;
        }
        /// <summary>
        /// /获取订阅
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        //public static async Task<List<Concern>> GetConAsync(int page,string mid)
        //{
        //    List<Concern> mylist = new List<Concern>();
        //    string url = "http://space.bilibili.com/ajax/Bangumi/getList?mid=" + mid + "&pagesize=20&page=" + page;
        //    url += ApiHelper.GetSign(url);
        //    JsonObject json = await BaseService.GetJson(url);
        //    if (json.ContainsKey("data"))
        //    {
        //        json = json["data"].GetObject();
        //    }
        //    if (json.ContainsKey("result"))
        //    {
        //        JsonArray array = json["result"].GetArray();
        //        foreach (var item in array)
        //        {
        //            Concern cont = new Concern();
        //            JsonObject temp = item.GetObject();
        //            if (temp.ContainsKey("season_id"))
        //                cont.ID = temp["season_id"].GetString();
        //            if (temp.ContainsKey("newest_ep_index"))
        //                cont.New = temp["newest_ep_index"].ToString();
        //            if (temp.ContainsKey("is_finish"))
        //                cont.isFinish = temp["is_finish"].ToString();
        //            if (temp.ContainsKey("cover"))
        //                cont.Cover = temp["cover"].GetString();
        //            if (temp.ContainsKey("title"))
        //                cont.Title = temp["title"].GetString();
        //            if (temp.ContainsKey("squareCover"))
        //                cont.sqCover = temp["squareCover"].GetString();
        //            mylist.Add(cont);
        //        }
        //    }
        //    return mylist;
        //}
        /// <summary>
        /// 获取相关视频
        /// </summary>
        /// <param name = "url" ></ param >
        /// < returns ></ returns >
        public static async Task<List<RelateVideo>> GetRelatesAsync(string aid)
        {
            string url = "http://app.bilibili.com/x/view?_device=android&_ulv=10000&plat=0&build=424000&aid=" + aid;
            List<RelateVideo> relates = new List<RelateVideo>();
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("data"))
            {
                json = json["data"].GetObject();
                if (json.ContainsKey("relates"))
                {
                    JsonArray array = json["relates"].GetArray();
                    foreach (var item in array)
                    {
                        RelateVideo basic = new RelateVideo();
                        JsonObject temp = item.GetObject();
                        if (temp.ContainsKey("aid"))
                            basic.ID = temp["aid"].ToString();
                        if (temp.ContainsKey("title"))
                            basic.Title = temp["title"].GetString();
                        if (temp.ContainsKey("pic"))
                            basic.Cover = temp["pic"].GetString();
                        if (temp.ContainsKey("owner"))
                        {
                            json = temp["owner"].GetObject();
                            if (json.ContainsKey("name"))
                                basic.Owner = json["name"].GetString();
                        }
                        if (temp.ContainsKey("stat"))
                        {
                            json = temp["stat"].GetObject();
                            if (json.ContainsKey("danmaku"))
                                basic.Danmaku = json["danmaku"].ToString();
                            if (json.ContainsKey("view"))
                                basic.View = json["view"].ToString();
                            if (json.ContainsKey("favorite"))
                                basic.Favorite = json["favorite"].ToString();
                        }
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
            if (!WebStatusHelper.IsOnline())
            {
                return null;
            }
            if (!ApiHelper.IsLogin())
            {
                return null;
            }
            try
            {
                Count count = new Count();
                string url = "http://message.bilibili.com/api/notify/query.notify.count.do?access_key=" + ApiHelper.accesskey + "&actionKey=appkey" + "&appkey=422fd9d7289a1dd9&build=427000&platform=android&ts=" + ApiHelper.GetLinuxTS().ToString();
                url += ApiHelper.GetSign(url);
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = json["data"].GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                return null;
            }
            if (!ApiHelper.IsLogin())
            {
                return null;
            }
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
                        JsonObject temp = item.GetObject();
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
                            json = temp["publisher"].GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                return null;
            }
            if (!ApiHelper.IsLogin())
            {
                return null;
            }
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
                        JsonObject temp = item.GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                return null;
            }
            if (!ApiHelper.IsLogin())
            {
                return null;
            }
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
                        JsonObject temp = item.GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                return null;
            }
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
                        JsonObject temp = item.GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            if (!ApiHelper.IsLogin())
            {
                report("请先登录");
                return null;
            }
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
                        JsonObject temp = item.GetObject();
                        if (temp.ContainsKey("aid"))
                            h.Aid = temp["aid"].ToString();
                        if (temp.ContainsKey("pic"))
                            h.Pic = StringDeal.delQuotationmarks(temp["pic"].ToString());
                        if (temp.ContainsKey("title"))
                            h.Title = StringDeal.delQuotationmarks(temp["title"].ToString());
                        if (temp.ContainsKey("view_at")) 
                        {
                            h.Time = StringDeal.LinuxToData(temp["view_at"].ToString());
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
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            if (!ApiHelper.IsLogin())
            {
                report("请先登录");
                return null;
            }
            List<Folder> myFolder = new List<Folder>();
            string url_folder = "http://api.bilibili.com/x/app/favourite/folder?_device=android&_ulv=10000&access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&build=427000&platform=android&vmid=" + UserHelper.mid + "&rnd=" + new Random().Next(1000, 2000).ToString();
            url_folder += ApiHelper.GetSign(url_folder);
            JsonObject json_f = await BaseService.GetJson(url_folder);
            if (json_f.ContainsKey("data"))
            {
                JsonArray favs = json_f["data"].GetArray();
                foreach (var item in favs)
                {
                    JsonObject json2 = item.GetObject();
                    Folder fav = new Folder();
                    if (json2.ContainsKey("ctime"))
                        fav.Ctime = StringDeal.LinuxToData(json2["ctime"].ToString());
                    if (json2.ContainsKey("cur_count"))
                        fav.Count = json2["cur_count"].ToString();
                    if (json2.ContainsKey("fid"))
                        fav.Fid = json2["fid"].ToString();
                    if (json2.ContainsKey("max_count"))
                        fav.MCount = json2["max_count"].ToString();
                    if (json2.ContainsKey("state"))
                        fav.State = json2["state"].ToString();
                    if (json2.ContainsKey("name"))
                        fav.Name = json2["name"].GetString();
                    if (json2.ContainsKey("videos"))
                    {
                        Pic pic = new Pic();
                        JsonArray array = json2["videos"].GetArray();
                        pic.Count = array.Count;
                        List<string> list = new List<string>();
                        for (int i = 0; i < array.Count; i++)
                        {
                            json2 = array[i].GetObject();
                            if (json2.ContainsKey("pic"))
                            {
                                string p = json2["pic"].GetString();
                                switch (i)
                                {
                                    case 0:pic.Pic1 = p;break;
                                    case 1: pic.Pic2 = p; break;
                                    case 2: pic.Pic3 = p; break;
                                }
                            }
                        }
                        fav.VideoPics = pic;
                    }
                    myFolder.Add(fav);
                }
                return myFolder;
            }
            return null;
        }
        //public static async Task<User> GetUserinfoAsync(string mid)
        //{
        //    string url = "http://space.bilibili.com/ajax/member/GetInfo?rnd=" + new Random().Next(0, 1000).ToString();
        //    string result = await BaseService.SendPostAsync(url, "mid=" + mid, "http://space.bilibili.com/" + mid);
        //    JsonObject json = JsonObject.Parse(result);
        //    if (json.ContainsKey("data"))
        //    {
        //        User user = new User();
        //        user.Attentions = new List<string>();
        //        json = json["data"].GetObject();
        //        if (json.ContainsKey("article"))
        //            user.Article = json["article"].ToString();
        //        if (json.ContainsKey("attention"))
        //            user.Attention = json["attention"].ToString();
        //        if (json.ContainsKey("attentions"))
        //        {
        //            JsonArray ar = json["attentions"].GetArray();
        //            foreach (var item in ar)
        //            {
        //                user.Attentions.Add(item.GetNumber().ToString());
        //            }
        //        }
        //        if (json.ContainsKey("birthday"))
        //            user.BirthDay = json["birthday"].GetString();
        //        if (json.ContainsKey("place"))
        //            user.Place = json["place"].GetString();
        //        if (json.ContainsKey("coins"))
        //            user.Coins = json["coins"].ToString();
        //        if (json.ContainsKey("sign"))
        //            user.Sign = StringDeal.delQuotationmarks(json["sign"].ToString());
        //        if (json.ContainsKey("coins"))
        //            user.Coins = json["coins"].ToString();
        //        if (json.ContainsKey("description"))
        //            user.Discription = json["description"].GetString();
        //        if (json.ContainsKey("fans"))
        //            user.Fans = json["fans"].ToString();
        //        if (json.ContainsKey("face"))
        //            user.Face = json["face"].GetString();
        //        if (json.ContainsKey("friend"))
        //            user.Friend = json["friend"].ToString();
        //        if (json.ContainsKey("im9_sign"))
        //            user.Im9_sign = json["im9_sign"].GetString();
        //        if (json.ContainsKey("mid"))
        //            user.Mid = json["mid"].ToString();
        //        if (json.ContainsKey("name"))
        //            user.Name = json["name"].GetString();
        //        if (json.ContainsKey("regtime"))
        //            user.RegTime = StringDeal.LinuxToData(json["regtime"].ToString());
        //        string url2 = "http://space.bilibili.com/ajax/settings/getSettings?mid=" + mid;
        //        JsonObject json_toutu = await BaseService.GetJson(url2);
        //        if (json_toutu.ContainsKey("data"))
        //        {
        //            json_toutu = json_toutu["data"].GetObject();
        //            if (json_toutu.ContainsKey("toutu"))
        //            {
        //                json_toutu = json_toutu["toutu"].GetObject();
        //                if (json_toutu.ContainsKey("l_img"))
        //                    user.Toutu = json_toutu["l_img"].GetString();
        //                if (json_toutu.ContainsKey("s_img"))
        //                    user.Toutu_s = json_toutu["s_img"].GetString();
        //            }
        //        }
        //        if (json.ContainsKey("sex"))
        //            user.Sex = StringDeal.delQuotationmarks(json["sex"].ToString());
        //        if (json.ContainsKey("level_info"))
        //        {
        //            JsonObject json2 = json["level_info"].GetObject();
        //            if (json2.ContainsKey("current_exp"))
        //                user.Current_exp = json2["current_exp"].ToString();
        //            if (json2.ContainsKey("next_exp"))
        //                user.Next_exp = json2["next_exp"].ToString();
        //            if (json2.ContainsKey("current_min"))
        //                user.Current_min = json2["current_min"].ToString();
        //            if (json2.ContainsKey("current_level"))
        //                user.Current_level = json2["current_level"].ToString();
        //        }
        //        return user;
        //    }
        //    return null;
        //}

        /// <summary>
        /// 获取硬币记录
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CoinHs>> GetCoinHistoryAsync()
        {
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            if (!ApiHelper.IsLogin())
            {
                report("请先登录");
                return null;
            }
            try
            {
                List<CoinHs> list = new List<CoinHs>();
                string url = "http://account.bilibili.com/site/GetMoneyLog";
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = json["data"].GetObject();
                    if (json.ContainsKey("result"))
                    {
                        JsonArray array = json["result"].GetArray();
                        foreach (var item in array)
                        {
                            CoinHs hs = new CoinHs();
                            json = item.GetObject();
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
            if (!WebStatusHelper.IsOnline())
            {
                return null;
            }
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
                    json = json["data"].GetObject();
                    if (json.ContainsKey("list"))
                    {
                        JsonArray ja = json["list"].GetArray();
                        foreach (var item in ja)
                        {
                            Friend friend = new Friend();
                            JsonObject temp = item.GetObject();
                            if (temp.ContainsKey("face"))
                                friend.Face = temp["face"].GetString();
                            if (temp.ContainsKey("uname"))
                                friend.Uname = temp["uname"].GetString();
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

        /// <summary>
        /// 获取排行榜
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public static async Task<List<Rank>> GetRankItemsAsync(string tid)
        {
            if (!WebStatusHelper.IsOnline())
            {
                report("没有网络连接");
                return null;
            }
            List<Rank> list = new List<Rank>();
            string url = "http://www.bilibili.com/index/rank/all-03-" + tid + ".json";
            JsonObject json = await BaseService.GetJson(url);
            try
            {
                int i = 1;
                json = json["rank"].GetObject();
                JsonArray array = json["list"].GetArray();
                foreach (var item in array)
                {
                    Rank rank = new Rank();
                    json = item.GetObject();
                    if (json.ContainsKey("aid"))
                        rank.Aid = json["aid"].GetString();
                    if (json.ContainsKey("author"))
                        rank.Author = json["author"].GetString();
                    if (json.ContainsKey("coins"))
                        rank.Coins = json["coins"].ToString();
                    if (json.ContainsKey("duration"))
                        rank.Duration = json["duration"].GetString();
                    if (json.ContainsKey("create"))
                        rank.Create = json["create"].GetString();
                    if (json.ContainsKey("pic"))
                        rank.Pic = json["pic"].GetString();
                    if (json.ContainsKey("title"))
                        rank.Title = json["title"].GetString();
                    if (json.ContainsKey("favorites"))
                        rank.Favorites = json["favorites"].ToString();
                    if (json.ContainsKey("play"))
                        rank.Play = json["play"].ToString();
                    rank.Ranking = i.ToString();
                    i++;
                    list.Add(rank);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<FlipItem>> GetBangumiBanners()
        {
            List<FlipItem> items = new List<FlipItem>();
            string url = "https://bangumi.bilibili.com/api/app_index_page_v4_2?access_key=" + ApiHelper.accesskey + "&appkey=" + ApiHelper.appkey + "&build=411005&mobi_app=android&platform=android&ts=" + ApiHelper.GetLinuxTS().ToString();
            url += ApiHelper.GetSign(url);
            JsonObject json = await BaseService.GetJson(url);
            if (json["code"].ToString() == "0")
            {
                if (json.ContainsKey("result"))
                {
                    json = json["result"].GetObject();
                    if (json.ContainsKey("ad"))
                    {
                        json = json["ad"].GetObject();
                        if (json.ContainsKey("head"))
                        {
                            JsonArray array = json["head"].GetArray();
                            foreach (var temp in array)
                            {
                                FlipItem item = new FlipItem();
                                json = temp.GetObject();
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
                }
                return items;
            }
            else
            {
                return null;
            }
        }


        public static async Task<List<PraiseMe>> GetPraiseListAsync()
        {
            List<PraiseMe> list = new List<PraiseMe>();
            try
            {
                string url = "http://message.bilibili.com/api/notify/query.praiseme.list.do?_device=wp&&_ulv=10000&access_key=" + ApiHelper.accesskey + "&actionKey=appkey&appkey=" + ApiHelper.appkey + "&build=410005&data_type=1&page_size=40&platform=android&ts=" + ApiHelper.GetLinuxTS().ToString();
                url += ApiHelper.GetSign(url);
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    JsonArray array = json["data"].GetArray();
                    foreach (var item in array)
                    {
                        PraiseMe pr = new PraiseMe();
                        string title = string.Empty;
                        json = item.GetObject();
                        if (json.ContainsKey("content"))
                            pr.Content = json["content"].GetString();
                        if (json.ContainsKey("title"))
                            title = json["title"].GetString();
                        if (json.ContainsKey("time_at"))
                            pr.Time = json["time_at"].GetString();
                        if (json.ContainsKey("publisher"))
                        {
                            json = json["publisher"].GetObject();
                            if(json.ContainsKey("face"))
                                pr.Face = json["face"].GetString();
                            if (json.ContainsKey("mid"))
                                pr.Mid = json["mid"].ToString();
                            if (json.ContainsKey("name"))
                                pr.Name = json["name"].GetString();
                        }
                        pr.Title = Regex.Match(title, @"(?<={).*?(?=})").Value;
                        pr.Aid = Regex.Match(title, @"(?<=av)\d+").Value;
                        list.Add(pr);
                    }
                } 
                return list;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<FlipItem>> GetHomeBanners()
        {
            List<FlipItem> list = new List<FlipItem>();
            try
            {
                string url = "http://app.bilibili.com/x/banner?plat=4&build=412001";
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    JsonArray array = json["data"].GetArray();
                    foreach (var temp in array)
                    {
                        FlipItem item = new FlipItem();
                        json = temp.GetObject();
                        if (json.ContainsKey("image"))
                            item.Img = json["image"].GetString();
                        if (json.ContainsKey("title"))
                            item.Title = json["title"].GetString();
                        if (json.ContainsKey("value"))
                            item.Link = json["value"].GetString();
                        list.Add(item);
                    }
                }
            }
            catch
            {

            }
            return list;
        }
    }
}