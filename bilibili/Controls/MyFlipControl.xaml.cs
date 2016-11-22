using bilibili.Models;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili.Controls
{
    public sealed partial class MyFlipControl : UserControl
    {
        public delegate void NaviHandler(string arg);
        public event NaviHandler navi;
        List<FlipItem> myList = null;
        bool isListLoaded = false;
        int count = 0;
        public MyFlipControl()
        {
            this.InitializeComponent();
        }

        public void init(List<FlipItem> list)
        {
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
            navi((center.SelectedItem as FlipItem).Link);
        }
    }
}
