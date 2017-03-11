using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;

namespace bilibili.Animation
{
   /// <summary>
   /// 使用复合器的所有动画的通用基类，包含要设置在可视元素的动画上的通用属性
   /// </summary>
    public abstract class CompositionBehaviorBase : Behavior<UIElement>
    {
       /// <summary>
       /// Called after the behavior is attached to the <see cref="P:Microsoft.Xaml.Interactivity.Behavior.AssociatedObject" />.
       /// </summary>
       /// <remarks>
       /// Override this to hook up functionality to the <see cref="P:Microsoft.Xaml.Interactivity.Behavior.AssociatedObject" />
       /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            var frameworkElement = AssociatedObject as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.Loaded += OnFrameworkElementLoaded;
            }
        }

       /// <summary>
       /// Called while the behavior is detaching from the <see cref="P:Microsoft.Xaml.Interactivity.Behavior.AssociatedObject" />.
       /// </summary>
       /// <remarks>
       /// Override this to finalize and free everything associated to the <see cref="P:Microsoft.Xaml.Interactivity.Behavior.AssociatedObject" />
       /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            var frameworkElement = AssociatedObject as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.Loaded -= OnFrameworkElementLoaded;
            }
        }

        private void OnFrameworkElementLoaded(object sender, RoutedEventArgs e)
        {
            if (AutomaticallyStart)
            {
                StartAnimation();
            }
        }

       /// <summary>
       /// 持续时间
       /// </summary>
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(double), typeof(CompositionBehaviorBase), new PropertyMetadata(1d, PropertyChangedCallback));

       /// <summary>
       /// 延时
       /// </summary>
        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(nameof(Delay), typeof(double), typeof(CompositionBehaviorBase), new PropertyMetadata(0d, PropertyChangedCallback));

       /// <summary>
       /// 自动开始
       /// </summary>
        public static readonly DependencyProperty AutomaticallyStartProperty = DependencyProperty.Register(nameof(AutomaticallyStart), typeof(bool), typeof(CompositionBehaviorBase), new PropertyMetadata(true, PropertyChangedCallback));

       /// <summary>
       /// 是否自动开始动画
       /// </summary>
        public bool AutomaticallyStart
        {
            get { return (bool)GetValue(AutomaticallyStartProperty); }
            set { SetValue(AutomaticallyStartProperty, value); }
        }

       /// <summary>
       /// 延时
       /// </summary>
        public double Delay
        {
            get { return (double)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

       /// <summary>
       /// 持续时间
       /// </summary>
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

       /// <summary>
       /// 开始动画
       /// </summary>
        public abstract void StartAnimation();

       /// <summary>
       /// If any of the properties are changed then the animation is automatically started depending on the AutomaticallyStart property.
       /// </summary>
       /// <param name="dependencyObject">The dependency object.</param>
       /// <param name="dependencyPropertyChangedEventArgs">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        protected static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as CompositionBehaviorBase;

            if (behavior?.AutomaticallyStart ?? false)
            {
                behavior.StartAnimation();
            }
        }
    }
}
