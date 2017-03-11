using bilibili.Animation.Root;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;

namespace bilibili.Animation
{
   /// <summary>
   /// 定义一个用于存储和管理元素动画复合集的对象
   /// </summary>
    public class AnimationSet : IDisposable
    {
        private Dictionary<string, CompositionAnimation> _animations;
        private List<EffectAnimationDefinition> _effectAnimations;
        private Dictionary<string, object> _directPropertyChanges;
        private List<EffectDirectPropertyChangeDefinition> _directEffectPropertyChanges;
        private List<AnimationSet> _animationSets;
        private Storyboard _storyboard;
        private Dictionary<string, Timeline> _storyboardAnimations;
       /// <summary>
       /// 管理应用程序和系统复合器进程之间的会话
       /// </summary>
        private Compositor _compositor;
       /// <summary>
       /// 动态动画或效果，当所有成员都完成时触发
       /// </summary>
        private CompositionScopedBatch _batch;
       /// <summary>
       /// 通知一个或多个等待的线程已发生事件
       /// </summary>
        private ManualResetEvent _manualResetEvent;
        public static bool UseComposition { get; set; }
        public Visual Visual { get; private set; }
        public UIElement Element { get; private set; }
        public AnimationSet(UIElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException("应用动画前，UI对象不能为空");
            }
            Visual visual = ElementCompositionPreview.GetElementVisual(element);
            if (visual == null)
            {
                throw new NullReferenceException("应用动画前，可视元素不能为空");
            }
            Visual = visual;
            if(Visual.Compositor == null)
            {
                throw new NullReferenceException("应用动画前遇到无效的复合器");
            }
            Element = element;
            _compositor = Visual.Compositor;
            _animations = new Dictionary<string, CompositionAnimation>();
            _effectAnimations = new List<EffectAnimationDefinition>();
            _manualResetEvent = new ManualResetEvent(false);
            _directPropertyChanges = new Dictionary<string, object>();
            _directEffectPropertyChanges = new List<EffectDirectPropertyChangeDefinition>();
            _animationSets = new List<AnimationSet>();
            _storyboard = new Storyboard();
            _storyboardAnimations = new Dictionary<string, Timeline>();
        }

       /// <summary>
       /// 当所有动画完成时发生
       /// </summary>
        public event EventHandler Completed;

       /// <summary>
       /// 开始所有动画（无需等待）
       /// </summary>
        public async void Start()
        {
            await StartAsync();
        }

       /// <summary>
       /// 开始所有动画，并返回一个可等待的<see cref="Task"/>
       /// </summary>
        public async Task StartAsync()
        {
            foreach (var set in _animationSets)
            {
                await set.StartAsync();
            }
            if (_batch != null)
            {
                if (!_batch.IsEnded)
                {
                    _batch.End();
                }
                _batch.Completed -= Batch_Completed;
            }
            foreach (var property in _directPropertyChanges)
            {
                typeof(Visual).GetProperty(property.Key).SetValue(Visual, property.Value);
            }
            foreach (var definition in _directEffectPropertyChanges)
            {
                definition.EffectBrush.Properties.InsertScalar(definition.PropertyName, definition.Value);
            }
            List<Task> tasks = new List<Task>();
            if (_animations.Count > 0 || _effectAnimations.Count > 0)
            {
                _batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                _batch.Completed += Batch_Completed;

                foreach (var anim in _animations)
                {
                    Visual.StartAnimation(anim.Key, anim.Value);
                }

                foreach (var effect in _effectAnimations)
                {
                    effect.EffectBrush.StartAnimation(effect.PropertyName, effect.Animation);
                }

                Task compositionTask = Task.Run(() =>
                {
                    _manualResetEvent.Reset();
                    _manualResetEvent.WaitOne();
                });

                _batch.End();

                tasks.Add(compositionTask);
            }

            tasks.Add(_storyboard.BeginAsync());
            await Task.WhenAll(tasks);
            Completed?.Invoke(this, new EventArgs());
        }

       /// <summary>
       /// 在运行新动画前等待现有动画完成
       /// </summary>
        public AnimationSet Then()
        {
            // 保存现有动画复合集
            var savedAnimationSet = new AnimationSet(Element);
            savedAnimationSet._animations = _animations;
            savedAnimationSet._effectAnimations = _effectAnimations;
            savedAnimationSet._directPropertyChanges = _directPropertyChanges;
            savedAnimationSet._directEffectPropertyChanges = _directEffectPropertyChanges;
            savedAnimationSet._storyboard = _storyboard;
            savedAnimationSet._storyboardAnimations = _storyboardAnimations;
            _animationSets.Add(savedAnimationSet);
            // 将现有动画复合集重置
            _animations = new Dictionary<string, CompositionAnimation>();
            _effectAnimations = new List<EffectAnimationDefinition>();
            _directPropertyChanges = new Dictionary<string, object>();
            _directEffectPropertyChanges = new List<EffectDirectPropertyChangeDefinition>();
            _storyboard = new Storyboard();
            _storyboardAnimations = new Dictionary<string, Timeline>();
            return this;
        }

