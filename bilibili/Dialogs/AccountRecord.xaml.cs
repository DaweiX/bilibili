using bilibili.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class AccountRecord : ContentDialog
    {
        public AccountRecord()
        {
            this.InitializeComponent();
            GetRecords();
        }

        async void GetRecords()
        {
            try
            {
                string url = "https://account.bilibili.com/site/GetLoginLog";
                JsonObject json = await BaseService.GetJson(url);
                if (json.ContainsKey("data"))
                {
                    json = json["data"].GetObject();
                    if (json.ContainsKey("result"))
                    {
                        JsonArray array = json["result"].GetArray();
                        for (int i = 0; i < array.Count; i++)
                        {
                            JsonObject json2 = array[i].GetObject();
                            Record record = new Record();
                            if (json2.ContainsKey("geo"))
                                record.Geo = json2["geo"].GetString();
                            if (json2.ContainsKey("ip"))
                                record.IP = json2["ip"].GetString();
                            if (json2.ContainsKey("time_at"))
                                record.Time = json2["time_at"].GetString();
                            list.Items.Add(record);
                        }
                    }
                }
            }
            catch
            {

            }          
        }

        class Record
        {
            public string Geo { get; set; }
            public string IP { get; set; }
            public string Time { get; set; }
        }
    }
}
