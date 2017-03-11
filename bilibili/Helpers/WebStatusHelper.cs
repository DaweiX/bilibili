using Windows.Networking.Connectivity;

namespace bilibili.Helpers
{
    public enum ConnectionType
    {
        NoConn,
        DataConn,
        WlanConn,
        PPPoE
    }

    class WebStatusHelper
    {
       /// <summary>
       /// 检查是否有网络连接
       /// </summary>
        public static bool IsOnline()
        {
            bool isConnection = false;
            ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
                isConnection = true;
            return isConnection;
        }
       /// <summary>
       /// 获取网络连接类型
       /// </summary>
       /// <returns></returns>
        public static ConnectionType GetConnType()
        {
            ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
            {
                if (profile.IsWwanConnectionProfile)
                    return ConnectionType.DataConn;
                else if (profile.IsWlanConnectionProfile)
                    return ConnectionType.WlanConn;
                else
                    return ConnectionType.PPPoE;
            }
            return ConnectionType.NoConn;
        }
    }
}
