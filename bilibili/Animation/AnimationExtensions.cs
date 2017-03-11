using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace bilibili.Animation.Root
{
   /// <summary>
   /// 这些拓展方法将动画展现于可视元素上
   /// </summary>
    public static partial class AnimationExtensions
    {
       /// <summary>
       /// 默认缓动函数：三次函数
       /// </summary>
        private static readonly EasingFunctionBase _defaultStoryboardEasingFunction = new CubicEase();

       /// <summary>
       /// 开始一个时间线
       /// </summary>
        public static Task BeginAsync(this Storyboard storyboard)
        {
            // this:此方法为拓展方法
            var taskSource = new TaskCompletionSource<object>();
            EventHandler<object> completed = null;
            completed += (s, e) =>
            {
                storyboard.Completed -= completed;
                taskSource.SetResult(null);
            };
            storyboard.Completed += completed;
            storyboard.Begin();
            return taskSource.Task;
        }
        public static string GetAnimationPath(CompositeTransform transform, UIElement element, string property)
        {
            if (element.RenderTransform == transform)
            {
                return "(UIElement.RenderTransform).(CompositeTransform.Rotation)";
            }

            var group = element.RenderTransform as TransformGroup;

            if (group == null)
            {
                return string.Empty;
            }

            for (var index = 0; index < group.Children.Count; index++)
            {
                if (group.Children[index] == transform)
                {
                    return $"(UIElement.RenderTransform).(TransformGroup.Children)[{index}].(CompositeTransform.{property})";
                }
            }

            return string.Empty;
        }
        private static CompositeTransform GetAttachedCompositeTransform(UIElement element)
        {
            //  We need to use an index to keep track of our CompositeTransform as animation engine
            //  recreates new transform objects when animating properties

            //  Already attached?
            var compositeTransformIndex = AnimationTools.GetAnimationCompositeTransformIndex(element);

            if (compositeTransformIndex > -2)
            {
                if (compositeTransformIndex == -1 && element.RenderTransform is CompositeTransform)
                {
                    return (CompositeTransform)element.RenderTransform;
                }

                var group = element.RenderTransform as TransformGroup;

                if (group?.Children.Count > compositeTransformIndex && group.Children[compositeTransformIndex] is CompositeTransform)
                {
                    return (CompositeTransform)group.Children[compositeTransformIndex];
                }
            }

            //  Let's create a new CompositeTransform
            var result = new CompositeTransform();

            var currentTransform = element.RenderTransform;

            if (currentTransform != null)
            {
                //  We found a RenderTransform

                //  Is it a TransformGroup?
                var currentTransformGroup = currentTransform as TransformGroup;

                if (currentTransformGroup != null)
                {
                    currentTransformGroup.Children.Add(result);

                    AnimationTools.SetAnimationCompositeTransformIndex(element, currentTransformGroup.Children.Count - 1);
                }
                else
                {
                    //  Let's create our own TransformGroup
                    var group = new TransformGroup();
                    group.Children.Add(currentTransform);
                    group.Children.Add(result);
                    element.RenderTransform = group;

                    AnimationTools.SetAnimationCompositeTransformIndex(element, 1);
                }
            }
            else
            {
                element.RenderTransform = result;

                AnimationTools.SetAnimationCompositeTransformIndex(element, -1);
            }

            return result;
        }
    }
}
