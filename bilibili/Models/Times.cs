using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class Times
    {
        /// <summary>
        /// 封面
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 是否更新
        /// </summary>
        public bool IsNew { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string LastUpdate { get; set; }
        /// <summary>
        /// 是否完结
        /// </summary>
        public int IsFinish { get; set; }
        /// <summary>
        /// 星期X
        /// </summary>
        public int Weekday { get; set; }
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 总集数
        /// </summary>
        public string Count { get; set; }
    }
}
