using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Xml;

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
        public DateTime Updated { get; private set; }

        public int CompareTo(Post other)
        {
            return this.Updated.CompareTo(other.Updated);
        }

        public void GoTo() => this.GoTo(this.Link.ToString());
        private void GoTo(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Win32Exception)
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
        }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Author
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("uri")]
        public Uri Link { get; private set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PostLink
    {
        [JsonProperty("@href")]
        public Uri Url { get; private set; }
    }
}
