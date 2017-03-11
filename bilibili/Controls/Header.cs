using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//  The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace bilibili.Controls
{
    public sealed class Header : Control
    {
        public Header()
        {
            this.DefaultStyleKey = typeof(Header);
        }
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(Header), new PropertyMetadata(null));
        public static readonly DependencyProperty BorderWidthProperty = DependencyProperty.Register("BorderWidth", typeof(double), typeof(Header), new PropertyMetadata(6));
        public static readonly DependencyProperty BorderHeightProperty = DependencyProperty.Register("BorderHeight", typeof(double), typeof(Header), new PropertyMetadata(28));
        public static readonly DependencyProperty IsClickEnabledProperty = DependencyProperty.Register("IsClickEnabled", typeof(bool), typeof(Header), new PropertyMetadata(false));
        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register("Placement", typeof(placement), typeof(Header), new PropertyMetadata(placement.Left));
       /// <summary>
       /// 题头文字
       /// </summary>
        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
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
       /// <summary>
       /// 是否具有点击逻辑
       /// </summary>
        public bool IsClickEnabled
        {
            get { return (bool)GetValue(IsClickEnabledProperty); }
            set { SetValue(IsClickEnabledProperty, value); }
        }

        public enum placement
        {
            Left,
            Below
        }

       /// <summary>
       /// 侧边栏显示位置
       /// </summary>
        public placement Placement
        {
            get { return (placement)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            placement pl = (placement)GetValue(PlacementProperty);
            if (pl == placement.Left)
            {
                VisualStateManager.GoToState(this, "left", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "below", false);
            }
        }
    }
}
