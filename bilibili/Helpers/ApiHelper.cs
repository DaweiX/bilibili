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
        public static string appkey = "422fd9d7289a1dd9";
        public static string accesskey = string.Empty;
        public static string code = string.Empty;
        public static string password, username, e_password;
        public static bool isfirst = true;
        /// <summary>
        /// 获取Linux时间戳
        /// </summary>
        /// <returns>无符号整型时间戳</returns>
        public static uint GetLinuxTS()
        {
            uint ts = Convert.ToUInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            return ts;
        }

        public static async Task<string> GetEncryptedPassword(string passWord)
        {
            string base64String;
            try
            {
                //https://secure.bilibili.com/login?act=getkey&rnd=4928
                //https://passport.bilibili.com/login?act=getkey&rnd=4928
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                HttpClient httpClient = new Windows.Web.Http.HttpClient(httpBaseProtocolFilter);
                //WebClientClass wc = new WebClientClass();
                string stringAsync = await httpClient.GetStringAsync((new Uri("https://secure.bilibili.com/login?act=getkey&rnd=" + new Random().Next(1000, 9999), UriKind.Absolute)));
                JsonObject json = JsonObject.Parse(stringAsync);
                string str = json["hash"].GetString();
                string str1 = json["key"].GetString();
                string str2 = string.Concat(str, passWord);
                string str3 = Regex.Match(str1, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim();
                byte[] numArray = Convert.FromBase64String(str3);
                AsymmetricKeyAlgorithmProvider asymmetricKeyAlgorithmProvider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
                CryptographicKey cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(WindowsRuntimeBufferExtensions.AsBuffer(numArray), 0);
                IBuffer buffer = CryptographicEngine.Encrypt(cryptographicKey, WindowsRuntimeBufferExtensions.AsBuffer(Encoding.UTF8.GetBytes(str2)), null);
                base64String = Convert.ToBase64String(WindowsRuntimeBufferExtensions.ToArray(buffer));
            }
            catch (Exception)
            {
                //throw;
                base64String = passWord;
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
        /// <returns></returns>
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
            string p = isfirst ? await GetEncryptedPassword(pwd) : pwd;        
            string url = "https://api.bilibili.com/login?appkey=" + appkey + "&platform=wp&pwd=" + WebUtility.UrlEncode(p) + "&type=json&userid=" + WebUtility.UrlEncode(username);
            JsonObject json = await BaseService.GetJson(url);
            if (json.ContainsKey("access_key"))
                accesskey1 =json["access_key"].GetString();
            if (json.ContainsKey("mid"))
                mid = json["mid"].ToString();
            if (json.ContainsKey("code"))
                code = json["code"].ToString();
            if (accesskey1 != string.Empty && code == "0") 
            {
                accesskey = accesskey1;
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
                hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
                SettingHelper.SetValue("_username", username);
                SettingHelper.SetValue("_epassword", e_password);
                return true;
            }
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="password"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public async static Task<bool> login(string pwd, string uname, bool isFirst = true)
        {
            username = uname;
            password = pwd;
            isfirst = isFirst;
            HttpResponseMessage message = new HttpResponseMessage();
            if (!string.IsNullOrEmpty(SettingHelper.GetValue("_accesskey").ToString()))
            {
                //http://api.bilibili.com/login/sso?&access_key=c0ca6415ce6d8bcb7bda0ea9bc9a2419&appkey=422fd9d7289a1dd9&platform=wp
                string url = "http://api.bilibili.com/login/sso?gourl=http%3A%2F%2Fwww.bilibili.com&access_key=" + accesskey + "&appkey=" + appkey + "&platform=android&scale=xhdpi";
                url += GetSign(url);
                message = await new HttpClient().GetAsync(new Uri(url));
                accesskey = SettingHelper.GetValue("_accesskey").ToString();
            }
            else
            {
                bool key = await GetAccessKey(password, username);
                if (key)
                {
                    message = await new HttpClient().GetAsync(new Uri("http://api.bilibili.com/login/sso?&access_key=" + accesskey + "&appkey=" + appkey + "&platform=wp"));
                }
            }
            message.EnsureSuccessStatusCode();
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            List<string> ls = new List<string>();
            foreach (HttpCookie item in cookieCollection)
            {
                ls.Add(item.Name);
            }
            if (ls.Contains("DedeUserID"))
            {
                if (string.IsNullOrEmpty(SettingHelper.GetValue("_accesskey").ToString())) 
                    SettingHelper.SetValue("_accesskey", accesskey);
                return true;
            }
            else
            {
                try
                {
                    if (await GetAccessKey(password, uname))
                    {
                        if (await login(password, uname))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch
                {
                    return false;
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
        }
    }
}
