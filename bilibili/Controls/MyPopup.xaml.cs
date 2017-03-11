using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

//  The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili.Controls
{
    public sealed partial class MyPopup : UserControl
    {
        public MyPopup()
        {
            this.InitializeComponent();
        }
        public async Task Show(string message)
        {
            txt_pop.Text = message;
            pop.IsOpen = true;
            await Task.Delay(1500);
            pop.IsOpen = false;
        }
    }
}
