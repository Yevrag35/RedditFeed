using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfAnimatedGif;

namespace RedditFeed
{
    public partial class MainWindow
    {
        private ImageAnimationController GifController { get; set; }

        private void LoadingGif_AnimationLoaded(object sender, RoutedEventArgs e)
        {
            this.GifController = ImageBehavior.GetAnimationController(this.LoadingGif);
        }

        private void ReloadBtn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.ReloadBtn.IsEnabled)
            {
                this.ReloadBtn.Visibility = Visibility.Hidden;
                this.ReloadBtnLoading.Visibility = Visibility.Visible;
            }
            else
            {
                this.ReloadBtnLoading.Visibility = Visibility.Hidden;
                this.ReloadBtn.Visibility = Visibility.Visible;
            }
        }
        private void ReloadBtnLoading_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.ReloadBtnLoading.Visibility == Visibility.Hidden)
            {
                this.ReloadBtn.IsEnabled = true;
                this.GifController.Pause();
            }
            else
            {
                this.ReloadBtn.IsEnabled = false;
                this.GifController.Play();
            }
        }
    }
}
