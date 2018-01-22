using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ImportTweets
{
    public static class UpdateTweetList
    {
        [FunctionName("UpdateTweetList")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            string resultString = string.Empty;
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var twitter = new TwitterIntegration("", "",
                "", "");

            var response = twitter.GetTweets("@salesforce", 10);
            var tweets = JsonConvert.DeserializeObject<List<TweetResponse>>(response);

            var putReponse = new List<TweetProjection>();
            foreach (var tweet in tweets)
            {
                var t = new TweetProjection()
                {
                    UserName = tweet.User.Name,
                    ScreenName = tweet.User.Screen_name,
                    ProfileImageUrl = tweet.User.Profile_background_image_url,
                    Text = tweet.Text,
                    CreatedAt = tweet.Created_at,
                    RetweetCount = tweet.Retweet_count
                };

                putReponse.Add(t);
            }

            var request = (HttpWebRequest)WebRequest.Create("http://tweetapi.azurewebsites.net/api/tweets");
            request.Method = "PUT";

            var encoding = new UTF8Encoding();
            var serializedTweets = JsonConvert.SerializeObject(putReponse);
            request.ContentLength = encoding.GetByteCount(serializedTweets);
            request.ContentType = "application/json";

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(encoding.GetBytes(serializedTweets), 0, encoding.GetByteCount(serializedTweets));
            }

            // Get the response and read it into a string.
            using (var putResponse = (HttpWebResponse)request.GetResponse())
            {
                if (putResponse.StatusCode != HttpStatusCode.OK)
                {
                    log.Info(string.Format("Unexpected status code returned: {0}", putResponse.StatusCode));
                }
            }
        }
    }
}
