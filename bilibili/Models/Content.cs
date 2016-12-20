using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace bilibili.Models
{
    class Content
    {
        private string creat;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        /// <summary>
        /// 图片
        /// </summary>
        public string Pic { get; set; }
        /// <summary>
        /// 视频编号
        /// </summary>
        public string Num { get; set; }
        public string Creat
        {
            get { return "收藏于" + creat; }
            set { creat = value; }
        }
        public string Play { get; set; }
        public string Comment { get; set; }
    }

    class Basic
    {
        public string Cover { get; set; }
        public string Title { get; set; }
        public string ID { get; set; }
        public string sqCover { get; set; }
        public string Owner { get; set; }
        public string View { get; set; }
        public string Favorite { get; set; }
        public string Danmaku { get; set; }
    }

    class Concern:Basic
    {
        public string IsConcerned { get; set; }
        public string _new;
        public string _isfinish;
        public string isFinish
        {
            get { return _isfinish; }
            set { _isfinish = value; }
        }
        public string New
        {
            get
            {
                if (_isfinish == "1") return "已完结，" + _new + "话全";
                else if (_isfinish == "0") return "更新到" + _new + "话";
                else return _new;
            }
            set
            {
                _new = value;
            }
        }
    }
    class Folder
    {
        public string Ctime { get; set; }
        public string Fid { get; set; }
        public string Name { get; set; }
        public string Count { get; set; }
        public string MCount { get; set; }
        public string State { get; set; }
        public List<Basic> Videos { get; set; }
    }
    class User
    {
        public string Attention { get; set; }
        public string Sign { get; set; }
        public string Discription { get; set; }
        public List<string> Attentions { get; set; }
        public string Article { get; set; }
        public string BirthDay { get; set; }
        public string Face { get; set; }
        public string Coins { get; set; }
        public string Fans { get; set; }
        public string Friend { get; set; }
        public string Current_level { get; set; }
        public string Current_exp { get; set; }
        public string Current_min { get; set; }
        public string Next_exp { get; set; }
        public string Im9_sign { get; set; }
        public string Mid { get; set; }
        public string Name { get; set; }
        public string RegTime { get; set; }
        public string Sex { get; set; }
        public string Toutu { get; set; }
        public string ToutuId { get; set; }
        public string Place { get; set; }
    }
}
