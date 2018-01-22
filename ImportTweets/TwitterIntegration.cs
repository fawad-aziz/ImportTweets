using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ImportTweets
{
    public class TwitterIntegration
    {
        private const string OauthVersion = "1.0";
        private const string OauthSignatureMethod = "HMAC-SHA1";

        private readonly string _consumerKey;
        private readonly string _consumerKeySecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;

        public TwitterIntegration(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerKeySecret = consumerKeySecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
        }

        public string GetTweets(string screenName, int count)
        {
            string resourceUrl = string.Format("https://api.twitter.com/1.1/statuses/user_timeline.json");

            var requestParameters = new SortedDictionary<string, string>
            {
                { "count", count.ToString() },
                { "screen_name", screenName }
            };

            var response = GetResponse(resourceUrl, Method.GET, requestParameters);

            return response;
        }

        private string GetResponse(string resourceUrl, Method method, SortedDictionary<string, string> requestParameters)
        {
            ServicePointManager.Expect100Continue = false;
            WebRequest request = null;
            string resultString = string.Empty;

            request = (HttpWebRequest)WebRequest.Create(resourceUrl + "?" + requestParameters.ToWebString());
            request.Method = method.ToString();

            if (request != null)
            {
                var authHeader = CreateHeader(resourceUrl, method, requestParameters);
                request.Headers.Add("Authorization", authHeader);
                var response = request.GetResponse();

                using (var sd = new StreamReader(response.GetResponseStream()))
                {
                    resultString = sd.ReadToEnd();
                    response.Close();
                }
            }

            return resultString;
        }

        private string CreateOauthNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        private string CreateHeader(string resourceUrl, Method method, SortedDictionary<string, string> requestParameters)
        {
            var oauthNonce = CreateOauthNonce();
            var oauthTimestamp = CreateOAuthTimestamp();
            var oauthSignature = CreateOauthSignature
            (resourceUrl, method, oauthNonce, oauthTimestamp, requestParameters);

            //The oAuth signature is then used to generate the Authentication header. 
            const string headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method =\"{1}\", " + "oauth_timestamp=\"{2}\", oauth_consumer_key =\"{3}\", " + "oauth_token=\"{4}\", oauth_signature =\"{5}\", " + "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                           Uri.EscapeDataString(oauthNonce),
                                           Uri.EscapeDataString(OauthSignatureMethod),
                                           Uri.EscapeDataString(oauthTimestamp),
                                           Uri.EscapeDataString(_consumerKey),
                                           Uri.EscapeDataString(_accessToken),
                                           Uri.EscapeDataString(oauthSignature),
                                           Uri.EscapeDataString(OauthVersion)
                );

            return authHeader;
        }

        private string CreateOauthSignature(string resourceUrl, Method method, string oauthNonce, string oauthTimestamp, SortedDictionary<string, string> requestParameters)
        {
            //firstly we need to add the standard oauth parameters to the sorted list
            requestParameters.Add("oauth_consumer_key", _consumerKey);
            requestParameters.Add("oauth_nonce", oauthNonce);
            requestParameters.Add("oauth_signature_method", OauthSignatureMethod);
            requestParameters.Add("oauth_timestamp", oauthTimestamp);
            requestParameters.Add("oauth_token", _accessToken);
            requestParameters.Add("oauth_version", OauthVersion);

            var sigBaseString = requestParameters.ToWebString();

            var signatureBaseString = string.Concat(method.ToString(), "&", Uri.EscapeDataString(resourceUrl), "&",
                                Uri.EscapeDataString(sigBaseString.ToString()));

            //Using this base string, we then encrypt the data using a composite of the 
            //secret keys and the HMAC-SHA1 algorithm.
            var compositeKey = string.Concat(Uri.EscapeDataString(_consumerKeySecret), "&", Uri.EscapeDataString(_accessTokenSecret));

            string oauthSignature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                oauthSignature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString)));
            }

            return oauthSignature;
        }

        private static string CreateOAuthTimestamp()
        {
            var nowUtc = DateTime.UtcNow;
            var timeSpan = nowUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            return timestamp;
        }
    }

    public enum Method
    {
        GET,
        PUT
    }
}
