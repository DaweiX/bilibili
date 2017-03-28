using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace bilibili.Http
{
   ///<summary>
   ///访问HTTP的基础服务
   /// </summary>
    class BaseService
    {
       ///<summary>
       ///发送Get请求
       /// </summary>
        public async static Task<string> SentGetAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri(url);
                HttpResponseMessage msg = await client.GetAsync(uri);
                IInputStream stream = await msg.Content.ReadAsInputStreamAsync();
                var reader = new StreamReader(stream.AsStreamForRead(), System.Text.Encoding.UTF8);
                string str = await reader.ReadToEndAsync();
                return str;
            }
        }
        /// <summary>
        /// 获得Json数据
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<JsonObject> GetJson(string url)
        {
            string json = await SentGetAsync(url);
            if (json != null)
                return JsonObject.Parse(json);
            else
                return null;
        }
       /// <summary>
       /// 发送Post请求
       /// </summary>
       /// <param name="url"></param>
       /// <param name="message"></param>
       /// <returns></returns>
        public static async Task<string> SendPostAsync(string url, string message,string refer= "http://www.bilibili.com/")
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Referer = new Uri(refer);
                    var response = await client.PostAsync(new Uri(url), new HttpStringContent(message, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
           catch(Exception)
            {
                return string.Empty;
            }
        }
    }
}