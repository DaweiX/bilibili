using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class SearchResult
    {
        /// <summary>
        /// 番号
        /// </summary>
        public string Aid { get; set; }
        /// <summary>
        /// 上传者
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 收藏数
        /// </summary>
        public string Fav { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string Pic { get; set; }
        /// <summary>
        /// 播放数
        /// </summary>
        public string View { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        public string Review { get; set; }
        /// <summary>
        /// 分类标签
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
    }
    class SearchResult_Bangumi
    {
        public string Cover { get; set; }
        public string Title { get; set; }
        public string IsFinish { get; set; }
        public string ID { get; set; }
        public string Count { get; set; }
    }
}
