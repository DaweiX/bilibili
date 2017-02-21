namespace bilibili.Models
{
    class Pulls
    {
        private string avatar = string.Empty;
        private string index;
        public string Aid { get; set; }
        public string Author { get; set; }
        public string Create { get; set; }
        public string Pic { get; set; }
        public string Title { get; set; }
        public string Avatar
        {
            get { return avatar; }
            set { avatar = value; }
        }
        public string Type { get; set; }
        public string Index
        {
            get { return "第" + index + "话"; }
            set { index = value; }
        }
        public string Play { get; set; }
        public string Danmaku { get; set; }
        public string BanTitle { get; set; }
    }
}
