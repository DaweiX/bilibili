using Windows.UI.Xaml;

namespace bilibili.FrameManager
{
    public enum AdaptiveType
    {
        Left,
        Right,
        Both
    }

    public class FrameDisplayTrigger : StateTriggerBase
    {
        // 400 + 600
        const int both_minwidth = 1000;

        readonly IframeManager _frameManager;
        AdaptiveType _adaptiveType;
        bool rightFrameHasContent;

        public FrameDisplayTrigger()
        {
            _frameManager = FrameHelpers.FrameManager;
            Window.Current.SizeChanged += Current_SizeChanged;
            _frameManager.RightFrameContentChange += _frameManager_RightFrameContentChange;
        }

        private void _frameManager_RightFrameContentChange(object sender, bool e)
        {
            rightFrameHasContent = e;
            var type = AdaptiveDevice(Window.Current.Bounds.Width);
            SetActive(type == AdaptiveType);
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            var type = AdaptiveDevice(e.Size.Width);
            SetActive(type == AdaptiveType);
        }

        private AdaptiveType AdaptiveDevice(double width)
        {
            if (width < both_minwidth)
            {
                return rightFrameHasContent
                    ? AdaptiveType.Right
                    : AdaptiveType.Left;
            }
            else
            {
                return AdaptiveType.Both;
            }
        }

        public AdaptiveType AdaptiveType
        {
            get { return _adaptiveType; }
            set
            {
                _adaptiveType = value;
                var device = AdaptiveDevice(Window.Current.Bounds.Width);
                SetActive(device == _adaptiveType);
            }
        }
    }
}
