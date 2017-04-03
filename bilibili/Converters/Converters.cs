using System;
using bilibili.Helpers;
using bilibili.Methods;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace bilibili.Converters
{
    public class NumToForeground_Dialog : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = int.Parse(value.ToString());
            return b > 0 ? "Green" : "Red";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class NumToForeground_Rank : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = int.Parse(value.ToString());
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = b < 4 ? ColorRelated.GetColor() : SettingHelper.GetValue("_nighttheme").ToString() == "light" ? Colors.Black : Colors.White;
            return brush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class BoolToVisibility : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class BoolToForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            SolidColorBrush brush = new SolidColorBrush();
            if (b)
            {
                brush.Color = ColorRelated.GetColor();
                return brush;
            }
            else
            {
                return Application.Current.Resources["SystemControlForegroundBaseHighBrush"];
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class StatusToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Application.Current.Resources["SystemControlForegroundBaseHighBrush"];
            return int.Parse(value.ToString()) == 0 ? "#e273a9" : "{ThemeResource bili_Fontcolor_Main}";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class BoolToOrientation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Orientation.Horizontal : Orientation.Vertical;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class BoolToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Symbol.Play : Symbol.Pause;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class PriorityToThickness : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value
                ? new Thickness(1)
                : new Thickness(0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
