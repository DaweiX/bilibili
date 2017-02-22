using bilibili.Http;
using bilibili.Methods;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace bilibili.Views.PartViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Part : Page
    {
        byte mode;
        bool isLoading = false;
        List<KeyValuePair<string, string>> KeyStringList = new List<KeyValuePair<string, string>>();
        public Part()
        {
            this.InitializeComponent();
            ContentServ.report += Report;
        }

        private async void Report(string status)
        {
            await popup.Show(status);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mode = (byte)e.Parameter;
            switch (mode)
            {
                case 0:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("广告", "166"));
                    }
                    break;
                case 1:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("完结动画", "32"));
                        KeyStringList.Add(new KeyValuePair<string, string>("连载动画", "33"));
                        KeyStringList.Add(new KeyValuePair<string, string>("资讯", "51"));
                        KeyStringList.Add(new KeyValuePair<string, string>("官方延伸", "152"));
                        KeyStringList.Add(new KeyValuePair<string, string>("国产动画", "153"));
                    }
                    break;
                case 2:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("精彩推荐", "27"));
                        KeyStringList.Add(new KeyValuePair<string, string>("MAD·AMV", "24"));
                        KeyStringList.Add(new KeyValuePair<string, string>("MMD·3D", "25"));
                        KeyStringList.Add(new KeyValuePair<string, string>("短片·手书·配音", "47"));
                    }
                    break;
                case 3:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("宅舞", "20"));
                        KeyStringList.Add(new KeyValuePair<string, string>("三次元舞蹈", "154"));
                        KeyStringList.Add(new KeyValuePair<string, string>("舞蹈教程", "156"));
                    }
                    break;
                case 4:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("精彩推荐", "71"));
                        KeyStringList.Add(new KeyValuePair<string, string>("明星", "137"));
                        KeyStringList.Add(new KeyValuePair<string, string>("Korea相关", "131"));
                    }
                    break;
                case 5:
                    {
                        //http://www.bilibili.com/index/ding/155.json?rnd=8008
                        KeyStringList.Add(new KeyValuePair<string, string>("精彩推荐", "155"));
                        KeyStringList.Add(new KeyValuePair<string, string>("美妆", "157"));
                        KeyStringList.Add(new KeyValuePair<string, string>("服饰", "158"));
                        KeyStringList.Add(new KeyValuePair<string, string>("健身", "164"));
                        KeyStringList.Add(new KeyValuePair<string, string>("资讯", "159"));
                    }
                    break;
                case 6:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("精彩推荐", "4"));
                        KeyStringList.Add(new KeyValuePair<string, string>("单机联机", "17"));
                        KeyStringList.Add(new KeyValuePair<string, string>("网游·电竞", "65"));
                        KeyStringList.Add(new KeyValuePair<string, string>("音游", "136"));
                        KeyStringList.Add(new KeyValuePair<string, string>("Mugen", "19"));
                        KeyStringList.Add(new KeyValuePair<string, string>("GMV", "121"));
                    }
                    break;
                case 7:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("精彩推荐", "119"));
                        KeyStringList.Add(new KeyValuePair<string, string>("鬼畜调教", "22"));
                        KeyStringList.Add(new KeyValuePair<string, string>("音MAD", "26"));
                        KeyStringList.Add(new KeyValuePair<string, string>("人力VOVALOID", "126"));
                        KeyStringList.Add(new KeyValuePair<string, string>("教程演示", "127"));
                    }
                    break;
                case 8:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("搞笑", "138"));
                        KeyStringList.Add(new KeyValuePair<string, string>("日常", "21"));
                        KeyStringList.Add(new KeyValuePair<string, string>("美食圈", "76"));
                        KeyStringList.Add(new KeyValuePair<string, string>("动物圈", "75"));
                        KeyStringList.Add(new KeyValuePair<string, string>("手工", "161"));
                        KeyStringList.Add(new KeyValuePair<string, string>("绘画", "162"));
                        KeyStringList.Add(new KeyValuePair<string, string>("运动", "163"));
                    }
                    break;
                case 9:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("精彩推荐", "23"));
                        KeyStringList.Add(new KeyValuePair<string, string>("短片", "85"));
                        KeyStringList.Add(new KeyValuePair<string, string>("欧美电影", "145"));
                        KeyStringList.Add(new KeyValuePair<string, string>("日本电影", "146"));
                        KeyStringList.Add(new KeyValuePair<string, string>("国产电影", "147"));
                        KeyStringList.Add(new KeyValuePair<string, string>("其他国家", "83"));
                    }
                    break;
                case 10:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("精彩推荐", "3"));
                        KeyStringList.Add(new KeyValuePair<string, string>("原创音乐", "28"));
                        KeyStringList.Add(new KeyValuePair<string, string>("翻唱", "31"));
                        KeyStringList.Add(new KeyValuePair<string, string>("VOVALOID·UTAU", "30"));
                        KeyStringList.Add(new KeyValuePair<string, string>("演奏", "59"));
                        KeyStringList.Add(new KeyValuePair<string, string>("三次元音乐", "29"));
                        KeyStringList.Add(new KeyValuePair<string, string>("OP·ED·OST", "54"));
                        KeyStringList.Add(new KeyValuePair<string, string>("音乐选集", "130"));
                    }
                    break;
                case 11:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("连载剧集", "15"));
                        KeyStringList.Add(new KeyValuePair<string, string>("完结剧集", "34"));
                        KeyStringList.Add(new KeyValuePair<string, string>("特摄·布袋戏", "86"));
                        KeyStringList.Add(new KeyValuePair<string, string>("电视剧相关", "128"));
                    }
                    break;
                case 12:
                    {
                        KeyStringList.Add(new KeyValuePair<string, string>("精彩推荐", "36"));
                        KeyStringList.Add(new KeyValuePair<string, string>("纪录片", "37"));
                        KeyStringList.Add(new KeyValuePair<string, string>("趣味科普人文", "124"));
                        KeyStringList.Add(new KeyValuePair<string, string>("野生技术协会", "122"));
                        KeyStringList.Add(new KeyValuePair<string, string>("演讲·公开课", "39"));
                        KeyStringList.Add(new KeyValuePair<string, string>("星海", "96"));
                        KeyStringList.Add(new KeyValuePair<string, string>("数码", "95"));
                        KeyStringList.Add(new KeyValuePair<string, string>("机械", "98"));
                    }
                    break;
            }
            pivot.ItemsSource = GetPivotItems(KeyStringList);
        }

        private void gridview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail_P), (e.ClickedItem as Models.Video).Aid, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void gridview_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            GridView gridview = sender as GridView;
            var scroll = Load.FindChildOfType<ScrollViewer>(gridview);
            var text = Load.FindChildOfType<TextBlock>(gridview);
            scroll.ViewChanged += async (s, a) =>
            {
                if ((scroll.VerticalOffset >= scroll.ScrollableHeight - 50 || scroll.ScrollableHeight == 0) && !isLoading)
                {
                    text.Visibility = Visibility.Visible;
                    int count0 = gridview.Items.Count;
                    int page = gridview.Items.Count / 20 + 1;
                    isLoading = true;
                    List<Models.Video> temps = await ContentServ.GetVideosAsync(int.Parse(gridview.Tag.ToString()), page);
                    if (temps == null) return;
                    if (temps.Count < 20) 
                    {
                        gridview.ContainerContentChanging -= gridview_ContainerContentChanging;
                        text.Text = "装填完毕！";
                        return;
                    }
                    text.Visibility = Visibility.Collapsed;
                    foreach (var item in temps)
                    {
                        gridview.Items.Add(item);
                    }
                    isLoading = false;
                }
            };
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width.Width = WidthFit.GetWidth(ActualWidth);
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = pivot.SelectedIndex;
            GridView gridview = FindName("gridview" + index.ToString()) as GridView;
            if (gridview.Items.Count == 0)
            {
                var temp = await ContentServ.GetVideosAsync(int.Parse(gridview.Tag.ToString()), 1);
                if (temp != null)
                {
                    foreach (var item in temp)
                    {
                        gridview.Items.Add(item);
                    }
                }
            }
        }

        private GridView GetGridView(object tag, string name = "")
        {
            ResourceDictionary dic = new ResourceDictionary { Source = new System.Uri("ms-appx:///StyleDic.xaml") };
            GridView _gridview = new GridView();
            _gridview.IsItemClickEnabled = true;
            _gridview.ItemClick += gridview_ItemClick;
            _gridview.ItemTemplate = (DataTemplate)Resources["dt_list"];
            _gridview.Template = (ControlTemplate)dic["myControlTemplete"];
            _gridview.ContainerContentChanging += gridview_ContainerContentChanging;
            _gridview.HorizontalAlignment = HorizontalAlignment.Center;
            _gridview.Tag = tag;
            if (!string.IsNullOrEmpty(name))
            {
                _gridview.Name = name;
            }
            return _gridview;
        }

        /// <summary>
        /// 获取指定参数的PivotItem
        /// </summary>
        /// <param name="arglist">Key:题头，Value:Tid</param>
        /// <returns></returns>
        private List<PivotItem> GetPivotItems(List<KeyValuePair<string,string>> arglist)
        {
            List<PivotItem> list = new List<PivotItem>();
            int index = 0;
            for (int i = 0; i < arglist.Count; i++)
            {
                KeyValuePair<string, string> arg = arglist[i];
                PivotItem item = new PivotItem { Margin = new Thickness(0) };
                item.Header = arg.Key;
                item.Content = GetGridView(arg.Value, "gridview" + index.ToString());
                list.Add(item);
                index++;
            }
            return list;
        }
    }
}
