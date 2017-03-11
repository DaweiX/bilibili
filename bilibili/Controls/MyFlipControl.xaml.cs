using bilibili.Models;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Data;
using System;

//  The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili.Controls
{
    public sealed partial class MyFlipControl : UserControl
    {
        static double n = 0;// 长宽比
        public delegate void NaviHandler(string arg);
        public event NaviHandler navi;
        List<FlipItem> myList;
        bool isListLoaded;
        private static int count;
        public static int Count
        {
            get { return count; }
            private set { count = value; }
        }
        public MyFlipControl()
        {
            this.InitializeComponent();
        }

       /// <summary>
       /// 初始化
       /// </summary>
       /// <param name="list">要显示的项</param>
       /// <param name="value">单项长宽比</param>
        public void init(List<FlipItem> list, double value = 3.2)
        {
            n = value;
            ChangeSize();
            myList = list;
            Count = list.Count;
            if (count < 3) return;
            else
            {
                AddItems();
                isListLoaded = true;
                center.SelectedIndex = 1;
            }
            // for (int i = 0; i < count; i++)
            // {
            //     round.Children.Add(new Ellipse
            //     {
            //         Name = "r" + i.ToString(),
            //         Width = 10,
            //         Height = 10,
            //         Fill = new SolidColorBrush(Colors.Gray),
            //         Margin = new Thickness(4, 0, 4, 0)
            //     });
            // }
        }
        bool isArrange;

        private void AddItems()
        {
            isArrange = true;
            for (int i = 0; i < myList.Count; i++)
            {
                left.Items.Add(myList[i]);
                right.Items.Add(myList[i]);
                center.Items.Add(myList[i]);
            }
            isArrange = false;
        }

        private void ClearItems()
        {
            isArrange = true;
            left.Items.Clear();
            right.Items.Clear();
            center.Items.Clear();
            isArrange = false;
        }

        private void center_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isListLoaded && !isArrange) 
            {
                int index = center.SelectedIndex;
                left.SelectedIndex = index > 0 ? index - 1 : count - 1;
                right.SelectedIndex = index < count - 1 ? index + 1 : 0;
                if (index == 0)
                {
                    GoBackward();
                }
                if (index == myList.Count - 1)
                {
                    GoForward();
                }
            }
        }

        private void GoBackward()
        {
            FlipItem head = myList[0];
            FlipItem foot = myList[myList.Count - 1];
            myList.Insert(0, foot);
            myList.RemoveAt(myList.Count - 1);
            ClearItems();
            AddItems();
            center.SelectedIndex = 1;
        }

        private void GoForward()
        {
            FlipItem head = myList[0];
            FlipItem foot = myList[myList.Count - 1];
            myList.Insert(myList.Count - 1, head);
            myList.RemoveAt(0);
            ClearItems();
            AddItems();
            center.SelectedIndex = myList.Count - 2;
        }

        private void center_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var temp = center.SelectedItem as FlipItem;
            if (temp != null)
            {
                navi(temp.Link);
            }
        }

        void ChangeSize()
        {
            if (n != 0)
            {
                double width = ActualWidth;
                if (width > 640)
                {
                    Height = width / 2 / n;
                }
                else
                {
                    Height = width / n;
                }
            }       
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeSize();
        }
    }

    public class CVT1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int b = (int)value;
            if (b == -1) return -1;
            return b - 1;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class CVT2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int b = (int)value;
            if (b == -1) return -1;
            return b == MyFlipControl.Count - 1 ? 0 : b + 1;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
