using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace bilibili.FrameManager
{
    public class FrameHelpers : FrameManagerBase
    {
        public static IframeManager FrameManager { get; private set; }
        public delegate bool GoBack();
        public static GoBack action;
        public static void Init()
        {
            FrameManager = new FrameHelpers();
            Frame rootFrame = Window.Current.Content as Frame;
            FrameManager.MainFrame = rootFrame = new Frame();
            rootFrame.ContentTransitions = new TransitionCollection { new NavigationThemeTransition() };
        }
    }
}
