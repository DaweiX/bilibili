using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class Reply
    {
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
    }
    public class Friend
    {
        public string AddTime { get; set; }
        public string Face { get; set; }
        public string Uname { get; set; }
        public string Fid { get; set; }
    }
}
