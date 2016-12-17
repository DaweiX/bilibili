using System;

namespace bilibili.Methods
{
    static class StringDeal
    {
        public static string delQuotationmarks(string str)
        {
            str = str.Remove(0, 1);
            str = str.Remove(str.Length - 1, 1);
            return str;
        }

        public static string LinuxToData(string str)
        {
            try
            {
                long sec = long.Parse(str);
                DateTimeOffset start = DateTimeOffset.FromUnixTimeSeconds(sec);
                return start.DateTime.ToLocalTime().ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string RemoveSpecial(string str)
        {
            string name = str.Replace('/', '_');
            name = name.Replace('\\', '_');
            name = name.Replace('*', '_');
            name = name.Replace('?', '_');
            name = name.Replace(':', '_');
            name = name.Replace('"', '_');
            name = name.Replace('|', '_');
            name = name.Replace('<', '_');
            name = name.Replace('>', '_');
            return name;
        }
    } 
}
