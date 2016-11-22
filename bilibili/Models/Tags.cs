using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class Tags
    {
        public string Cover { get; set; }
        public string TagID { get; set; }
        public string TagName { get; set; }
    }
    class Count
    {
        public string At_me { get; set; }
        public string Chat_me { get; set; }
        public string Notify_me { get; set; }
        public string Praise_me { get; set; }
        public string Reply_me { get; set; }
    }
}
