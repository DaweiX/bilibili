using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili.Models
{
    class Topic
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Pic { get; set; }
    }

    class Event
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Cover { get; set; }
        public string Status { get; set; }
    }

    class KeyWord
    {
        private string _status;
        public string Keyword { get; set; }
        public string Status
        {
            get
            {
                switch (_status)
                {
                    case "up": return "↑";
                    case "down": return "↓";
                    case "keep": return "→";
                    default: return "";
                }
            }
            set
            {
                _status = value;
            }
        }
    }
}
