using Windows.UI.Xaml.Controls;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace bilibili.Dialogs
{
    public sealed partial class SendReply : ContentDialog
    {
        public delegate void ReplyHandler(string txt);
        public event ReplyHandler Send;
        public SendReply()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Send(txt.Text);
        }
    }
}
