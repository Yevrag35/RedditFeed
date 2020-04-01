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
//using System.Windows.Threading;

//using Thread = System.Threading.Thread;

namespace RedditFeed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private static readonly string[] DOTS = new string[3]
        //{
        //    ".",
        //    "..",
        //    "..."
        //};
        private static bool _isLoading = true;
        //private const string LOADING_FORMAT = "Loading{0}";

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
            //this.Posts.CollectionChanged += this.OnFeedChange;

            //this.Posts.AddSortDescription(x => x.Updated, ListSortDirection.Descending);

            this.RedditList.ItemsSource = this.Posts.View;

            _isLoading = false;
        }

        #region LOAD FEED
        //private async void LoadingLbl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (this.LoadingLbl.Visibility == Visibility.Visible)
        //    {
        //        _isLoading = true;
        //        this.LoadingLbl.Content = string.Format(LOADING_FORMAT, string.Empty);
        //        await this.LoopDots();
        //    }
        //}
        //private async Task LoopDots()
        //{
        //    //await this.Dispatcher.InvokeAsync(() =>
        //    //{
        //    //    
        //    //}, DispatcherPriority.Background);
        //    while (_isLoading)
        //    {
        //        for (int i = 0; i < DOTS.Length; i++)
        //        {
        //            ((MainWindow)Application.Current.MainWindow).LoadingLbl.Content = string.Format(LOADING_FORMAT, DOTS[i]);
        //            Thread.Sleep(800);
        //        }
        //    }

        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        ((MainWindow)Application.Current.MainWindow).LoadingLbl.Visibility = Visibility.Hidden;
        //    });
        //}
        private async Task ReloadAsync(Uri subreddit, PostRange range)
        {
            List<Post> posts = await ((MainWindow)Application.Current.MainWindow).Posts.ReloadFeed(
                subreddit,
                range
            );
            //_isLoading = false;
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

        private async void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            //this.LoadingLbl.Visibility = Visibility.Visible;
            this.ReloadBtn.IsEnabled = false;
            await this.ReloadAsync(this.AllPreferences.Preferences.SubredditUrl, this.AllPreferences.Preferences.Range);
            GC.Collect();
        }

        
    }
}
