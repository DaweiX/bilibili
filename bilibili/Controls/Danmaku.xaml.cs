using bilibili.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili.Controls
{
    public sealed partial class Danmaku : UserControl
    {
        public Danmaku()
        {
            this.InitializeComponent();
            speed = SettingHelper.GetValue("_speed") != null ? int.Parse(SettingHelper.GetValue("_speed").ToString()) : 8;
            if (SettingHelper.GetValue("_space") != null)
            {
                int value = int.Parse(SettingHelper.GetValue("_space").ToString());
                ChangeSpace(value);
            }
            else
            {
                ChangeSpace(0);
            }
        }
        int row = 0;
        int speed = 8;
        int fontsize = 21;
        bool isPause = false;
        /// <summary>
        /// 添加滚动弹幕
        /// </summary>
        /// <param name="model"></param>
        public async void AddBasic(DanmuModel model ,bool isMyDanmu)
        {
            TextBlock txt1 = new TextBlock
            {
                Foreground = model.DanColor,
            };
            TextBlock txt2 = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(1),
            };
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                DataContext = model,
            };
            if (isMyDanmu)
            {
                grid.BorderBrush = new SolidColorBrush(Colors.HotPink);
                grid.BorderThickness = new Thickness(2);
            }
            double size = double.Parse(model.Size);
            if (size == 25)
            {
                txt2.FontSize = fontsize;
                txt1.FontSize = fontsize;
            }
            else
            {
                txt2.FontSize = fontsize - 4;
                txt1.FontSize = fontsize - 4;
            }
            txt1.Text = txt2.Text = model.Message;
            grid.Children.Add(txt2);
            grid.Children.Add(txt1);
            TranslateTransform move = new TranslateTransform();
            move.X = grid_0.ActualWidth;
            grid.RenderTransform = move;
            grid_0.Children.Add(grid);
            Grid.SetRow(grid, row);
            row++;
            if (row == 10)
                row = 0;
            grid.DataContext = model;
            grid.UpdateLayout();
            Storyboard sty = new Storyboard();
            DoubleAnimation ani = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(speed)),
                To = -(grid.ActualWidth),
            };
            sty.Children.Add(ani);
            Storyboard.SetTarget(ani, move);
            Storyboard.SetTargetProperty(ani, "X");
            grid.Resources.Remove("justintimeStoryboard");
            grid.Resources.Add("sty", sty);
            sty.Begin();
            await Task.Run(async () =>
            {
                int i = 0;
                while (true)
                {
                    if (isPause)
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            sty.Pause();
                        });
                    }
                    else
                    {
                        if (i == speed * 2)
                        {
                            break;
                        }
                        i++;
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            sty.Resume();
                        });
                    }
                    await Task.Delay(500);
                }
            });
            grid_0.Children.Remove(grid);
        }

        public void IsPauseDanmaku(bool isAction)
        {
            isPause = isAction;
        }

        /// <summary>
        /// 弹幕间距
        /// </summary>
        /// <param name="value">0:单倍行距；1:1.2倍行距;2:1.5倍行距</param>
        public void ChangeSpace(int value)
        {
            double h = 0;
            double actualHeight = r1.ActualHeight;
            switch (value)
            {
                case 0:
                    h = 1.5 * actualHeight;break;
                case 1:
                    h = 1.2 * actualHeight;break;
                case 2:
                    h = actualHeight;break;
            }
            space.Height = new GridLength(h, GridUnitType.Pixel);
        }

        /// <summary>
        /// 弹幕速度
        /// </summary>
        /// <param name="value"></param>
        public void ChangeSpeed(int value)
        {
            speed = value;
        }

        /// <summary>
        /// 弹幕字号
        /// </summary>
        /// <param name="value"></param>
        public void ChangeSize(int value)
        {
            switch(value)
            {
                case 0:fontsize = 14;break;
                case 1: fontsize = 16; break;
                case 2: fontsize = 21; break;
                case 3: fontsize = 25; break;
            }
        }

        /// <summary>
        /// 添加顶部和底部弹幕
        /// </summary>
        /// <param name="model"></param>
        public async void AddTop(DanmuModel model, bool isMyDanmu)
        {
            TextBlock txt1 = new TextBlock
            {
                Foreground = model.DanColor,
            };
            TextBlock txt2 = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(1),
            };
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                DataContext = model,
            };
            if (isMyDanmu)
            {
                grid.BorderBrush = new SolidColorBrush(Colors.HotPink);
                grid.BorderThickness = new Thickness(2);
            }
            double size = double.Parse(model.Size);
            if (size == 25)
            {
                txt2.FontSize = fontsize;
                txt1.FontSize = fontsize;
            }
            else
            {
                txt2.FontSize = fontsize - 4;
                txt1.FontSize = fontsize - 4;
            }
            txt1.Text = txt2.Text = model.Message;
            grid.Children.Add(txt2);
            grid.Children.Add(txt1);
            grid.UpdateLayout();
            if (model.Mode == "5")
            {
                top.Children.Add(grid);
                await Task.Delay(5000);
                if (!isPause)
                {
                    top.Children.Remove(grid);
                }
            }
            else if (model.Mode == "4")
            {
                bottom.Children.Add(grid);
                await Task.Delay(5000);
                if (!isPause)
                {
                    bottom.Children.Remove(grid);
                }
            }
        }

        /// <summary>
        /// 清除弹幕
        /// </summary>
        public void ClearDanmu()
        {
            grid_0.Children.Clear();
            top.Children.Clear();
            bottom.Children.Clear();
        }

        public void ClearGun(bool isvisual)
        {
            grid_0.Visibility = isvisual ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ClearBottom(bool isvisual)
        {
            bottom.Visibility = isvisual ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ClearTop(bool isvisual)
        {
            top.Visibility = isvisual ? Visibility.Visible : Visibility.Collapsed;
        }

        public class DanmuModel
        {
            public string aid { get; set; }
            public string Message { get; set; }
            private decimal time;
            public decimal Time
            {
                get { return time; }
                set { time = value; }
            }
            /// <summary>
            /// 3 滚动弹幕 4底端弹幕 5顶端弹幕 6.逆向弹幕 7精准定位 8高级弹幕
            /// </summary>
            public string Mode { get; set; }
            public string Size { get; set;}
            public string Color { get; set; }
            public SolidColorBrush color { get; set; }
            public SolidColorBrush DanColor
            {
                get
                {
                    try
                    {
                        Color = Convert.ToInt32(Color).ToString("X2");
                        if (Color.StartsWith("#"))
                            Color = Color.Replace("#", string.Empty);
                        int v = int.Parse(Color, System.Globalization.NumberStyles.HexNumber);
                        SolidColorBrush solid = new SolidColorBrush(new Color()
                        {
                            A = Convert.ToByte(255),
                            R = Convert.ToByte((v >> 16) & 255),
                            G = Convert.ToByte((v >> 8) & 255),
                            B = Convert.ToByte((v >> 0) & 255)
                        });
                        color = solid;
                        return solid;
                    }
                    catch (Exception)
                    {
                        SolidColorBrush solid = new SolidColorBrush(new Color()
                        {
                            A = 255,
                            R = 255,
                            G = 255,
                            B = 255
                        });
                        color = solid;
                        return solid;
                    }
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualHeight < 500)
            {
                fontsize = 16;
            }
            else
            {
                fontsize = 21;
            }
        }
    }
}
