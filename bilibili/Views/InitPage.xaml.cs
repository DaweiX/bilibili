using bilibili.Animation.Root;
using bilibili.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class InitPage : Page
    {
        string arg = string.Empty;
        public InitPage()
        {
            this.InitializeComponent();
            SettingHelper.Devicetype = SettingHelper.GetDeviceType();
            if (SettingHelper.Devicetype == DeviceType.Mobile)
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await this.Scale(0.5f, 0.5f, (float)ActualWidth / 2, (float)ActualHeight / 2, 1).StartAsync();
            await this.Scale(1f, 1f, (float)ActualWidth / 2, (float)ActualHeight / 2, 300).StartAsync();
            if (e.Parameter.ToString().Length > 0) 
            {
                arg = e.Parameter.ToString();
            }
            BackgroundTaskRegistration task;
            bool isLogin = await autologin();
            if (isLogin)
            {
                if (SettingHelper.GetValue("_pull") != null)
                {
                    if ((bool)SettingHelper.GetValue("_pull") == true)
                    {
                        task = await RegisterBackgroundTask(typeof(BackgroundTask.TileTask), "TileTask", new TimeTrigger(15, false), null);
                    }
                }
                else
                {
                    task = await RegisterBackgroundTask(typeof(BackgroundTask.TileTask), "TileTask", new TimeTrigger(15, false), null);
                }
            }
            await Task.Delay(1000);
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
            Frame rootFrame = new Frame();
            Window.Current.Content = rootFrame;
            rootFrame.Navigate(typeof(MainPage), arg, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        /// <summary>
        /// 自动登录
        /// </summary>
        async Task<bool> autologin()
        {
            try
            {
                if (!WebStatusHelper.IsOnline()) return false;
                if (!SettingHelper.ContainsKey("_accesskey"))
                {
                    SettingHelper.SetValue("_accesskey", string.Empty);
                }
                if (SettingHelper.ContainsKey("_autologin"))
                {
                    if (bool.Parse(SettingHelper.GetValue("_autologin").ToString()) == true)
                    {
                        string p = string.Empty;
                        string u = string.Empty;
                        if (!string.IsNullOrEmpty(SettingHelper.GetValue("_epassword").ToString()))
                        {
                            p = SettingHelper.GetValue("_epassword").ToString();
                        }
                        if (!string.IsNullOrEmpty(SettingHelper.GetValue("_username").ToString()))
                        {
                            u = SettingHelper.GetValue("_username").ToString();
                        }
                        await ApiHelper.login(p, u, false);
                    }
                }
                if (ApiHelper.IsLogin()) return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }


        private async Task<BackgroundTaskRegistration> RegisterBackgroundTask(Type EntryPoint, string name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.DeniedByUser)
            {
                return null;
            }
            foreach (var item in BackgroundTaskRegistration.AllTasks)
            {
                if (item.Value.Name == name)
                {
                    item.Value.Unregister(true);
                }
            }
            var builder = new BackgroundTaskBuilder { Name = name, TaskEntryPoint = EntryPoint.FullName, IsNetworkRequested = false };
            builder.SetTrigger(trigger);
            if (condition != null)
            {
                builder.AddCondition(condition);
            }
            BackgroundTaskRegistration task = builder.Register();
            //await popup.Show(string.Format("----Register{0}-----", task.Name));
            return task;
        }
    }
}
