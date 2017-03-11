using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace bilibili.FrameManager
{
    public abstract class FrameManagerBase : IframeManager
    {

        public FrameManagerBase()
        {
            // FrameManager = this;
            // SystemNavigationManager.GetForCurrentView().BackRequested += FrameManagerBase_BackRequested; ;
            FrameHelpers.action += () => 
            {
                if (RightFrame == null) return false;
                if (RightFrame.CanGoBack)
                {
                    RightFrame.GoBack();
                    RightFrameContentChange?.Invoke(this, RightFrame.CanGoBack);
                }
                else if (LeftFrame.CanGoBack)
                {
                    LeftFrame.GoBack();
                    LeftFrameContentChange?.Invoke(this, LeftFrame.CanGoBack);
                }
                else return false;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = RightFrame.CanGoBack || LeftFrame.CanGoBack
                    ? AppViewBackButtonVisibility.Visible
                    : SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility == AppViewBackButtonVisibility.Visible
                        ? AppViewBackButtonVisibility.Visible
                        : AppViewBackButtonVisibility.Collapsed;
                return true;
            };
        }

        public Frame LeftFrame { get; set; }

        public Frame MainFrame { get; set; }

        public Frame RightFrame { get; set; }

        public event EventHandler<bool> LeftFrameContentChange;
        public event EventHandler<bool> RightFrameContentChange;

        public void LeftFrameNavi(Action<Frame> action)
        {
            action(LeftFrame);
            LeftFrameContentChange?.Invoke(this, true);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        public void RightFrameNavi(Action<Frame> action)
        {
            action(RightFrame);
            RightFrameContentChange?.Invoke(this, true);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }
    }
}
