using bilibili.Http;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

//  “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace bilibili.Dialogs
{
    public sealed partial class CoinHistory : ContentDialog
    {
        public CoinHistory()
        {
            this.InitializeComponent();
            load();
        }

        async void load()
        {
            list.ItemsSource = await ContentServ.GetCoinHistoryAsync();
        }
    }
}
