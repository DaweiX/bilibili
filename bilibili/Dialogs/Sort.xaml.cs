using Windows.UI.Xaml.Controls;
using bilibili.Helpers;
using System.Text;
using System;

//  “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace bilibili.Dialogs
{
    public sealed partial class Sort : ContentDialog
    {
        public Action action;
        string defaultSort = "番剧_13,动画_1,生活_160,电影_23,娱乐_71,鬼畜_119,科技_36,游戏_4,音乐_3,舞蹈_20,时尚_155,广告_166,电视剧_11";
        string sort = string.Empty;
        public Sort()
        {
            this.InitializeComponent();
            sort = SettingHelper.GetValue("_sort").ToString();
            if (sort == string.Empty)
            {
                sort = defaultSort;
            }
            foreach (var item in sort.Split('.'))
            {
                list.Items.Add(new SortItem
                {
                    Title = item.Split('_')[0],
                    Tid = item.Split('_')[1]
                });
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in list.Items)
            {
                SortItem sortItem = item as SortItem;
                builder.Append( $"{sortItem.Title}_{sortItem.Tid}.");
            }
            SettingHelper.SetValue("_sort", builder.Remove(builder.Length - 1, 1).ToString());
            action();
        }

        class SortItem
        {
            public string Title { get; set; }
            public string Tid { get; set; }
        }
    }
}
