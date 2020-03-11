using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        //internal PostCollection Posts;
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
            this.Posts = PostCollection.LoadFeed(this.AllPreferences.Preferences.SubredditUrl, this.AllPreferences.Preferences.Range);
            this.Posts.CollectionChanged += this.OnFeedChange;

            //this.Posts.AddSortDescription(x => x.Updated, ListSortDirection.Descending);

            this.RedditList.ItemsSource = this.Posts.View;

            _isLoading = false;
        }

        #region LOAD FEED
        private void OnFeedChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Dispatcher.Invoke(() =>
                {
                    ((MainWindow)Application.Current.MainWindow).RedditList.Items.Refresh();
                });
                
                //((MainWindow)Application.Current.MainWindow).RedditList.ItemsSource = ((MainWindow)Application.Current.MainWindow).Posts.View;
                //((MainWindow)Application.Current.MainWindow).RedditList.Items.Refresh();
            }
        }

        private void TriggerBtn_Click(object sender, RoutedEventArgs e)
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
        private async void SRLabel_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.SRLabel.IsEnabled && !this.SRLabel.Content.Equals(this.SRText.Text))
            {
                this.SRLabel.Content = this.SRText.Text;
                this.AllPreferences.SaveSubreddit(this.SRText.Text, false);
                this.SRLabel.Visibility = Visibility.Visible;

                List<Post> posts = await ((MainWindow)Application.Current.MainWindow).Posts.ReloadFeed(
                    ((MainWindow)Application.Current.MainWindow).AllPreferences.Preferences.SubredditUrl,
                    ((MainWindow)Application.Current.MainWindow).AllPreferences.Preferences.Range
                );
                await this.Dispatcher.InvokeAsync(() =>
                {
                    ((MainWindow)Application.Current.MainWindow).Posts.Clear();
                    foreach (Post p in posts)
                    {
                        ((MainWindow)Application.Current.MainWindow).Posts.Add(p);
                    }
                    //((MainWindow)Application.Current.MainWindow).TriggerBtn.RaiseEvent(click);
                });
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
