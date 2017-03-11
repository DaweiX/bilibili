using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Connectivity;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BackgroundTask
{
    // 别声明成public
    class Helper
    {

        public static string appkey = "422fd9d7289a1dd9";
        static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
       /// <summary>
       /// 获取指定键的值
       /// </summary>
       /// <param name="key">键名称</param>
       /// <returns></returns>
        public static object GetValue(string key)
        {
            if (container.Values[key] != null)
            {
                return container.Values[key];
            }
            else
            {
                return null;
            }
        }
       /// <summary>
       /// 设置指定键的值
       /// </summary>
       /// <param name="key">键名称</param>
       /// <param name="value">值</param>
        public static void SetValue(string key, object value)
        {
            container.Values[key] = value;
        }
       /// <summary>
       /// 指示应用容器内是否存在某键
       /// </summary>
       /// <param name="key">键名称</param>
       /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            if (container.Values[key] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
       /// <summary>
       /// 获取Linux时间戳
       /// </summary>
       /// <returns>无符号整型时间戳</returns>
        public static uint GetLinuxTS()
        {
            uint ts = Convert.ToUInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            return ts;
        }
       /// <summary>
       /// 获取签名
       /// </summary>
       /// <param name="url">要请求的url</param>
       /// <returns></returns>
        public static string GetSign(string url)
        {
            string t = url.Substring(url.IndexOf('?', 6)).Remove(0, 1);
            string[] argss = t.Split('&');
            List<string> list = argss.ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string s in list)
            {
                stringBuilder.Append(stringBuilder.Length > 0 ? "&" : string.Empty);
                stringBuilder.Append(s);
            }
            string a = stringBuilder.ToString();
            string b = "ba3a4e554e9a6e15dc4d1d70c2b154e3";
            string c = "&sign=" + GetMD5(a + b);
            return c;
        }

        static public string GetMD5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var result = CryptographicBuffer.EncodeToHexString(hashed);
            return result;
        }
       /// <summary>
       /// 是否登录
       /// </summary>
       /// <returns></returns>
        public static bool IsLogin()
        {
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            List<string> ls = new List<string>();
            foreach (HttpCookie item in cookieCollection)
            {
                ls.Add(item.Name);
            }
            if (!ls.Contains("DedeUserID") || !ls.Contains("DedeUserID__ckMd5"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
       ///<summary>
       ///发送Get请求
       /// </summary>
        public async static Task<string> SentGetAsync(string url)
        {
            HttpClient client = new HttpClient();
            Uri uri = new Uri(url);
            HttpResponseMessage msg = await client.GetAsync(uri);
            return await msg.Content.ReadAsStringAsync();
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
    }
}