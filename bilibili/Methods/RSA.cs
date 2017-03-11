using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Web.Http;
using Windows.Storage.Streams;
using System.Net;
using Windows.Web.Http.Filters;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using bilibili.Http;
using Windows.Security.Cryptography.Certificates;

namespace bilibili.Methods
{
    class RSA
    {
       /// <summary>
       /// 获取RSA加密密文
       /// </summary>
       /// <param name="passWord">明文</param>
       /// <returns></returns>
        public static async Task<string> GetRSA(string passWord)
        {
            string base64String;
            try
            {
                // https:// secure.bilibili.com/login?act=getkey&rnd=4928
                // https:// passport.bilibili.com/login?act=getkey&rnd=4928
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                HttpClient httpClient = new HttpClient(httpBaseProtocolFilter);
                string stringAsync = await httpClient.GetStringAsync((new Uri("https:// secure.bilibili.com/login?act=getkey&rnd=" + new Random().Next(1000, 9999), UriKind.Absolute)));
                JsonObject jObjects = JsonObject.Parse(stringAsync);
                string str = jObjects["hash"].ToString();
                string str1 = jObjects["key"].ToString();
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
                throw;             
            }
            return base64String;
        }
    }
}
