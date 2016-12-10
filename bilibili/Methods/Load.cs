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
        public static double GetWidth(double width, int max, int min, int offset = 12)
        {
            if (offset < 0 || offset > 24)
            {
                offset = 12;
            }
            double w = 1;
            int column = 1;
            int maxcolumn = (int)width / min;
            double i2 = width / min;
            for (int i = 1; i <= maxcolumn; i++)
            {
                if (Math.Abs(i - i2) < 1) 
                {
                    column = (int)Math.Truncate(i2) == 0 ? 1 : (int)Math.Truncate(i2);
                }
            }
            w = width / column;
            w -= offset;
            //double rate = w * (double)column / width;
            //if (rate < 0.99)
            //{
            //    column++;
            //    w = width / column;
            //    w -= offset;
            //}
            return w;
        }
    }
}
