using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RedditFeed
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Post : IComparable<Post>
    {
        [JsonProperty("author")]
        private Author _author;

        [JsonProperty("link")]
        private PostLink _postLink;

        public string Author => _author.Name;
        public Uri AuthorLink => _author.Link;

        [JsonProperty("id")]
        public string Id { get; private set; }

        public Uri Link => _postLink.Url;

        [JsonProperty("title")]
        public string Title { get; private set; }

        [JsonProperty("updated")]
        public DateTime LastUpdated { get; private set; }
        public string Updated { get; private set; }

        public int CompareTo(Post other)
        {
            return this.LastUpdated.CompareTo(other.LastUpdated);
        }

        public void GoTo() => this.GoTo(this.Link.ToString());
        private void GoTo(string url)
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private const string DATE_FORMAT = "M/d h:mm tt";
        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this.Updated = LastUpdated.ToLocalTime().ToString(DATE_FORMAT);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Author
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("uri")]
        public Uri Link { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                this.Name = this.Name.Replace("/u/", string.Empty);
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PostLink
    {
        [JsonProperty("@href")]
        public Uri Url { get; private set; }
    }
}
