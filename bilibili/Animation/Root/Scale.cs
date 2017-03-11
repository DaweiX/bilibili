using System;
using System.Numerics;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace bilibili.Animation.Root
{
   /// <summary>
   /// 实现缩放动画的核心算法
   /// </summary>
    public static partial class AnimationExtensions
    {
       /// <summary>
       /// 指示是否支持模糊效果（SDK >= 14393）
       /// </summary>
        internal static bool IsBlurSupported => ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3);

        public static AnimationSet Scale(
            this UIElement associatedObject,
            float scaleX = 1f,
            float scaleY = 1f,
            float centerX = 0f,
            float centerY = 0f,
            double duration = 500d,
            double delay = 0d)
        {
            if (associatedObject == null)
            {
                return null;
            }

            var animationSet = new AnimationSet(associatedObject);
            return animationSet.Scale(scaleX, scaleY, centerX, centerY, duration, delay);
        }

        public static AnimationSet Scale(
           this AnimationSet animationSet,
           float scaleX = 1f,
           float scaleY = 1f,
           float centerX = 0f,
           float centerY = 0f,
           double duration = 500d,
           double delay = 0d)
        {
            if (animationSet == null)
            {
                return null;
            }

            if (!AnimationSet.UseComposition)
            {
                var element = animationSet.Element;
                var transform = GetAttachedCompositeTransform(element);

                transform.CenterX = centerX;
                transform.CenterY = centerY;

                var animationX = new DoubleAnimation();
                var animationY = new DoubleAnimation();

                animationX.To = scaleX;
                animationY.To = scaleY;

                animationX.Duration = animationY.Duration = TimeSpan.FromMilliseconds(duration);
                animationX.BeginTime = animationY.BeginTime = TimeSpan.FromMilliseconds(delay);
                animationX.EasingFunction = animationY.EasingFunction = _defaultStoryboardEasingFunction;

                animationSet.AddStoryboardAnimation(GetAnimationPath(transform, element, "ScaleX"), animationX);
                animationSet.AddStoryboardAnimation(GetAnimationPath(transform, element, "ScaleY"), animationY);
            }
            else
            {
                var visual = animationSet.Visual;
                visual.CenterPoint = new Vector3(centerX, centerY, 0);
                var scaleVector = new Vector3(scaleX, scaleY, 1.0f);

                if (duration <= 0)
                {
                    animationSet.AddCompositionDirectPropertyChange("Scale", scaleVector);
                    return animationSet;
                }

                var compositor = visual.Compositor;

                if (compositor == null)
                {
                    return null;
                }

                var animation = compositor.CreateVector3KeyFrameAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(duration);
                animation.DelayTime = TimeSpan.FromMilliseconds(delay);
                animation.InsertKeyFrame(1f, scaleVector);

                animationSet.AddCompositionAnimation("Scale", animation);
            }

            return animationSet;
        }
    }
}
