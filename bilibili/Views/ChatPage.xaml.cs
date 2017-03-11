using bilibili.Helpers;
using bilibili.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//  “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
   /// <summary>
   /// 可用于自身或导航至 Frame 内部的空白页。
   /// </summary>
    public sealed partial class ChatPage : Page
    {
        string rid = string.Empty;
        public ChatPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = "http://message.bilibili.com/api/msg/send.msg.do";
                string result = await BaseService.SendPostAsync(url, string.Format("access_key={0}&actionKey=appkey&appkey={1}build=422000&data_type=1&mobi_app=android&platform=android&rid={2}&ts={3}000&msg={4}", ApiHelper.accesskey, ApiHelper.appkey, rid, ApiHelper.GetLinuxTS().ToString(), Uri.EscapeDataString(txt.Text)));
                JsonObject json = JsonObject.Parse(result);
                if (json["code"].ToString() != "0")
                {
                    // 发送失败
                }
            }
            catch  
            {
                // 发送失败
            }
        }
    }
}
