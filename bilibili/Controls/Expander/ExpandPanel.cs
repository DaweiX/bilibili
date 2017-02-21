using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace bilibili.Controls
{
    [ContentProperty(Name = "Content")]
    public sealed class ExpandPanel : Control
    {
        private ToggleButton button;
        public ExpandPanel()
        {
            this.DefaultStyleKey = typeof(ExpandPanel);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            button = GetTemplateChild("btn") as ToggleButton;
            button.Loaded += (s, e) => { SwitchStatus(false); };
            button.Checked += (s, e) => { SwitchStatus(); };
            button.Unchecked += (s, e) => { SwitchStatus(); };
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(ExpandPanel), new PropertyMetadata(null));
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(ExpandPanel), new PropertyMetadata(false));
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        private void SwitchStatus(bool useTransition = true)
        {
            if (button.IsChecked.Value)
            {
                VisualStateManager.GoToState(this, "Open", useTransition);
            }
            else
            {
                VisualStateManager.GoToState(this, "Close", useTransition);
            }
        }
    }
    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
