using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace ImportTweets
{
    public static class UpdateTweetList
    {
        [FunctionName("UpdateTweetList")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            string resultString = string.Empty;
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var twitter = new TwitterIntegration("wlpBwS7qxJlcpq49RCAPDC3uP", "Ya0zbzhRqLC4eWh6SZM2HLpENrUiqSiPoPOUPC2S7uF3oVvcYF",
                "818087845265018881-wo6JNjud3lE7EwbVDRrC1U6q31iBPtn", "Gu3ubtrC4PDTyu73jPLKe3x8ddg1jR0xoGJGDgVdw07YC");

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

            var request = (HttpWebRequest)WebRequest.Create("http://localhost:62499/api/tweets");
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
