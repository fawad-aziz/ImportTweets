namespace ImportTweets
{
    public class TweetResponse
    {
        public string Created_at { get; set; }
        public long Id { get; set; }
        public string Text { get; set; }
        public TwitterUser User { get; set; }
        public int Retweet_count { get; set; }
    }

    public class TwitterUser
    {
        public int Id { get; set; }
        public string Id_str { get; set; }
        public string Name { get; set; }
        public string Screen_name { get; set; }
        public string Profile_background_image_url { get; set; }
        public string Profile_background_image_url_https { get; set; }
        public string Profile_image_url { get; set; }
        public string Profile_image_url_https { get; set; }
    }
}
