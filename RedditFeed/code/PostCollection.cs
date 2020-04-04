using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace RedditFeed
{
    public class PostCollection : ObservableCollection<Post>
    {
        private ListCollectionView _backingView;
        private static JsonSerializer DefaultSerializer { get; } = new JsonSerializer
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTime,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        public Uri FeedUrl { get; private set; }
        public ListCollectionView View => _backingView;

        public PostCollection() : base()
        {
            this.UpdateView();
        }
        public PostCollection(IEnumerable<Post> posts) : base(posts)
        {
            this.UpdateView();
        }

        public void UpdateView()
        {
            _backingView = CollectionViewSource.GetDefaultView(base.Items) as ListCollectionView;
            this.AddSortDescription(x => x.LastUpdated, ListSortDirection.Descending);
        }
        public void UpdateView(Expression<Func<Post, object>> propEx, ListSortDirection direction)
        {
            _backingView = CollectionViewSource.GetDefaultView(this) as ListCollectionView;
            this.AddSortDescription(propEx, direction);
        }
        public void AddRange(IEnumerable<Post> posts)
        {
            foreach (Post p in posts)
            {
                base.Add(p);
            }
        }
        public void AddSortDescription(Expression<Func<Post, object>> exp, ListSortDirection direction)
        {
            if (_backingView != null)
            {
                string name = null;
                if (exp.Body is MemberExpression memEx)
                {
                    name = memEx.Member.Name;
                }
                else if (exp.Body is UnaryExpression unEx && unEx.Operand is MemberExpression unExMem)
                {
                    name = unExMem.Member.Name;
                }
                var sd = new SortDescription(name, direction);
                if (!_backingView.SortDescriptions.Contains(sd))
                {
                    _backingView.SortDescriptions.Add(sd);
                }
            }
        }

        public static PostCollection LoadFeed(Uri url, PostRange range)
        {
            JObject feedJob = privateGetFeed(url, range, out Uri feedUrl);
            if (feedJob != null)
            {
                var pCol = new PostCollection(feedJob.SelectToken("$.feed.entry").ToObject<List<Post>>(DefaultSerializer))
                {
                    FeedUrl = feedUrl
                };
                return pCol;
            }
            else
                return null;
        }
        public Task<List<Post>> ReloadFeed(Uri url, PostRange range)
        {
            return Task.Run(() =>
            {
                JObject feedJob = privateGetFeed(url, range, out Uri feedUrl);
                List<Post> posts = null;
                if (feedJob != null)
                {
                    this.FeedUrl = feedUrl;
                    posts = feedJob.SelectToken("$.feed.entry").ToObject<List<Post>>(DefaultSerializer);
                }
                return posts;
            });
        }
        private static JObject privateGetFeed(Uri url, PostRange range, out Uri feedUrl)
        {
            UriBuilder builder = new UriBuilder(url)
            {
                Query = string.Format("?t={0}", range.ToString().ToLower())
            };
            feedUrl = builder.Uri;

            var xml = new XmlDocument();
            xml.Load(feedUrl.AbsoluteUri);

            return JObject.Parse(JsonConvert.SerializeXmlNode(xml));
        }
    }
}
