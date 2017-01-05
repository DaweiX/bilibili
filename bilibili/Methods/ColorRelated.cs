using bilibili.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace bilibili.Methods
{
    class ColorRelated
    {
        public static Color GetColor()
        {
            Color color = new Color();
            switch (SettingHelper.GetValue("_Theme").ToString())
            {
                case "Pink":
                    color = Color.FromArgb(255, 226, 115, 169);
                    break;
                case "Red":
                    color = Color.FromArgb(255, 244, 67, 54);
                    break;
                case "Yellow":
                    color = Color.FromArgb(255, 255, 176, 7);
                    break;
                case "Green":
                    color = Color.FromArgb(255, 123, 179, 58);
                    break;
                case "Blue":
                    color = Color.FromArgb(255, 0, 153, 204);
                    break;
                case "Purple":
                    color = Color.FromArgb(255, 185, 44, 191);
                    break;
                case "Orange":
                    color = Color.FromArgb(255, 255, 102, 51);
                    break;
                default:
                    break;
            }
            return color;
        }

    }
}
