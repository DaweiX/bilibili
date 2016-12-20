using bilibili.Models;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili.Controls
{
    public sealed partial class MyFlipControl : UserControl
    {
        static double n = 0;//长宽比
        public delegate void NaviHandler(string arg);
        public event NaviHandler navi;
        List<FlipItem> myList = null;
        bool isListLoaded = false;
        int count = 0;
        public MyFlipControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list">要显示的项</param>
        /// <param name="value">单项长宽比</param>
        public void init(List<FlipItem> list, double value)
        {
            n = value;
            ChangeSize();
            myList = list;
            count = list.Count;
            if (count < 3) return;
            else
            {
                left.ItemsSource = center.ItemsSource = right.ItemsSource = myList;
                isListLoaded = true;
                left.SelectedIndex = 0;
                center.SelectedIndex = 1;
                right.SelectedIndex = 2;
            }
        }

        private void center_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(isListLoaded)
            {
                int index = center.SelectedIndex;
                if (index > 0)
                {
                    left.SelectedIndex = index - 1;
                }
                else
                {
                    left.SelectedIndex = count - 1;
                }
                if (index < count - 1)
                {
                    right.SelectedIndex = index + 1;
                }
                else
                {
                    right.SelectedIndex = 0;
                }
            }          
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
}