       /// <summary>
       /// 设定所有动画的持续时间（毫秒）
       /// </summary>
        public AnimationSet SetDuration(double duration)
        {
            if (duration <= 0) duration = 1;
            return SetDuration(TimeSpan.FromMilliseconds(duration));
        }

        public AnimationSet SetDuration(TimeSpan duration)
        {
            foreach (var anim in _animations)
            {
                var animation = anim.Value as KeyFrameAnimation;
                if (animation != null)
                {
                    animation.Duration = duration;
                }
            }
            foreach (var effect in _effectAnimations)
            {
                var animation = effect.Animation as KeyFrameAnimation;
                if (animation != null)
                {
                    animation.Duration = duration;
                }
            }
            foreach (var timeline in _storyboardAnimations)
            {
                var animation = timeline.Value as DoubleAnimation;
                if (animation != null)
                {
                    animation.Duration = duration;
                }
            }
            return this;
        }

       /// <summary>
       /// 设定所有动画的延时时间（毫秒）
       /// </summary>
        public AnimationSet SetDelay(double delayTime)
        {
            if (delayTime < 0)
            {
                delayTime = 0;
            }

            return SetDelay(TimeSpan.FromMilliseconds(delayTime));
        }

        public AnimationSet SetDelay(TimeSpan delayTime)
        {
            foreach (var anim in _animations)
            {
                var animation = anim.Value as KeyFrameAnimation;
                if (animation != null)
                {
                    animation.DelayTime = delayTime;
                }
            }
            foreach (var effect in _effectAnimations)
            {
                var animation = effect.Animation as KeyFrameAnimation;
                if (animation != null)
                {
                    animation.DelayTime = delayTime;
                }
            }
            foreach (var timeline in _storyboardAnimations)
            {
                var animation = timeline.Value as DoubleAnimation;
                if (animation != null)
                {
                    animation.BeginTime = delayTime;
                }
            }
            return this;
        }

       /// <summary>
       /// 添加一个要运行于<see cref="StartAsync"/>的复合动画
       /// </summary>
        public void AddCompositionAnimation(string propertyName, CompositionAnimation animation)
        {
            _animations[propertyName] = animation;
        }

       /// <summary>
       /// 移除一个将要作用于<see cref="Visual"/> 的复合动画
       /// </summary>
        public void RemoveCompositionAnimation(string propertyName)
        {
            if (_animations.ContainsKey(propertyName))
            {
                _animations.Remove(propertyName);
            }
        }

       /// <summary>
       /// 添加一个要即刻变化的属性
       /// </summary>
        public void AddCompositionDirectPropertyChange(string propertyName, object value)
        {
            _directPropertyChanges[propertyName] = value;
        }

        private void Batch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            _manualResetEvent.Set();
        }

       /// <summary>
       /// 添加一个要运行的时间线动画
       /// </summary>
       /// <param name="propertyPath">在故事板上执行动画的属性</param>
       /// <param name="timeline">要添加到故事板的时间线</param>
        public void AddStoryboardAnimation(string propertyPath, Timeline timeline)
        {
            if (_storyboardAnimations.ContainsKey(propertyPath))
            {
                var previousAnimation = _storyboardAnimations[propertyPath];
                _storyboard.Children.Remove(previousAnimation);
                _storyboardAnimations.Remove(propertyPath);
            }
            _storyboardAnimations.Add(propertyPath, timeline);
            _storyboard.Children.Add(timeline);
            Storyboard.SetTarget(timeline, Element);
            Storyboard.SetTargetProperty(timeline, propertyPath);
        }

       /// <summary>
       /// 释放资源
       /// </summary>
        public void Dispose()
        {
            _manualResetEvent?.Dispose();
        }

        public void AddCompositionEffectAnimation(CompositionEffectBrush effectBrush, CompositionAnimation animation, string propertyName)
        {
            var effect = new EffectAnimationDefinition()
            {
                EffectBrush = effectBrush,
                Animation = animation,
                PropertyName = propertyName
            };

            _effectAnimations.Add(effect);
        }

        internal void AddEffectDirectPropertyChange(CompositionEffectBrush effectBrush, float value, string propertyName)
        {
            var definition = new EffectDirectPropertyChangeDefinition()
            {
                EffectBrush = effectBrush,
                Value = value,
                PropertyName = propertyName
            };

            _directEffectPropertyChanges.Add(definition);
        }
    }
   /// <summary>
   /// 用于将动画效果链接至可视元素的类，用于<see cref="AnimationSet"/>类
   /// </summary>
    class EffectAnimationDefinition
    {
       /// <summary>
       /// 画刷
       /// </summary>
        public CompositionEffectBrush EffectBrush { get; set; }
       /// <summary>
       /// 动画
       /// </summary>
        public CompositionAnimation Animation { get; set; }
       /// <summary>
       /// 属性名
       /// </summary>
        public string PropertyName { get; set; }
    }

    class EffectDirectPropertyChangeDefinition
    {
        public CompositionEffectBrush EffectBrush { get; set; }
        public float Value { get; set; }
        public string PropertyName { get; set; }
    }
}
