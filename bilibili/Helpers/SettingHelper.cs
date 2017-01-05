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
        public static DeviceType Devicetype;
        static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key">键名称</param>
        /// <returns></returns>
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
        /// <param name="key">键名称</param>
        /// <param name="value">值</param>
        public static void SetValue(string key,object value)
        {
            container.Values[key] = value;     
        }
        /// <summary>
        /// 指示应用容器内是否存在某键
        /// </summary>
        /// <param name="key">键名称</param>
        /// <returns></returns>
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
        /// 获取设备类型
        /// </summary>
        /// <returns></returns>
        public static DeviceType GetDeviceType()
        {
            string type = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
            switch(type)
            {
                case "Windows.Desktop":return DeviceType.PC;
                case "Windows.Mobile": return DeviceType.Mobile;
                default:return DeviceType.Unknown;
            }
        }
    }
}
