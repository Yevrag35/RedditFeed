using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RedditFeed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isLoading = true;

        internal JsonSettings AllPreferences;
        internal PostCollection Posts;

        public string SubredditLabel
        {
            get => this.AllPreferences?.Preferences?.Subreddit;
            set { return; }//this.AllPreferences?.SaveSubreddit(value, _isLoading);
        }

        public MainWindow()
        {
            this.AllPreferences = new JsonSettings(true);

            InitializeComponent();
            this.SRText.Text = this.AllPreferences.Preferences.Subreddit;
            var feed = new RssFeed(this.AllPreferences.Preferences.SubredditUrl);
            feed.GetFeed();
            this.Posts = feed.GetPosts();

            this.Posts.UpdateView();
            this.Posts.AddSortDescription(x => x.Updated, ListSortDirection.Descending);

            this.RedditList.ItemsSource = this.Posts.View;
            this.RedditList.Items.Refresh();

            _isLoading = false;
        }

        #region LOAD FEED
        private void LoadFeed()
        {

        }

        #endregion

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.RedditList.SelectedItem is Post post)
            {
                post.GoTo();
            }
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.SRLabel.Visibility = Visibility.Hidden;
            this.SRLabel.IsEnabled = false;
            this.SRText.Visibility = Visibility.Visible;
        }
        private void SRLabel_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.SRLabel.IsEnabled && !this.SRLabel.Content.Equals(this.SRText.Text))
            {
                this.SRLabel.Content = this.SRText.Text;
                this.AllPreferences.SaveSubreddit(this.SRText.Text, false);
                this.SRLabel.Visibility = Visibility.Visible;
            }
        }

        private void SRText_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.SRText.Text))
            {
                this.SRText.SelectAll();
            }
        }
        private void SRText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                e.Handled = true;
                if (string.IsNullOrWhiteSpace(this.SRText.Text))
                    this.SRText.Text = RedditFeed.Resources.DefaultSubreddit;

                this.SRText.Visibility = Visibility.Hidden;
                this.SRLabel.IsEnabled = true;
            }
        }
        private void SRText_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.SRText.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                this.SRText.Focus();
            }
        }
    }
}
