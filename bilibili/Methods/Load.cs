using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace bilibili.Methods
{
    class Load
    {
        public static T FindChildOfType<T>(DependencyObject root) where T : class
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (var i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    queue.Enqueue(child);
                }
            }
            return null;
        }
    }
    class WidthFit
    {
        /// <summary>
        /// 获取自适应列表项宽度
        /// </summary>
        /// <param name="width">当前窗口宽度</param>
        /// <param name="max">列表项最大宽度</param>
        /// <param name="min">列表项最小宽度</param>
        /// <param name="offset">偏移量</param>
        /// <returns></returns>
        public static double GetWidth(double width, int max, int min, int offset = 16)
        {
            if (offset < 0 || offset > 20)
            {
                offset = 16;
            }
            double w = 1;
            int column = 1;
            width = width - 16;
            double maxcolumn = width / min;
            double mincolumn = width / max;
            for (int i = (int)mincolumn; i < maxcolumn + 1; i++)
            {
                double temp = width / i;
                if (temp < max && temp > min)
                {
                    column = i;
                }
            }
            w = width / column - column * 3;
            return w;
        }
    }
}
