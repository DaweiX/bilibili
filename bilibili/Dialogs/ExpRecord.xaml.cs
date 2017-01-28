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

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace bilibili.Dialogs
{
    public sealed partial class ExpRecord : ContentDialog
    {
        public ExpRecord()
        {
            this.InitializeComponent();
            GetExpHistoryAsync();
        }

        class Exp
        {
            public string Delta { get; set; }
            public string Reason { get; set; }
            public string Time { get; set; }
        }

        async void GetExpHistoryAsync()
        {
            try
            {
                string url = "https://account.bilibili.com/site/GetExpLog";
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = json["data"].GetObject();
                    if (json.ContainsKey("result"))
                    {
                        JsonArray array = json["result"].GetArray();
                        for (int i = 0; i < array.Count; i++)
                        {
                            Exp exp = new Exp();
                            json = array[i].GetObject();
                            if (json.ContainsKey("delta"))
                            {
                                exp.Delta = json["delta"].ToString();
                            }
                            if (json.ContainsKey("reason"))
                            {
                                exp.Reason = json["reason"].GetString();
                            }
                            if (json.ContainsKey("time"))
                            {
                                exp.Time = json["time"].GetString();
                            }
                            list.Items.Add(exp);
                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}
