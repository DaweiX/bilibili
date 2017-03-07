using System.Collections.Generic;

namespace bilibili.Helpers
{
    static class VideoHelper
    {
        /// <summary>
        /// 系统原生支持的视频格式
        /// 引用自https://support.microsoft.com/zh-cn/instantanswers/6ee34367-6814-47bc-823a-49d6abbc4823/file-formats-for-the-movies-tv-app
        /// </summary>
        public static List<string> videoExtensions_sys => new List<string>
        {
            //如果无法播放，为系统使用的编解码器不受支持的缘故
            ".3g2",
            ".3gp",
            ".3gpp",
            ".3gpp2",
            ".avi",  
            ".divx",
            ".m2t",
            ".m2ts",
            ".m4v",
            ".mkv",  //喵喵喵？
            ".mod",
            ".mov",
            ".mp4",
            ".mp4v",
            ".mpe",
            ".mpeg",
            ".mpg",
            ".mpv2",
            ".mts",
            ".tod",
            ".ts",
            ".tts",
            ".wm",
            ".wmv",
            ".xvid"
        };
    }
}
