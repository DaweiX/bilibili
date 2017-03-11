using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using bilibili.Http;
using bilibili.Methods;
using System.Text.RegularExpressions;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace bilibili.Helpers
{
    class ApiHelper
    {
        public const string appkey = "422fd9d7289a1dd9";
        public static string accesskey = string.Empty;
        public static int code = 233;
        public static string Password, Username, e_password = string.Empty;
        public delegate void LoginReport(string message);
        public static event LoginReport Report;
        public static bool isfirst = true;
        static bool isTryOnce = false;
       /// <summary>
       /// 获取Linux时间戳
       /// </summary>
       /// <returns>无符号整型时间戳</returns>
        public static uint GetLinuxTS()
        {
            uint ts = Convert.ToUInt32((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            return ts;
        }

        public static async Task<string> GetEncryptedPassword(string password,string uname)
        {
            string base64String;
            try
            {
                // https:// secure.bilibili.com/login?act=getkey&rnd=4928
                // https:// passport.bilibili.com/login?act=getkey&rnd=4928
                string url = "https:// passport.bilibili.com/api/oauth2/getKey?appkey=" + appkey + "&ts=" + GetLinuxTS().ToString();
                // url += GetSign(url);
                Report("正在加密密码");
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                HttpClient httpClient = new HttpClient(httpBaseProtocolFilter);
                string result = await BaseService.SendPostAsync(url, "http://passport.bilibili.com");
                JsonObject json = JsonObject.Parse(result);
                json = json["data"].GetObject();
                string str = json["hash"].GetString();
                string str1 = json["key"].GetString();
                // 加盐为什么要用密码加？这样的加盐没什么意义，但貌似服务器只认这样的加密方式(lll￢ω￢)
                string str2 = string.Concat(str, password);
                string str3 = Regex.Match(str1, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim();
                byte[] numArray = Convert.FromBase64String(str3);
                AsymmetricKeyAlgorithmProvider asymmetricKeyAlgorithmProvider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
                CryptographicKey cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(WindowsRuntimeBufferExtensions.AsBuffer(numArray), 0);
                IBuffer buffer = CryptographicEngine.Encrypt(cryptographicKey, WindowsRuntimeBufferExtensions.AsBuffer(Encoding.UTF8.GetBytes(str2)), null);
                base64String = Convert.ToBase64String(WindowsRuntimeBufferExtensions.ToArray(buffer));
                Report("完毕。正在登录");
            }
            catch (Exception)
            {
                // throw;
                base64String = password;
            }
            return base64String;
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
            string c = "&sign=" + Secret.GetMD5(a + b);
            return c;
        }
       /// <summary>
       /// 获取指定参数的请求字符串
       /// </summary>
       /// <param name="order">排列顺序</param>
        public static string GetUrl(int tid, int page, int pagesize, Order order = Order._default)
        {
            return "http://api.bilibili.com/list?_device=wp&_ulv=10000&build=424000&platform=android&appkey=422fd9d7289a1dd9&tid=" + tid.ToString() + "&page=" + page.ToString() + "&pagesize=" + pagesize.ToString() + "&order=" + order.ToString().Remove(0, 1) + "&ver=2&rnd=" + new Random().Next(1000, 9999).ToString();
        }
        public enum Order
        {
            _hot,
            _default,
            _others,
        }
       /// <summary>
       /// 获取AccessKey
       /// </summary>
       /// <param name="password"></param>
       /// <param name="username"></param>
       /// <returns>成功返回true</returns>
        public async static Task<bool> GetAccessKey(string pwd,string uname)
        {
            string accesskey1 = string.Empty;
            string mid = string.Empty;
            e_password = await GetEncryptedPassword(pwd, uname);
            string p = isfirst ? e_password : pwd;
            // https:// passport.bilibili.com/api/oauth2/login?appkey=422fd9d7289a1dd9&platform=uwp&password=dmkiFvBbaX91U%2Fv%2F7tr2xGSofxLR9ldh7vizCNKCBhFAU7kF6AK71LrRzCSOw5wF%2FgmUMT5LiJKqZyCYZTPb656Ti5aiKgxsNH0PQ6qbSSQVOytQBS5LXiHxGR8D%2FJv3NW6EGjlbLD25VS%2FVWLn1XpGc90peI6aIXkh%2FvZtIK7M%3D&username=DaweiX%40outlook.com&ts=1484100747&sign=1717ec912bd7e9a6e35b395740e5d624
            string a = WebUtility.UrlEncode(p);
            // dmkiFvBbaX91U%2Fv%2F7tr2xGSofxLR……
            // string url = "https:// passport.bilibili.com/api/oauth2/login?appkey=" + appkey + "&platform=uwp&password=" + WebUtility.UrlEncode(p) + "&username=" + WebUtility.UrlEncode(username) + "&ts=" + ApiHelper.GetLinuxTS().ToString();
            // url += GetSign(url);
            string url = "https:// api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=" + a + "&type=json&userid=" + WebUtility.UrlEncode(uname);
            url += GetSign(url);
            // string result = await BaseService.SendPostAsync(url, "http://passport.bilibili.com");
            // JsonObject json = JsonObject.Parse(result);
            // if (json.ContainsKey("code"))
            //     code = json["code"].ToString();
            // if (json.ContainsKey("data"))
            // {
            //     json = json["data"].GetObject();
            //     if (json.ContainsKey("access_token"))
            //         accesskey1 = json["access_token"].GetString();
            //     if (json.ContainsKey("mid"))
            //         mid = json["mid"].ToString();
            // }
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("code"))
                code = (int)json["code"].GetNumber();
            if (code != 0)
                return false;
            if (json.ContainsKey("access_key"))
                accesskey1 = json["access_key"].GetString();
            if (json.ContainsKey("mid"))
                mid = json["mid"].ToString();
            if (accesskey1 != string.Empty)
            {
                accesskey = accesskey1;
                SettingHelper.SetValue("_accesskey", accesskey);
                return true;
            }
            return false;
        }
       /// <summary>
       /// 是否登录
       /// </summary>
       /// <returns></returns>
        public static bool IsLogin()
        {
            if (!WebStatusHelper.IsOnline()) return false;
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
       /// <summary>
       /// 登录
       /// </summary>
       /// <param name="password"></param>
       /// <param name="username"></param>
       /// <returns></returns>
        public async static Task login(string pwd, string uname, bool isFirst = true)
        {
            string url = string.Empty;
            Password = pwd;
            Username = uname;
            if (IsLogin()) return;
            isfirst = isFirst;
            // string url = "http://api.bilibili.com/login/sso?gourl=http%3A%2F%2Fwww.bilibili.com&access_key=" + accesskey + "&appkey=" + appkey + "&platform=android&scale=xhdpi";
            // "http://api.bilibili.com/login/sso?&access_key=" + model.access_key + "&appkey=422fd9d7289a1dd9&platform=wp"
            if (!string.IsNullOrEmpty(SettingHelper.GetValue("_accesskey").ToString()))
            {
                accesskey = SettingHelper.GetValue("_accesskey").ToString();
                url = "http://api.bilibili.com/login/sso?&access_key=" + accesskey + "&appkey=" + appkey + "&platform=wp";
                await BaseService.SentGetAsync(url);
            }
            else
            {
                bool key = await GetAccessKey(pwd, uname);
                if (key)
                {
                    url = "http://api.bilibili.com/login/sso?&access_key=" + accesskey + "&appkey=" + appkey + "&platform=wp";
                    await BaseService.SentGetAsync(url);
                }
            }
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            List<string> ls = new List<string>();
            foreach (HttpCookie item in cookieCollection)
            {
                ls.Add(item.Name);
            }
            if (!ls.Contains("DedeUserID") || !ls.Contains("DedeUserID__ckMd5"))
            {
                SettingHelper.SetValue("_accesskey", accesskey);
                hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
                SettingHelper.SetValue("_accesskey", accesskey);
                SettingHelper.SetValue("_username", uname);
                SettingHelper.SetValue("_epassword", e_password);
                code = 0;
                return;
            }
            else
            {
                try
                {
                    if (isTryOnce == false)
                    {
                        if (await GetAccessKey(pwd, uname))
                        {
                            isTryOnce = true;
                            await login(pwd, uname);
                            return;
                        }
                    }                 
                }
                catch(Exception e)
                {
                    string a = e.Message;
                    return;
                }
            }
        }
       /// <summary>
       /// 登出
       /// </summary>
        public static void logout()
        {
            List<HttpCookie> listCookies = new List<HttpCookie>();
            listCookies.Add(new HttpCookie("sid", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("DedeUserID", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("DedeUserID__ckMd5", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("SESSDATA", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("LIVE_LOGIN_DATA", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("LIVE_LOGIN_DATA__ckMd5", ".bilibili.com", "/"));
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            foreach (HttpCookie cookie in listCookies)
            {
                filter.CookieManager.DeleteCookie(cookie);
            }
            ApiHelper.accesskey = string.Empty;
            SettingHelper.SetValue("_islogin", false);
            SettingHelper.SetValue("_autologin", false);
            SettingHelper.SetValue("_accesskey", string.Empty);
            SettingHelper.SetValue("_toastquene", string.Empty);
        }
    }
}
