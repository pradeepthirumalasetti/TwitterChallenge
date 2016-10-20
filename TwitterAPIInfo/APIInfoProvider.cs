using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace TwitterAPIInfo.APIController
{
    public class APIInfoProvider
    {
        private readonly OAuthInfo oauth;

        public APIInfoProvider(OAuthInfo oauth)
        {
            this.oauth = oauth;
        }
        public List<TweetInfo> GetHomeTimeline(long? sinceId = null, long? maxId = null, int? count = 20, string screenName = "salesforce")
        {
            //API URL
            return GetTimeline("https://api.twitter.com/1.1/statuses/user_timeline.json", sinceId, maxId, count, screenName);
        }

        private List<TweetInfo> GetTimeline(string url, long? sinceId, long? maxId, int? count, string screenName)
        {
            var builder = new RequestBuilder(oauth, "GET", url);

            if (sinceId.HasValue)
                builder.AddParameter("since_id", sinceId.Value.ToString());

            if (maxId.HasValue)
                builder.AddParameter("max_id", maxId.Value.ToString());

            if (count.HasValue)
                builder.AddParameter("count", count.Value.ToString());

            if (screenName != "")
                builder.AddParameter("screen_name", screenName);

            var responseContent = builder.Execute();

            var serializer = new JavaScriptSerializer();

            var tweets = (object[])serializer.DeserializeObject(responseContent);

            List<TweetInfo> lstTweets = new List<TweetInfo>();
            foreach(Dictionary<string, object> tweet in tweets)
            {
                var user = ((Dictionary<string, object>)tweet["user"]);
                lstTweets.Add(new TweetInfo
                {
                    TweetID =  tweet["id"] != null ? (long)tweet["id"] : 0,
                    TweetText = tweet["text"] != null ? (string)tweet["text"] : string.Empty,
                    UserName = user != null && user["name"] != null ? (string)user["name"] : string.Empty,
                    ScreenName = user != null && user["screen_name"] != null ? (string)user["screen_name"] : string.Empty,
                    ProfileImageUrl = user != null && user["profile_image_url"] != null ?(string)user["profile_image_url"] : string.Empty,
                    RetweetCount = tweet["retweet_count"] != null ? (int)tweet["retweet_count"] : 0
                });
            };
            return lstTweets;
        }
    }

    public class RequestBuilder
    {
        private const string VERSION = "1.0";
        private const string SIGNATURE_METHOD = "HMAC-SHA1";

        private readonly OAuthInfo oauth;
        private readonly string method;
        private readonly IDictionary<string, string> customParameters;
        private readonly string url;

        public RequestBuilder(OAuthInfo oauth, string method, string url)
        {
            this.oauth = oauth;
            this.method = method;
            this.url = url;
            customParameters = new Dictionary<string, string>();
        }

        public RequestBuilder AddParameter(string name, string value)
        {
            customParameters.Add(name, value.EncodeRFC3986());
            return this;
        }

        public string Execute()
        {
            var timespan = GetTimestamp();
            var nonce = CreateNonce();

            var parameters = new Dictionary<string, string>(customParameters);
            AddOAuthParameters(parameters, timespan, nonce);

            var signature = GenerateSignature(parameters);
            var headerValue = GenerateAuthorizationHeaderValue(parameters, signature);

            var request = (HttpWebRequest)WebRequest.Create(GetRequestUrl());
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";

            request.Headers.Add("Authorization", headerValue);

            WriteRequestBody(request);

           

            var response = request.GetResponse();

            string content;

            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            }

            request.Abort();

            return content;
        }

        private void WriteRequestBody(HttpWebRequest request)
        {
            if (method == "GET")
                return;

            var requestBody = Encoding.ASCII.GetBytes(GetCustomParametersString());
            using (var stream = request.GetRequestStream())
                stream.Write(requestBody, 0, requestBody.Length);
        }

        private string GetRequestUrl()
        {
            if (method != "GET" || customParameters.Count == 0)
                return url;

            return string.Format("{0}?{1}", url, GetCustomParametersString());
        }

        private string GetCustomParametersString()
        {
            return customParameters.Select(x => string.Format("{0}={1}", x.Key, x.Value)).Join("&");
        }

        private string GenerateAuthorizationHeaderValue(IEnumerable<KeyValuePair<string, string>> parameters, string signature)
        {
            return new StringBuilder("OAuth ")
                .Append(parameters.Concat(new KeyValuePair<string, string>("oauth_signature", signature))
                            .Where(x => x.Key.StartsWith("oauth_"))
                            .Select(x => string.Format("{0}=\"{1}\"", x.Key, x.Value.EncodeRFC3986()))
                            .Join(","))
                .ToString();
        }

        private string GenerateSignature(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var dataToSign = new StringBuilder()
                .Append(method).Append("&")
                .Append(url.EncodeRFC3986()).Append("&")
                .Append(parameters
                            .OrderBy(x => x.Key)
                            .Select(x => string.Format("{0}={1}", x.Key, x.Value))
                            .Join("&")
                            .EncodeRFC3986());

            var signatureKey = string.Format("{0}&{1}", oauth.ConsumerSecret.EncodeRFC3986(), oauth.AccessSecret.EncodeRFC3986());
            var sha1 = new HMACSHA1(Encoding.ASCII.GetBytes(signatureKey));

            var signatureBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(dataToSign.ToString()));
            return Convert.ToBase64String(signatureBytes);
        }

        private void AddOAuthParameters(IDictionary<string, string> parameters, string timestamp, string nonce)
        {
            parameters.Add("oauth_version", VERSION);
            parameters.Add("oauth_consumer_key", oauth.ConsumerKey);
            parameters.Add("oauth_nonce", nonce);
            parameters.Add("oauth_signature_method", SIGNATURE_METHOD);
            parameters.Add("oauth_timestamp", timestamp);
            parameters.Add("oauth_token", oauth.AccessToken);
        }

        private static string GetTimestamp()
        {
            return ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }

        private static string CreateNonce()
        {
            return new Random().Next(0x0000000, 0x7fffffff).ToString("X8");
        }
    }

    public static class HelperExtensions
    {
        public static string Join<T>(this IEnumerable<T> items, string separator)
        {
            return string.Join(separator, items.ToArray());
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T value)
        {
            return items.Concat(new[] { value });
        }

        public static string EncodeRFC3986(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var encoded = Uri.EscapeDataString(value);

            return Regex
                .Replace(encoded, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper())
                .Replace("(", "%28")
                .Replace(")", "%29")
                .Replace("$", "%24")
                .Replace("!", "%21")
                .Replace("*", "%2A")
                .Replace("'", "%27")
                .Replace("%7E", "~");
        }
    }

    public class OAuthInfo
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
    }
}
