using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RedditFeed
{
    internal class RssFeed
    {
        private JToken _feed;
        private JsonSerializer DefaultSerializer { get; } = new JsonSerializer
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTime,
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        internal Uri FeedUrl { get; }

        internal RssFeed(Uri url)
        {
            UriBuilder builder = new UriBuilder(url)
            {
                Query = "?t=day"
            };
            this.FeedUrl = builder.Uri;
        }

        internal void GetFeed()
        {
            var xml = new XmlDocument();
            xml.Load(this.FeedUrl.ToString());
            _feed = JObject.Parse(JsonConvert.SerializeXmlNode(xml)).SelectToken("$.feed");
        }

        internal PostCollection GetPosts()
        {
            return _feed.SelectToken("$.entry").ToObject<PostCollection>(this.DefaultSerializer);
        }
    }
}
