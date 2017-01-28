using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace bilibili.Controls
{
    public sealed class Area : Control
    {
        public Area()
        {
            this.DefaultStyleKey = typeof(Area);
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(Area), new PropertyMetadata(null));
        public static readonly DependencyProperty BorderWidthProperty = DependencyProperty.Register("BorderWidth", typeof(double), typeof(Area), new PropertyMetadata(6));
        public static readonly DependencyProperty BorderHeightProperty = DependencyProperty.Register("BorderHeight", typeof(double), typeof(Area), new PropertyMetadata(28));
        public static readonly DependencyProperty HeaderClickEventProperty = DependencyProperty.Register("HeaderClickEvent", typeof(RoutedEventHandler), typeof(Area), new PropertyMetadata(null));
        /// <summary>
        /// 题头文字
        /// </summary>
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        /// <summary>
        /// 侧边宽度
        /// </summary>
        public double BorderWidth
        {
            get { return (double)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }
        /// <summary>
        /// 侧边高度
        /// </summary>
        public double BorderHeight
        {
            get { return (double)GetValue(BorderHeightProperty); }
            set { SetValue(BorderHeightProperty, value); }
        }

        public event OnClick HeaderClick
        {
            add
            {
                (FindName("h1") as HyperlinkButton).Click += HeaderClickEvent;
            }
            remove
            {
                (FindName("h1") as HyperlinkButton).Click -= HeaderClickEvent;
            }
        }

        public delegate RoutedEventHandler OnClick(object sender, RoutedEventArgs e);

        public RoutedEventHandler HeaderClickEvent
        {
            get { return (RoutedEventHandler)GetValue(HeaderClickEventProperty); }
            set { SetValue(HeaderClickEventProperty, value); }
        }
    }
}
