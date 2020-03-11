using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace RedditFeed
{
    /// <summary>
    /// Provides a collection class for <see cref="Post"/> objects.
	/// </summary>
    public class PostCollection : ICollection<Post>, ICollection, INotifyCollectionChanged
    {
        #region FIELDS/CONSTANTS
        /// <summary>
        /// The internal, backing <see cref="List{T}"/> collection that all methods invoke against.
        /// </summary>
        protected List<Post> InnerList;

        private ListCollectionView _backingView;

        private static JsonSerializer DefaultSerializer = new JsonSerializer
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTime,
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        #endregion

        #region INDEXERS
        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-bsaed index of the element to get.</param>
        public Post this[int index]
        {
            get => this.InnerList [index];
            set => this.InnerList[index] = value;
        }

        public Post this[string postId] => this.InnerList.Find(x => x.Id.Equals(postId));

        #endregion

        #region PROPERTIES
        /// <summary>
        /// Get the number of elements contained within the <see cref="PostCollection{T}"/>.
        /// </summary>
        public int Count => this.InnerList.Count;
        public Uri FeedUrl { get; private set; }
        public ListCollectionView View => _backingView;
        bool ICollection<Post>.IsReadOnly => false;
        bool ICollection.IsSynchronized => ((ICollection)this.InnerList).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)this.InnerList).SyncRoot;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Initializes a new instance of the <see cref="PostCollection{T}"/> class that is empty
        /// and has the default initial capacity.
        /// </summary>
        public PostCollection()
        {
            this.InnerList = new List<Post>();
            this.UpdateView();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PostCollection{T}"/> class that is empty
        /// and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new collection can initially store.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public PostCollection(int capacity)
        {
            this.InnerList = new List<Post>(capacity);
            this.UpdateView();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PostCollection{T}"/> class that
        /// contains elements copied from the specified <see cref="IEnumerable{T}"/> and has
        /// sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="items">The collection whose elements are copied to the new list.</param>
        /// <exception cref="ArgumentNullException"/>
        public PostCollection(IEnumerable<Post> items)
        {
            this.InnerList = new List<Post>(items);
            this.UpdateView();
        }

        #endregion

        #region EVENT HANDLERS

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this, e);
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
        }

        #endregion

        #region BASE METHODS
        public void Add(Post post)
        {
            this.InnerList.Add(post);
            //this.OnCollectionChanged(NotifyCollectionChangedAction.Add);
        }
        public void AddSortDescription(Expression<Func<Post, object>> propEx, ListSortDirection direction)
        {
            if (propEx.Body is MemberExpression memEx)
            {
                this.View.SortDescriptions.Add(new SortDescription(memEx.Member.Name, direction));
            }
            else if (propEx.Body is UnaryExpression unEx && unEx.Operand is MemberExpression unExMem)
            {
                this.View.SortDescriptions.Add(new SortDescription(unExMem.Member.Name, direction));
            }
        }
        public void AddRange(IEnumerable<Post> posts)
        {
            this.InnerList.AddRange(posts);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add);
        }
        public void Clear()
        {
            this.InnerList.Clear();
            this.OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        }
        public bool Contains(Post post) => this.InnerList.Contains(post);
        public bool Contains(Predicate<Post> match) => this.InnerList.Exists(match);
        void ICollection<Post>.CopyTo(Post[] array, int arrayIndex) => this.InnerList.CopyTo(array, arrayIndex);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)this.InnerList).CopyTo(array, index);
        public Post Find(Predicate<Post> match) => this.InnerList.Find(match);
        public List<Post> FindAll(Predicate<Post> match) => this.InnerList.FindAll(match);
        public int IndexOf(Post post) => this.InnerList.IndexOf(post);
        public bool Remove(Post post)
        {
            bool removed = this.InnerList.Remove(post);
            if (removed)
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove);

            return removed;
        }
        public int RemoveAll(Predicate<Post> match)
        {
            int removed = this.InnerList.RemoveAll(match);
            if (removed != -1)
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove);

            return removed;
        }
        public virtual void Sort()
        {
            this.InnerList.Sort();
            //this.OnCollectionChanged(NotifyCollectionChangedAction.Move);
        }
        public void Sort(IComparer<Post> comparer)
        {
            this.InnerList.Sort(comparer);
            //this.OnCollectionChanged(NotifyCollectionChangedAction.Move);
        }
        public void UpdateView()
        {
            _backingView = CollectionViewSource.GetDefaultView(this.InnerList) as ListCollectionView;
        }

        #endregion

        #region LOADING METHODS
        public static PostCollection LoadFeed(Uri url, PostRange range)
        {
            JObject feedJob = privateGetFeed(url, range, out Uri feedUrl);
            if (feedJob != null)
            {
                new PostCollection(feedJob.SelectToken("$.feed.entry").ToObject<List<Post>>(new JsonSerializer
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,

                }));
            }
        }
        public void ReloadFeed(Uri url, PostRange range)
        {
            JObject feedJob = privateGetFeed(url, range, out Uri feedUrl);
            if (feedJob != null)
            {
                this.FeedUrl = feedUrl;
                this.Clear();
                this.AddRange(feedJob.SelectToken("$.feed.entry").ToObject<List<Post>>());
                this.UpdateView();
            }

        }

        private static JObject privateGetFeed(Uri url, PostRange range, out Uri feedUrl)
        {
            UriBuilder builder = new UriBuilder(url)
            {
                Query = string.Format("?t={0}", range.ToString().ToLower())
            };
            feedUrl = builder.Uri;

            var xml = new XmlDocument();
            xml.Load(feedUrl.ToString());

            return JObject.Parse(JsonConvert.SerializeXmlNode(xml));
        }

        #endregion

        #region ENUMERATOR
        public IEnumerator<Post> GetEnumerator() => this.InnerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.InnerList.GetEnumerator();

        #endregion

        #region BACKEND/PRIVATE METHODS
        

        #endregion
    }
}