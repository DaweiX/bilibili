using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyWeb : Page
    {
        public MyWeb()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string url = e.Parameter.ToString();
            webview.Navigate(new Uri(url));
        }

        //private void webview_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        //{
        //    string url = args.Uri.AbsoluteUri;
        //    //string ban = Regex.Match(args.Uri.AbsoluteUri, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
        //    if (Regex.IsMatch(url, @"anime/"))
        //    {
        //        this.Frame.Navigate(typeof(Detail), Regex.Match(url, @"(?<=anime/)\d*"));
        //        return;
        //    }
        //    //string ban2 = Regex.Match(args.Uri.AbsoluteUri, @"^http://www.bilibili.com/bangumi/i/(.*?)$").Groups[1].Value;
        //    if (Regex.IsMatch(url, @"bangumi/i/"))
        //    {
        //        this.Frame.Navigate(typeof(Detail), Regex.Match(url, @"(?<=bangumi/i/)\d*"));
        //        return;
        //    }
        //    //string ban3 = Regex.Match(args.Uri.AbsoluteUri, @"^bilibili://?av=(.*?)$").Groups[1].Value;
        //    if (Regex.IsMatch(url, @"av/"))
        //    {
        //        this.Frame.Navigate(typeof(Detail_P), Regex.Match(url, @"(?<=av/)\d*"));
        //        return;
        //    }
        //    //if (Regex.IsMatch(args.Uri.AbsoluteUri, "/video/av(.*)?[/|+](.*)?"))
        //    //{

        //    //    string a = Regex.Match(args.Uri.AbsoluteUri, "/video/av(.*)?[/|+](.*)?").Groups[1].Value;
        //    //    this.Frame.Navigate(typeof(Detail_P), a);
        //    //}
        //}

        private void webview_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            string para = string.Empty;
            args.Handled = true;
            string url = args.Uri.AbsoluteUri;
            if (Regex.IsMatch(url, @"anime/"))
            {
                para = Regex.Match(url, @"(?<=anime/)\d*").Value;
                this.Frame.Navigate(typeof(Detail),para);
                return;
            }
            if (Regex.IsMatch(url, @"bangumi/i/"))
            {
                para = Regex.Match(url, @"(?<=bangumi/i/)\d*").Value;
                this.Frame.Navigate(typeof(Detail),para);
                return;
            }
            if ((Regex.IsMatch(url, @"/video/av")))
            {
                para = Regex.Match(url, @"(?<=/video/av)\d*").Value;
                Frame.Navigate(typeof(Detail_P), para);
                return;
            }
            else
            {
                webview.Navigate(new Uri(url));
            }
        }
    }
}
