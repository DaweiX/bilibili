using System;
using Windows.UI.Xaml.Controls;

namespace bilibili.FrameManager
{
    public interface IframeManager
    {
        event EventHandler<bool> LeftFrameContentChange;
        event EventHandler<bool> RightFrameContentChange;
        Frame RightFrame { get; set; }
        Frame LeftFrame { get; set; }
        Frame MainFrame { get; set; }
        void RightFrameNavi(Action<Frame> action);
        void LeftFrameNavi(Action<Frame> action);
    }
}
