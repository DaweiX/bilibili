using Windows.Storage;

namespace bilibili.Helpers
{
    public enum DeviceType
    {
        PC,
        Mobile,
        Unknown
    }
    class SettingHelper
    {
        static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;

       /// <summary>
       /// 获取指定键的值
       /// </summary>
        public static object GetValue(string key)
        {
            if (container.Values[key] != null)
            {
                return container.Values[key];
            }
            else
            {
                return null;
            }
        }

       /// <summary>
       /// 设置指定键的值
       /// </summary>
        public static void SetValue(string key,object value)
        {
            container.Values[key] = value;     
        }

       /// <summary>
       /// 指示应用容器内是否存在某键
       /// </summary>
        public static bool ContainsKey(string key)
        {
            if (container.Values[key] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
     
       /// <summary>
       /// 设备类型
       /// </summary>
       /// <returns></returns>
        public static DeviceType DeviceType
        {
            get
            {
                string type = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
                switch (type)
                {
                    case "Windows.Desktop": return DeviceType.PC;
                    case "Windows.Mobile": return DeviceType.Mobile;
                    default: return DeviceType.Unknown;
                }
            }
        }
    }
}
