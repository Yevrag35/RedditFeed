using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RedditFeed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static string DateFormat;
        private static bool _isLoading = true;

        internal JsonSettings AllPreferences;
        internal PostCollection Posts;

        public string SubredditLabel
        {
            get => this.AllPreferences?.Preferences?.Subreddit;
            set { return; }
        }

        public MainWindow()
        {
            this.AllPreferences = new JsonSettings(true);
            DateFormat = this.AllPreferences.Preferences.DateTimeFormat;

            this.InitializeComponent();
            this.SRText.Text = this.AllPreferences.Preferences.Subreddit;

            this.Posts = PostCollection.LoadFeed(
                this.AllPreferences.Preferences.SubredditUrl,
                this.AllPreferences.Preferences.Range
            );

            this.RedditList.ItemsSource = this.Posts.View;
            if (this.AllPreferences.Preferences.HideAuthorColumn)
                ((GridView)this.RedditList.View).Columns.Remove(this.AuthorColumn);

            _isLoading = false;
        }

        #region LOAD FEED
        private async Task ReloadAsync(Uri subreddit, PostRange range)
        {
            List<Post> posts = await ((MainWindow)Application.Current.MainWindow).Posts.ReloadFeed(
                subreddit,
                range
            );

            await this.Dispatcher.InvokeAsync(() =>
            {
                ((MainWindow)Application.Current.MainWindow).Posts.Clear();
                foreach (Post p in posts)
                {
                    ((MainWindow)Application.Current.MainWindow).Posts.Add(p);
                }
                ((MainWindow)Application.Current.MainWindow).ReloadBtn.IsEnabled = true;
            });
        }
        private async void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ReloadBtn.IsEnabled = false;
            await this.ReloadAsync(this.AllPreferences.Preferences.SubredditUrl, this.AllPreferences.Preferences.Range);
            GC.Collect();
        }

        #endregion

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.RedditList.SelectedItem is Post post)
            {
                post.GoTo();
            }
        }
      
        private async void SRLabel_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.SRLabel.IsEnabled && !this.SRLabel.Content.Equals(this.SRText.Text))
            {
                this.SRLabel.Content = this.SRText.Text;
                this.AllPreferences.SaveSubreddit(this.SRText.Text, false);
                this.SRLabel.Visibility = Visibility.Visible;

                await this.ReloadAsync(this.AllPreferences.Preferences.SubredditUrl, this.AllPreferences.Preferences.Range);
            }
        }
        private void SRLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.SRLabel.Visibility = Visibility.Hidden;
            this.SRLabel.IsEnabled = false;
            this.SRText.Visibility = Visibility.Visible;
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
