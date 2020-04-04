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
        private string _dtFormat;
        [JsonProperty("dateTimeFormat")]
        public string DateTimeFormat
        {
            get => _dtFormat;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _dtFormat = value;

                else
                    _dtFormat = Resources.Post_Updated_DateString;

            }
        }

        [JsonProperty("hideAuthor")]
        public bool HideAuthorColumn { get; set; } = false;

        private string _sub;
        [JsonProperty("subreddit", Order = 1)]
        public string Subreddit
        {
            get => _sub;
            set
            {
                _sub = value;
                this.UpdateUrl(!string.IsNullOrWhiteSpace(value));
            }
        }

        #region JSON PROPERTIES
        [JsonProperty("rangeOfPosts", Order = 2)]
        [JsonConverter(typeof(CamelEnumConverter))]
        public PostRange Range { get; set; } = PostRange.Day;

        public Uri SubredditUrl { get; private set; }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if (string.IsNullOrWhiteSpace(this.Subreddit))
            {
                Subreddit = Resources.DefaultSubreddit;
            }
            if (string.IsNullOrWhiteSpace(this.DateTimeFormat))
                this.DateTimeFormat = Resources.Post_Updated_DateString;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (string.IsNullOrWhiteSpace(this.Subreddit))
            {
                this.Subreddit = Resources.DefaultSubreddit;
            }
            if (string.IsNullOrWhiteSpace(this.DateTimeFormat))
                this.DateTimeFormat = Resources.Post_Updated_DateString;
        }

        private void UpdateUrl(bool defined)
        {
            if (defined)
            {
                this.SubredditUrl = new Uri(
                    string.Format(
                        Resources.RedditUrlFormat, this.Subreddit
                    ),
                    UriKind.Absolute
                );
            }
            else
            {
                this.SubredditUrl = null;
            }
        }

        #endregion
    }

    public enum PostRange
    {
        None,
        Week,
        Day
    }
}