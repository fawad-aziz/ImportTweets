using System;

namespace ImportTweets
{
    public class TweetProjection
    {
        public string UserName { get; set; }

        public string ScreenName { get; set; }

        public string ProfileImageUrl { get; set; }

        public string Text { get; set; }

        public string CreatedAt { get; set; }

        public int RetweetCount { get; set; }
    }
}
