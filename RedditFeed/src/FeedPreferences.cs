using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RedditFeed
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FeedPreferences
    {
        [JsonProperty("subreddit", Order = 1)]
        public string Subreddit { get; set; }

        #region JSON PROPERTIES
        [JsonProperty("rangeOfPosts", Order = 2)]
        [JsonConverter(typeof(CamelEnumConverter))]
        public PostRange Range { get; set; } = PostRange.Day;

        public Uri SubredditUrl { get; private set; }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if (string.IsNullOrWhiteSpace(Subreddit))
            {
                Subreddit = Resources.DefaultSubreddit;
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (string.IsNullOrWhiteSpace(Subreddit))
            {
                Subreddit = Resources.DefaultSubreddit;
            }
            this.SetUrl(new Uri(string.Format(Resources.RedditUrlFormat, Subreddit), UriKind.Absolute));
        }

        internal void UpdateUrl() => this.SubredditUrl = new Uri(string.Format(Resources.RedditUrlFormat, this.Subreddit), UriKind.Absolute);
        public void SetUrl(Uri newUrl) => this.SubredditUrl = newUrl;

        #endregion
    }

    public enum PostRange
    {
        None,
        Week,
        Day
    }
}