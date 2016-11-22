using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili.Controls
{
    public sealed partial class MessagePopup : UserControl
    {
        public MessagePopup()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 内容提示弹出控件
        /// </summary>
        /// <param name="content">提示内容</param>
        /// <param name="delay">显示的毫秒数</param>
        public async void Show(string content,int delay = 3000)
        {
            grid.Visibility = Visibility.Visible;
            txt.Text = content;
            await Task.Delay(delay);
            grid.Visibility = Visibility.Collapsed;
        }
    }
}
