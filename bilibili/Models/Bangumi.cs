using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class Bangumi
    {
        private string count;
        private bool isfinish;
        public string Cover { get; set; }
        public string Title { get; set; }
        public string Time { get; set; }
        public string Count
        {
            get
            {
                if (isfinish)
                {
                    return count + "话全";
                }
                else
                {
                    return "更新到第" + count + "话";
                }
            }
            set { count = value; }
        }
        public bool IsFinish
        {
            get { return isfinish; }
            set { isfinish = value; }
        }
        public string ID { get; set; }
        public string Brief { get; set; }
        public string New { get; set; }
    }

    class Chat
    {
        public string Title { get; set; }
        public string Time { get; set; }
        public string Content { get; set; }
        public string Face { get; set; }
        public string Name { get; set; }
        public string Mid { get; set; }
        public string Aid { get; set; }
    }
    class Notify
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Time { get; set; }
    }
    class Wisper
    {
        public string Face { get; set; }
        public string Time { get; set; }
        public string rid { get; set; }
        public string Room { get; set; }
        public string LastMsg { get; set; }
    }
    class HotBangumi
    {
        public string Cursor { get; set; }
        public string Cover { get; set; }
        public string Desc { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
    }
    class History
    {
        public string Aid { get; set; }
        public string Time { get; set; }
        public string Title { get; set;}
        public string Pic { get; set; }
    }
    class LocalVideo
    {
        public string Part { get; set; }
        public string Folder { get; set; }
        // public string Cid { get; set; }
        public string Format { get; set; }
    }
    class VideoURL
    {
        public double TotalLength { get; set; }
        public List<string> Acceptformat { get; set; }
        public List<string> Acceptquality { get; set; }
        public List<Purl> Ps { get; set; }
        public double Size { get; set; }
    }

    public class Purl
    {
        public double Length { get; set; }
        public double Size { get; set; }
        public string Url { get; set; }
        public List<string> Backup_URL { get; set; }
    }

    public class FlipItem
    {
        public string Img { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
    }
   /// <summary>
   /// 视频格式(其中,hdmp4的最高清晰度与mp4相同，弃用)
   /// </summary>
    public enum VideoFormat
    {
        flv,
        mp4,
        hdmp4
    }
    class LiveBanner
    {
        public string Img { get; set; }
        public string Link { get; set; }
        public string Remark { get; set; }
        public string Title { get; set; }
    }
    class Partition
    {
        public string Name { get; set; }
        public string Count { get; set; }
        public string Id { get; set; }
        public string Icon { get; set; }
    }
    class Live
    {
        private string online;
        public string AcQuality { get; set; }
        public string Online
        {
            get { return online + "人在线"; }
            set { online = value; }
        }
        public string PlayUrl { get; set; }
        public string RoomId { get; set; }
        public string Title { get; set; }
        public string Face { get; set; }
        public string Name { get; set; }
        public string Mid { get; set; }
        public string Cover { get; set; }
    }
    class CoinHs
    {
        public string Delta { get; set; }
        public string Reason { get; set; }
        public string Time { get; set; }
    }
    class PraiseMe
    {
        public string Content { get; set; }
        public string Face { get; set; }
        public string Mid { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public string Title { get; set; }
        public string Reply { get; set; }
        public string Aid { get; set; }
    }
    class RelateVideo
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string View { get; set; }
        public string Danmaku {  get; set; }
        public string Favorite { get; set; }
        public string Owner { get; set; }
        public string Cover { get; set; }
    }
}
