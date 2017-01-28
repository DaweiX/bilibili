using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace bilibili.Controls
{
    [TemplatePart(Name = Root, Type = typeof(Border))]
    [TemplatePart(Name = Header, Type = typeof(Grid))]
    [TemplatePart(Name = Contentpresenter, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = HeaderTransform, Type = typeof(CompositeTransform))]
    [TemplatePart(Name = ContentTransform, Type = typeof(CompositeTransform))]
    [TemplatePart(Name = ReleaseContent, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PullContent, Type = typeof(ContentPresenter))]
    public sealed class PullToRefreshPanel : Control
    {
        public PullToRefreshPanel()
        {
            this.DefaultStyleKey = typeof(PullToRefreshPanel);
        }

        public static readonly DependencyProperty PullToRefreshContentProperty =
            DependencyProperty.Register(nameof(PullToRefreshContent), typeof(object), typeof(PullToRefreshPanel), new PropertyMetadata("下拉刷新"));

        public static readonly DependencyProperty ReleaseToRefreshContentProperty =
            DependencyProperty.Register(nameof(ReleaseToRefreshContent), typeof(object), typeof(PullToRefreshPanel), new PropertyMetadata("松开刷新"));

        public static readonly DependencyProperty ContentProperty =
          DependencyProperty.Register(nameof(Content), typeof(object), typeof(PullToRefreshPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty ValueProperty =
  DependencyProperty.Register(nameof(Value), typeof(double), typeof(PullToRefreshPanel), new PropertyMetadata(0d));


        public object PullToRefreshContent
        {
            get { return (object)GetValue(PullToRefreshContentProperty); }
            set { SetValue(PullToRefreshContentProperty, value); }
        }

        public object ReleaseToRefreshContent
        {
            get { return (object)GetValue(ReleaseToRefreshContentProperty); }
            set { SetValue(ReleaseToRefreshContentProperty, value); }
        }

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public class RefreshProgressEventArgs : EventArgs
        {
            public double PullProgress { get; set; }
        }

        /// <summary>
        /// 当请求内容刷新时发生
        /// </summary>
        public event EventHandler RefreshRequested;

        // 定义控件名称，配合模板标识使用以锚定特定元素
        private const string Root = "root";
        private const string Header = "header";
        private const string Contentpresenter = "content";
        private const string ReleaseContent = "releaseContent";
        private const string PullContent = "pullContent";
        private const string ContentTransform = "ContentTransform";
        private const string HeaderTransform = "HeaderTransform";

        // 定义各可视元素及字段初始值
        private Border _root;
        private Grid _header;
        private ContentPresenter _content;
        private CompositeTransform _headerTransform;
        private CompositeTransform _contentTransform;
        private ContentPresenter _pullcontent;
        private ContentPresenter _releasecontent;

        protected override void OnApplyTemplate()
        {
            _content = GetTemplateChild(Contentpresenter) as ContentPresenter;
            _header = GetTemplateChild(Header) as Grid;
            _root = GetTemplateChild(Root) as Border;
            _contentTransform = GetTemplateChild(ContentTransform) as CompositeTransform;
            _headerTransform = GetTemplateChild(HeaderTransform) as CompositeTransform;
            _pullcontent = GetTemplateChild(PullContent) as ContentPresenter;
            _releasecontent = GetTemplateChild(ReleaseContent) as ContentPresenter;

           // _root.ManipulationStarted += _root_ManipulationStarted;
            _root.ManipulationCompleted += _root_ManipulationCompleted;
            _root.ManipulationDelta += _root_ManipulationDelta;
            _root.ManipulationMode = Windows.UI.Xaml.Input.ManipulationModes.TranslateY;

            //CompositionTarget.Rendering += CompositionTarget_Rendering;
            DisplayContent();
            base.OnApplyTemplate();
        }

        //private void _root_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        //{
        //    DisplayContent();
        //    //CompositionTarget.Rendering -= CompositionTarget_Rendering;
        //    //CompositionTarget.Rendering += CompositionTarget_Rendering;
        //}

        //为什么触控无法触发此事件？只有PC上能用这不科学啊
        private void _root_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.Y < 0) return;
            if (Value > 0) return;
            int MaxDrag = 100;
            double trans = Math.Abs(e.Cumulative.Translation.Y / 3);
            double delta = Math.Min(trans, MaxDrag) / MaxDrag;
            double f = delta - 1;
            //Sin（傅里叶）
            double easing = 1 + (f * f * f * (1 - delta));
            double x = easing * 100;
            _contentTransform.TranslateY = x;
            if (x > 50)
            {
                _pullcontent.Visibility = Visibility.Collapsed;
                _releasecontent.Visibility = Visibility.Visible;
            }
            else
            {
                _pullcontent.Visibility = Visibility.Visible;
                _releasecontent.Visibility = Visibility.Collapsed;
            }
            _headerTransform.TranslateY = x;
        }

        private void _root_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            OnCompleted();
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            //if (_scroller.VerticalOffset > 1)
            //{
            //CompositionTarget.Rendering -= CompositionTarget_Rendering;
            //_headerTransform.TranslateY = -_header.ActualHeight;
            //if (_contentTransform != null)
            //{
            //    _contentTransform.TranslateY = 0;
            //}
            //return;
            //}

        }

        /// <summary>
        /// 结束后重置各值
        /// </summary>
        private void OnCompleted()
        {
            //CompositionTarget.Rendering -= CompositionTarget_Rendering;
            double delta = _contentTransform.TranslateY;
            if (delta > 50)
            {
                RefreshRequested?.Invoke(this, new EventArgs());
            }
            DisplayContent();
        }

        private void DisplayContent()
        {
            if (_releasecontent != null) 
            {
                _releasecontent.Visibility = Visibility.Collapsed;
            }
            if (_pullcontent != null)
            {
                _pullcontent.Visibility = Visibility.Collapsed;
            }
            if (_headerTransform != null)
            {
                _headerTransform.TranslateY = -_pullcontent.ActualHeight;
                if (_contentTransform != null)
                {
                    _contentTransform.TranslateY = 0;
                }
            }
        }
    }
}
