using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class Times
    {
        private int _weekday;
        private int _isFinish;
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
        public string IsFinish
        {
            get
            {
                return _isFinish == 0 ? "未完结" : "已完结";
            }
            set { _isFinish = int.Parse(value); }
        }
        /// <summary>
        /// 星期X
        /// </summary>
        public string Weekday
        {
            get
            {
                switch (_weekday)
                {
                    case -1: return "其他";
                    case 1: return "星期一";
                    case 2: return "星期二";
                    case 3: return "星期三";
                    case 4: return "星期四";
                    case 5: return "星期五";
                    case 6: return "星期六";
                    case 0: return "星期日";
                    default: return "未知";
                }
            }
            set
            {
                _weekday = int.Parse(value);
            }
        }
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
