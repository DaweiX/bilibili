using Windows.UI.Xaml.Controls;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace bilibili
{
    public sealed partial class MainPage : Page
    {
        public class PaneItem
        {
            public string Title { get; set; }
            public string Glyph { get; set; }
            public int Index { get; set; }
            public bool IsLoginNeeded { get; set; }
        }
    }
}
