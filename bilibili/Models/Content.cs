using System.Collections.Generic;

namespace bilibili.Models
{
    class Content
    {
        private string creat;
        public string Title { get; set; }
        public string Pic { get; set; }
        public string Num { get; set; }
        public string Creat
        {
            get { return "收藏于" + creat; }
            set { creat = value; }
        }
        public string Play { get; set; }
        public string Comment { get; set; }
    }

    class Video
    {
        public string Aid { get; set; }
        public string Title { get; set; }
        public string Play { get; set; }
        public string Danmaku { get; set; }
        public string Author { get; set; }
        public string Cover { get; set; }
        public bool IsNew { get; set; }
    }

    class Site_Concern
    {
        public string Code { get; set; }
        public bool Status { get; set; }
        public string Count { get; set; }
        public List<ConcernItem> Result { get; set; }
    }

    class ConcernItem
    {
        public string Cover { get; set; }
        public string Bangumi_title { get; set; }
        public string Title { get; set; }
        public string Season_id { get; set; }
        public string SquareCover { get; set; }
        public string Copyright { get; set; }
        public string Favorites { get; set; }
        public string Danmaku_count { get; set; }
        public string Attention { get; set; }
        private string _new;
        private string _isfinish;
        public string Is_finish
        {
            get { return _isfinish; }
            set { _isfinish = value; }
        }
        public string Newest_ep_index
        {
            get
            {
                if (_isfinish == "1") return  _new + "话全";
                else if (_isfinish == "0") return "更新到" + _new + "话";
                else return _new;
            }
            set
            {
                _new = value;
            }
        }
    }
    public class Pic
    {
        public int Count { get; set; }
        public string Pic1 { get; set; }
        public string Pic2 { get; set; }
        public string Pic3 { get; set; }
    }
    class Folder
    {
        public string Ctime { get; set; }
        public string Fid { get; set; }
        public string Name { get; set; }
        public string Count { get; set; }
        public string MCount { get; set; }
        public string State { get; set; }
        public Pic VideoPics { get; set; }
    }
    class UserInfo
    {
        public string Sign { get; set; }
        public string Discription { get; set; }
        public List<string> Attentions { get; set; }
        public string Article { get; set; }
        public string BirthDay { get; set; }
        public string Face { get; set; }
        public string Coins { get; set; }
        public string Fans { get; set; }
        public string Friend { get; set; }
        public string Im9_sign { get; set; }
        public string Mid { get; set; }
        public string Name { get; set; }
        public string RegTime { get; set; }
        public string Sex { get; set; }
        public string Toutu { get; set; }
        public string ToutuId { get; set; }
        public string Place { get; set; }
        public LevelInfo Level_Info { get; set; }
    }
    class UserSettings
    {        
        public ToutuSets Toutu { get; set; }      
    }
    class ToutuSets
    {
        public string l_img { get; set; }
        public string S_img { get; set; }
    }
    class LevelInfo
    {
        public string Current_level { get; set; }
        public string Current_exp { get; set; }
        public string Current_min { get; set; }
        public string Next_exp { get; set; }
    }
    /// <summary>
    /// 用户基本信息
    /// </summary>
    class Site_UserInfo
    {
        public bool Status { get; set; }
        public UserInfo Data { get; set; }
    }
    /// <summary>
    /// 用户设置（头图等）
    /// </summary>
    class Site_UserSettings
    {
        public bool Status { get; set; }
        public UserSettings Data { get; set; }
    }

    /// <summary>
    /// 投稿视频
    /// </summary>
    class Site_MyVideo
    {
        public bool Status { get; set; }
        public SubmitList Data { get; set; }
    }

    class SubmitList
    {
        public string Count { get; set; }
        public List<MyVideo> Vlist { get; set; }
    }

    class MyVideo
    {
        public string Aid { get; set; }
        public string Author { get; set; }
        public string Coins { get; set; }
        public string Copyright { get; set; }
        public string Description { get; set; }
        public string Length { get; set; }
        public string Favorites { get; set; }
        public string Pic { get; set; }
        public string Title { get; set; }
        public string Typename { get; set; }
        public string Play { get; set; }
    }

}
