using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class Reply
    {
        private string baseon;
        /// <summary>
        /// 评论
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Uname { get; set; }
        /// <summary>
        /// 回复
        /// </summary>
        public List<Reply> Res { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 赞
        /// </summary>
        public string Like { get; set; }
        public string Oid { get; set; }
        public string Parent { get; set; }
        public string Root { get; set; }
        public string Mid { get; set; }
        public string Rpid { get; set; }
        public string Baseon
        {
            get
            {
                return baseon == string.Empty ? string.Empty : "回复 @" + baseon + " :";
            }
            set { baseon = value; }
        }
    }

    public class Site_Friend
    {
        public int Result { get; set; }
        public List<Friend> List { get; set; }
        public bool isEmpty { get; set; }
    }

    public class Friend
    {
        public string AddTime { get; set; }
        public string Face { get; set; }
        public string Uname { get; set; }
        public string Fid { get; set; }
    }

    public class UpForSearch
    {
        private string archives = string.Empty;
        private string fans = string.Empty;
        public string Archives
        {
            get { return archives; }
            set { archives = value; }
        }
        public string Cover { get; set; }
        public string Param { get; set; }
        public string Title { get; set; }
        public string Fans
        {
            get
            {
                return "投稿:" + archives + "\t粉丝:" + fans;
            }
            set
            {
                fans = value;
            }
        }
    }
}
