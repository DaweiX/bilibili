using bilibili.Animation.Root;
using System;
using Windows.UI.Xaml;

namespace bilibili.Animation.Effects
{
    public class BlurEffects : CompositionBehaviorBase
    {
        private FrameworkElement _frameworkElement;

        /// <summary>
        /// 模糊值
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(BlurEffects), new PropertyMetadata(0d, PropertyChangedCallback));

        /// <summary>
        /// 在效果联结在<see cref="Microsoft.Xaml.Interactivity.Behavior.AssociatedObject"/>之前调用
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            _frameworkElement = AssociatedObject as FrameworkElement;
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public override void StartAnimation()
        {
            if (AnimationExtensions.IsBlurSupported)
            {
                _frameworkElement?.Blur(duration: Duration, delay: Delay, value: (float)Value)?.StartAsync();
            }
        }
    }
}
