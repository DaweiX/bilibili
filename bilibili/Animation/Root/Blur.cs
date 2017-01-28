using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace bilibili.Animation.Root
{
    public static partial class AnimationExtensions
    {
        public static AnimationSet Blur(
    this FrameworkElement associatedObject,
    double value = 0d,
    double duration = 500d,
    double delay = 0d)
        {
            if (associatedObject == null)
            {
                return null;
            }

            var animationSet = new AnimationSet(associatedObject);
            return animationSet.Blur(value, duration, delay);
        }

        public static AnimationSet Blur(
          this AnimationSet animationSet,
          double value = 0d,
          double duration = 500d,
          double delay = 0d)
        {
            if (animationSet == null)
            {
                return null;
            }

            if (!IsBlurSupported)
            {
                // The operating system doesn't support blur.
                // Fail gracefully by not applying blur.
                // See 'IsBlurSupported' property
                return null;
            }

            var visual = animationSet.Visual;
            var associatedObject = animationSet.Element as FrameworkElement;

            if (associatedObject == null)
            {
                return animationSet;
            }

            var compositor = visual?.Compositor;
            const string blurName = "Blur";

            if (compositor == null)
            {
                return null;
            }

            // check to see if the visual already has a blur applied.
            var spriteVisual = ElementCompositionPreview.GetElementChildVisual(associatedObject) as SpriteVisual;
            var blurBrush = spriteVisual?.Brush as CompositionEffectBrush;

            if (blurBrush == null || blurBrush.Comment != blurName)
            {
                var blurEffect = new GaussianBlurEffect
                {
                    Name = blurName,
                    BlurAmount = 0f,
                    Optimization = EffectOptimization.Balanced,
                    BorderMode = EffectBorderMode.Hard,
                    Source = new CompositionEffectSourceParameter("source")
                };

                // Create a brush to which I want to apply. I also have noted that BlurAmount should be left out of the compiled shader.
                blurBrush = compositor.CreateEffectFactory(blurEffect, new[] { $"{blurName}.BlurAmount" }).CreateBrush();
                blurBrush.Comment = blurName;

                // Set the source of the blur as a backdrop brush
                blurBrush.SetSourceParameter("source", compositor.CreateBackdropBrush());

                var blurSprite = compositor.CreateSpriteVisual();
                blurSprite.Brush = blurBrush;
                ElementCompositionPreview.SetElementChildVisual(associatedObject, blurSprite);

                blurSprite.Size = new Vector2((float)associatedObject.ActualWidth, (float)associatedObject.ActualHeight);

                associatedObject.SizeChanged += (s, e) =>
                {
                    blurSprite.Size = new Vector2((float)associatedObject.ActualWidth, (float)associatedObject.ActualHeight);
                };
            }

            if (duration <= 0)
            {
                animationSet.AddEffectDirectPropertyChange(blurBrush, (float)value, $"{blurName}.BlurAmount");
            }
            else
            {
                // Create an animation to change the blur amount over time
                var blurAnimation = compositor.CreateScalarKeyFrameAnimation();
                blurAnimation.InsertKeyFrame(1f, (float)value);
                blurAnimation.Duration = TimeSpan.FromMilliseconds(duration);
                blurAnimation.DelayTime = TimeSpan.FromMilliseconds(delay);

                animationSet.AddCompositionEffectAnimation(blurBrush, blurAnimation, $"{blurName}.BlurAmount");
            }

            if (value == 0)
            {
                animationSet.Completed += AnimationSet_Completed;
            }

            return animationSet;
        }

        private static void AnimationSet_Completed(object sender, EventArgs e)
        {
            var animationSet = sender as AnimationSet;
            animationSet.Completed -= AnimationSet_Completed;

            var spriteVisual = ElementCompositionPreview.GetElementChildVisual(animationSet.Element) as SpriteVisual;
            var blurBrush = spriteVisual?.Brush as CompositionEffectBrush;

            if (blurBrush != null && blurBrush.Comment == "Blur")
            {
                spriteVisual.Brush = null;
            }
        }

    }
}
