using bilibili.Helpers;
using bilibili.Http;
using bilibili.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;

//  The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili.Controls
{
    public sealed partial class Comment : UserControl
    {
        int size;
        public delegate void MyHandler(string tid);
        public event MyHandler Navi;
        public event MyHandler Info;
        public Comment()
        {
            this.InitializeComponent();
        }

        public async Task init()
        {
            Sort();
            panel.Children.Clear();
            DeviceType type = SettingHelper.DeviceType;
            size = type == DeviceType.Mobile ? 3 : 6;
            for (int i = 0; i < list.Count; i++)
            {
                panel.Children.Add(GetPanel(list[i]));
            }
            Button btn = new Button
            {
                Content = "调整栏目排序",
                Margin = new Thickness(0, 8, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            btn.Click += async (s, e) => 
            {
                var dialog = new Dialogs.Sort();
                dialog.action += async () => { await init(); };
                await dialog.ShowAsync();
            };
            panel.Children.Add(btn);
            for (int i = 0; i < list.Count; i++)
            {
                await LoadItems(int.Parse(list[i].Value), 1);
            }
        }

        private async Task LoadItems(int tid, int page)
        {
            GridView gridview = FindName("gridview" + tid) as GridView;
            if (gridview != null)
            {
                List<Content> a = await ContentServ.GetContentAsync(tid, page, size);
                if (a != null)
                {
                    gridview.ItemsSource = a;
                }
            }
        }

        private async void Fresh_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;
            GridView gridview = FindName("gridview" + btn.Tag.ToString()) as GridView;
            await LoadItems(int.Parse(gridview.Tag.ToString()),new Random().Next(1,50));
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;
            Navi(btn.Content.ToString());
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (size == 3)
            {
                width.Width = ActualWidth / 3 - 8;
                return;
            }
            double i = this.ActualWidth;
            if (i > 800)
            {
                i = 6;
            }
            else if (i > 600)
            {
                i = 4;
            }
            else
            {
                i = 3;
            }
            width.Width = this.ActualWidth / i;
        }
       
        private void gridview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Info((e.ClickedItem as Content).Num);
        }

        public void Sort()
        {
            string defaultSort = "番剧_13.动画_1.生活_160.电影_23.娱乐_71.鬼畜_119.科技_36.游戏_4.音乐_3.舞蹈_20.时尚_155.广告_166.电视剧_11";
            string sort = defaultSort;
            if (SettingHelper.ContainsKey("_sort"))
            {
                string _sort = SettingHelper.GetValue("_sort").ToString();
                if (_sort != string.Empty)
                {
                    sort = _sort;
                }
            }
            else
            {
                SettingHelper.SetValue("_sort", defaultSort);
            }
            list.Clear();
            foreach (var item in sort.Split('.'))
            {
                string key = item.Split('_')[0];
                string value = item.Split('_')[1];
                list.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

        private StackPanel GetPanel(KeyValuePair<string, string> args)
        {
            StackPanel panel = new StackPanel();
            Grid grid = new Grid();
            HyperlinkButton btn1 = new HyperlinkButton
            {
                Content = args.Key,
                Margin = new Thickness(4),
            };
            btn1.Click += HyperlinkButton_Click;
            HyperlinkButton btn2 = new HyperlinkButton
            {
                Tag = args.Value,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5, 0, 16, 0),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
            };
            btn2.Click += Fresh_Click;
            Grid grid2 = new Grid();
            Rectangle rec = new Rectangle { Fill = new SolidColorBrush(Colors.Transparent) };
            SymbolIcon symbol = new SymbolIcon
            {
                Symbol = Symbol.Refresh,
                Foreground = Application.Current.Resources["bili_Theme"] as SolidColorBrush,
            };
            grid2.Children.Add(rec);
            grid2.Children.Add(symbol);
            btn2.Content = grid2;
            grid.Children.Add(btn1);
            grid.Children.Add(btn2);

            GridView gridview = new GridView
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = args.Value,
                Name = "gridview" + args.Value,
                IsItemClickEnabled = true,
                SelectionMode = ListViewSelectionMode.None,
                ItemTemplate = Resources["gv_itemtemplete"] as DataTemplate,
                Style = Resources["GridViewStyle1"] as Style
            };
            gridview.ItemClick += gridview_ItemClick;
            panel.Children.Add(grid);
            panel.Children.Add(gridview);
            return panel;
        }
    }
}
