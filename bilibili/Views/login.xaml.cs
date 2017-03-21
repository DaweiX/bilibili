using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using bilibili.Helpers;
using System.Threading.Tasks;

//  “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
   /// <summary>
   /// 可用于自身或导航至 Frame 内部的空白页。
   /// </summary>
    public sealed partial class login : Page
    {
        public delegate void ReportLogin();
        public event ReportLogin reportLogin;
        static string accesskey = string.Empty;
        static string mid = string.Empty;
        public login()
        {
            this.InitializeComponent();
            ApiHelper.Report += ProcessReport;
        }

        private async void ProcessReport(string message)
        {
            await ShowMessageAsync(message);
        }

        private async Task ShowMessageAsync(string message)
        {
            stk_message.Visibility = Visibility.Visible;
            status.Text = message;
            await Task.Delay(2000);
            stk_message.Visibility = Visibility.Collapsed;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txt2.Password) || string.IsNullOrEmpty(txt1.Text))
            {
                return;
            }
            int code = await ApiHelper.login(txt2.Password, txt1.Text);
            switch (code)
            {
                case 0:
                    {
                        await ShowMessageAsync("登录成功");
                        SettingHelper.SetValue("_autologin", (bool)autologin.IsChecked);
                        reportLogin();
                        Frame.GoBack();
                    }
                    break;
                case -1:
                    {
                        await ShowMessageAsync("233");
                    }
                    break;
                case -628:
                    {
                        await ShowMessageAsync("纳尼？未知错误");
                    }
                    break;
                case -627:
                    {
                        await ShowMessageAsync("密码错啦");
                    }
                    break;
                case -626:
                    {
                        await ShowMessageAsync("账号不存在哦");
                    }
                    break;
                case -625:
                    {
                        await ShowMessageAsync("手残多次了，233");
                    }
                    break;
            }   
        }

        private void txt2_GotFocus(object sender, RoutedEventArgs e)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri("ms-appx:///Assets//Images//22_hide.png");
            img_22.Source = bmp;
            BitmapImage bmp2 = new BitmapImage();
            bmp2.UriSource = new Uri("ms-appx:///Assets//Images//33_hide.png");
            img_33.Source = bmp2;
        }

        private void txt2_LostFocus(object sender, RoutedEventArgs e)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource= new Uri("ms-appx:///Assets//Images//22.png");
            img_22.Source = bmp;
            BitmapImage bmp2 = new BitmapImage();
            bmp2.UriSource = new Uri("ms-appx:///Assets//Images//33.png");
            img_33.Source = bmp2;
        }
    }
}
