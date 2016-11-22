using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class Details
    {
        /// <summary>
        /// 番号
        /// </summary>
        public string Aid { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// Up主
        /// </summary>
        public string Upzhu { get; set; }
        /// <summary>
        /// 视频地址编码
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 分类标签
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string Pic { get; set; }
        /// <summary>
        /// 播放数
        /// </summary>
        public string View { get; set; }
        /// <summary>
        /// 弹幕数
        /// </summary>
        public string Danmu { get; set; }
        /// <summary>
        /// 分集
        /// </summary>
        public List<Pages> Ps { get; set; }
        public string Coins { get; set; }
        public string Share { get; set; }
        public string Fav { get; set; }
        public string Reply { get; set; }
        public string Time { get; set; }
        public string IsFav { get; set; }
        public string Mid { get; set; }
    }

    class Pages
    {
        public string Cid { get; set; }
        public string Part { get; set; }
    }
}
